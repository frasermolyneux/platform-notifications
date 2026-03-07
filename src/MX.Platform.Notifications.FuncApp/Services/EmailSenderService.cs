using Azure;
using Azure.Communication.Email;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;

using MX.Platform.Notifications.Abstractions.V1.Models;

using Polly;
using Polly.Retry;

namespace MX.Platform.Notifications.FuncApp.Services;

/// <summary>
/// Sends emails via Azure Communication Services with Polly retry policies.
/// </summary>
public class EmailSenderService : IEmailSenderService
{
    private readonly EmailClient _emailClient;
    private readonly ILogger<EmailSenderService> _logger;
    private readonly TelemetryClient _telemetryClient;
    private readonly ResiliencePipeline _retryPipeline;

    public EmailSenderService(
        EmailClient emailClient,
        ILogger<EmailSenderService> logger,
        TelemetryClient telemetryClient)
    {
        _emailClient = emailClient;
        _logger = logger;
        _telemetryClient = telemetryClient;

        _retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(2),
                OnRetry = args =>
                {
                    _logger.LogWarning(
                        args.Outcome.Exception,
                        "Email send retry attempt {AttemptNumber} after {Delay}ms",
                        args.AttemptNumber,
                        args.RetryDelay.TotalMilliseconds);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public async Task<SendEmailResponseDto> SendEmailAsync(SendEmailRequestDto request, CancellationToken cancellationToken = default)
    {
        var senderAddress = $"{request.SenderUsername}@{request.SenderDomain}";

        var emailContent = new EmailContent(request.Subject);
        if (!string.IsNullOrWhiteSpace(request.HtmlBody))
        {
            emailContent.Html = request.HtmlBody;
        }
        if (!string.IsNullOrWhiteSpace(request.PlainTextBody))
        {
            emailContent.PlainText = request.PlainTextBody;
        }

        var recipients = new EmailRecipients(
            request.To.Select(t => new EmailAddress(t.EmailAddress, t.DisplayName)));

        if (request.Cc is not null)
        {
            foreach (var cc in request.Cc)
            {
                recipients.CC.Add(new EmailAddress(cc.EmailAddress, cc.DisplayName));
            }
        }

        if (request.Bcc is not null)
        {
            foreach (var bcc in request.Bcc)
            {
                recipients.BCC.Add(new EmailAddress(bcc.EmailAddress, bcc.DisplayName));
            }
        }

        var emailMessage = new EmailMessage(senderAddress, recipients, emailContent);

        var telemetry = new EventTelemetry("EmailSend")
        {
            Properties =
            {
                ["SenderDomain"] = request.SenderDomain,
                ["Subject"] = request.Subject,
                ["RecipientCount"] = request.To.Count.ToString()
            }
        };

        var result = await _retryPipeline.ExecuteAsync(async ct =>
        {
            var emailSendOperation = await _emailClient.SendAsync(
                WaitUntil.Completed,
                emailMessage,
                ct).ConfigureAwait(false);

            return emailSendOperation;
        }, cancellationToken).ConfigureAwait(false);

        telemetry.Properties["MessageId"] = result.Id;

        if (!result.HasValue)
        {
            throw new InvalidOperationException($"Email send operation completed but returned no result. OperationId: {result.Id}");
        }

        telemetry.Properties["Status"] = result.Value.Status.ToString();
        _telemetryClient.TrackEvent(telemetry);

        _logger.LogInformation(
            "Email sent successfully. MessageId: {MessageId}, Status: {Status}",
            result.Id,
            result.Value.Status);

        return new SendEmailResponseDto
        {
            MessageId = result.Id,
            Status = result.Value.Status.ToString()
        };
    }
}
