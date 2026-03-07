using System.Net;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using MX.Platform.Notifications.Abstractions.V1.Models;
using MX.Platform.Notifications.FuncApp.Services;

namespace MX.Platform.Notifications.FuncApp.Functions;

public class SendEmailFunction(
    ILogger<SendEmailFunction> logger,
    IClaimCheckService claimCheckService)
{
    private const int ClaimCheckThresholdBytes = 200 * 1024; // 200KB

    [Function(nameof(SendEmailFunction))]
    [ServiceBusOutput("email_send_queue", Connection = "ServiceBusConnection")]
    public async Task<string?> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/email/send")] HttpRequestData req,
        FunctionContext context)
    {
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync().ConfigureAwait(false);

        SendEmailRequestDto? emailRequest;
        try
        {
            emailRequest = JsonConvert.DeserializeObject<SendEmailRequestDto>(requestBody);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SendEmail request was not in expected format");
            throw;
        }

        if (emailRequest is null)
        {
            logger.LogError("SendEmail request body was null");
            throw new InvalidOperationException("SendEmail request body was null");
        }

        if (emailRequest.To.Count == 0)
        {
            logger.LogError("SendEmail request must have at least one recipient");
            throw new ArgumentException("SendEmail request must have at least one recipient");
        }

        if (string.IsNullOrWhiteSpace(emailRequest.SenderDomain))
        {
            logger.LogError("SendEmail request must have a sender domain");
            throw new ArgumentException("SendEmail request must have a sender domain");
        }

        // Claim-check: if the serialized message exceeds threshold, store body in blob storage
        var serialized = JsonConvert.SerializeObject(emailRequest);
        if (serialized.Length > ClaimCheckThresholdBytes)
        {
            logger.LogInformation("Email body exceeds {Threshold}KB, using claim-check pattern", ClaimCheckThresholdBytes / 1024);

            var blobReference = await claimCheckService.StoreAsync(serialized).ConfigureAwait(false);

            var claimCheckMessage = new
            {
                claimCheckReference = blobReference,
                isClaimCheck = true
            };
            return JsonConvert.SerializeObject(claimCheckMessage);
        }

        logger.LogInformation(
            "Email send request queued. Subject: {Subject}, To: {RecipientCount} recipients",
            emailRequest.Subject,
            emailRequest.To.Count);

        return serialized;
    }
}
