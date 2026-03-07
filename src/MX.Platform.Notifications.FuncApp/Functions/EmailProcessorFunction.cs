using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using MX.Platform.Notifications.Abstractions.V1.Models;
using MX.Platform.Notifications.FuncApp.Services;

namespace MX.Platform.Notifications.FuncApp.Functions;

public class EmailProcessorFunction(
    ILogger<EmailProcessorFunction> logger,
    IEmailSenderService emailSenderService,
    IClaimCheckService claimCheckService)
{
    [Function(nameof(EmailProcessorFunction))]
    public async Task Run(
        [ServiceBusTrigger("email_send_queue", Connection = "ServiceBusConnection")]
        string message)
    {
        SendEmailRequestDto? emailRequest;
        string? blobReference = null;

        // Check if this is a claim-check message
        var parsed = JsonConvert.DeserializeObject<dynamic>(message);
        if (parsed?.isClaimCheck == true)
        {
            blobReference = (string)parsed.claimCheckReference;
            logger.LogInformation("Retrieving email from claim-check blob: {Reference}", blobReference);

            try
            {
                var content = await claimCheckService.RetrieveAsync(blobReference).ConfigureAwait(false);
                emailRequest = JsonConvert.DeserializeObject<SendEmailRequestDto>(content);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to retrieve claim-check blob: {Reference}", blobReference);
                throw;
            }
        }
        else
        {
            emailRequest = JsonConvert.DeserializeObject<SendEmailRequestDto>(message);
        }

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

        // Clean up blob only after successful send
        if (blobReference is not null)
        {
            logger.LogInformation("Cleaning up claim-check blob: {Reference}", blobReference);
            await claimCheckService.DeleteAsync(blobReference).ConfigureAwait(false);
        }
    }
}
