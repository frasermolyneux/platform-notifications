using System.Net;

using Azure.Messaging.ServiceBus;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MX.Platform.Notifications.FuncApp.Functions;

public class ReprocessDlqFunction(
    ILogger<ReprocessDlqFunction> logger,
    IConfiguration configuration)
{
    private const string ReprocessCountProperty = "dlq-reprocess-count";
    private const int MaxReprocessAttempts = 3;

    [Function(nameof(ReprocessDlqFunction))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/admin/reprocess-dlq")] HttpRequestData req,
        FunctionContext context)
    {
        var queueName = req.Query["queueName"];
        if (string.IsNullOrEmpty(queueName))
        {
            var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResponse.WriteStringAsync("Please pass a queueName on the query string").ConfigureAwait(false);
            return badResponse;
        }

        var serviceBusNamespace = configuration["ServiceBusConnection__fullyQualifiedNamespace"];
        if (string.IsNullOrWhiteSpace(serviceBusNamespace))
        {
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync("Service Bus connection is not configured").ConfigureAwait(false);
            return errorResponse;
        }

        int reprocessed = 0;
        int skippedPoisonPill = 0;

        try
        {
            await using var client = new ServiceBusClient(serviceBusNamespace, new Azure.Identity.DefaultAzureCredential());
            await using var sender = client.CreateSender(queueName);
            await using var receiver = client.CreateReceiver(queueName, new ServiceBusReceiverOptions
            {
                SubQueue = SubQueue.DeadLetter,
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            });

            (reprocessed, skippedPoisonPill) = await ProcessDeadLetterMessagesAsync(sender, receiver).ConfigureAwait(false);
        }
        catch (ServiceBusException ex) when (ex.Reason == ServiceBusFailureReason.MessagingEntityNotFound)
        {
            logger.LogError(ex, "Queue '{QueueName}' not found", queueName);
            var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
            await notFoundResponse.WriteStringAsync($"Queue '{queueName}' not found").ConfigureAwait(false);
            return notFoundResponse;
        }

        logger.LogInformation(
            "DLQ reprocessing complete for '{QueueName}'. Reprocessed: {Reprocessed}, Skipped (poison): {Skipped}",
            queueName, reprocessed, skippedPoisonPill);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync($"Reprocessed: {reprocessed}, Skipped (poison-pill): {skippedPoisonPill}").ConfigureAwait(false);
        return response;
    }

    private async Task<(int reprocessed, int skipped)> ProcessDeadLetterMessagesAsync(
        ServiceBusSender sender, ServiceBusReceiver receiver)
    {
        const int fetchCount = 150;
        int totalReprocessed = 0;
        int totalSkipped = 0;

        IReadOnlyList<ServiceBusReceivedMessage> dlqMessages;
        do
        {
            dlqMessages = await receiver.ReceiveMessagesAsync(fetchCount).ConfigureAwait(false);

            logger.LogInformation("DLQ batch received: {Count} messages", dlqMessages.Count);

            foreach (var dlqMessage in dlqMessages)
            {
                int reprocessCount = dlqMessage.ApplicationProperties.TryGetValue(ReprocessCountProperty, out var count)
                    ? (int)count
                    : 0;

                if (reprocessCount >= MaxReprocessAttempts)
                {
                    logger.LogWarning(
                        "Skipping poison-pill message {MessageId} — already reprocessed {Count} times. DeadLetterReason: {Reason}",
                        dlqMessage.MessageId, reprocessCount, dlqMessage.DeadLetterReason);
                    await receiver.AbandonMessageAsync(dlqMessage).ConfigureAwait(false);
                    totalSkipped++;
                    continue;
                }

                var newMessage = new ServiceBusMessage(dlqMessage);
                newMessage.ApplicationProperties[ReprocessCountProperty] = reprocessCount + 1;

                try
                {
                    await sender.SendMessageAsync(newMessage).ConfigureAwait(false);
                    await receiver.CompleteMessageAsync(dlqMessage).ConfigureAwait(false);
                    totalReprocessed++;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to reprocess message {MessageId}, abandoning for retry", dlqMessage.MessageId);
                    await receiver.AbandonMessageAsync(dlqMessage).ConfigureAwait(false);
                }
            }
        } while (dlqMessages.Count > 0);

        return (totalReprocessed, totalSkipped);
    }
}
