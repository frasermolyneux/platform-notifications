using Azure;
using Azure.Communication.Email;

using Microsoft.Extensions.Logging;

using MX.Observability.ApplicationInsights.Auditing;
using MX.Observability.ApplicationInsights.Auditing.Models;
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
    private readonly IAuditLogger _auditLogger;
    private readonly ResiliencePipeline _retryPipeline;

    public EmailSenderService(
        EmailClient emailClient,
        ILogger<EmailSenderService> logger,
        IAuditLogger auditLogger)
    {
        _emailClient = emailClient;
        _logger = logger;
        _auditLogger = auditLogger;

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

        var result = await _retryPipeline.ExecuteAsync(async ct =>
        {
            var emailSendOperation = await _emailClient.SendAsync(
                WaitUntil.Completed,
                emailMessage,
                ct).ConfigureAwait(false);

            return emailSendOperation;
        }, cancellationToken).ConfigureAwait(false);

        if (!result.HasValue)
        {
            throw new InvalidOperationException($"Email send operation completed but returned no result. OperationId: {result.Id}");
        }

        _auditLogger.LogAudit(AuditEvent.SystemAction("EmailDispatched", AuditAction.Create)
            .WithTarget(result.Id, "Email")
            .WithSource(nameof(EmailSenderService))
            .WithProperty("SenderDomain", request.SenderDomain)
            .WithProperty("Subject", request.Subject)
            .WithProperty("RecipientCount", request.To.Count.ToString())
            .WithProperty("Status", result.Value.Status.ToString())
            .Build());

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
