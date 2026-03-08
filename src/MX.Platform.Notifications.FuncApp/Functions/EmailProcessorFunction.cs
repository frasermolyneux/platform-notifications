using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using MX.Platform.Notifications.Abstractions.V1.Models;
using MX.Platform.Notifications.FuncApp.Services;

namespace MX.Platform.Notifications.FuncApp.Functions;

public class EmailProcessorFunction(
    ILogger<EmailProcessorFunction> logger,
    IEmailSenderService emailSenderService)
{
    [Function(nameof(EmailProcessorFunction))]
    public async Task Run(
        [ServiceBusTrigger("email_send_queue", Connection = "ServiceBusConnection")]
        string message)
    {
        var emailRequest = JsonConvert.DeserializeObject<SendEmailRequestDto>(message);

        if (emailRequest is null)
        {
            logger.LogError("Failed to deserialize email request from queue message");
            throw new InvalidOperationException("Failed to deserialize email request");
        }

        logger.LogInformation(
            "Processing email. Subject: {Subject}, To: {RecipientCount} recipients",
            emailRequest.Subject,
            emailRequest.To.Count);

        var result = await emailSenderService.SendEmailAsync(emailRequest).ConfigureAwait(false);

        logger.LogInformation(
            "Email processed. MessageId: {MessageId}, Status: {Status}",
            result.MessageId,
            result.Status);
    }
}
