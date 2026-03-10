using Azure;
using Azure.Communication.Email;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;

using Moq;

using MX.Platform.Notifications.Abstractions.V1.Models;
using MX.Platform.Notifications.Api.Client.Testing.Factories;
using MX.Platform.Notifications.FuncApp.Services;

namespace MX.Platform.Notifications.FuncApp.Tests.Services;

public class EmailSenderServiceTests
{
    private readonly Mock<EmailClient> _emailClientMock;
    private readonly Mock<ILogger<EmailSenderService>> _loggerMock;
    private readonly TelemetryClient _telemetryClient;
    private readonly List<ITelemetry> _telemetryItems;
    private readonly EmailSenderService _sut;

    public EmailSenderServiceTests()
    {
        _emailClientMock = new Mock<EmailClient>();
        _loggerMock = new Mock<ILogger<EmailSenderService>>();
        _telemetryItems = [];
        var mockChannel = new Mock<ITelemetryChannel>();
        mockChannel.Setup(c => c.Send(It.IsAny<ITelemetry>()))
            .Callback<ITelemetry>(t => _telemetryItems.Add(t));
        var telemetryConfig = new TelemetryConfiguration
        {
            TelemetryChannel = mockChannel.Object
        };
        _telemetryClient = new TelemetryClient(telemetryConfig);
        _sut = new EmailSenderService(_emailClientMock.Object, _loggerMock.Object, _telemetryClient);
    }

    [Fact]
    public void Constructor_ShouldNotThrow()
    {
        // Arrange & Act
        var exception = Record.Exception(() => new EmailSenderService(
            _emailClientMock.Object,
            _loggerMock.Object,
            _telemetryClient));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public async Task SendEmailAsync_WithValidRequest_SendsEmailAndReturnsResponse()
    {
        // Arrange
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(
            senderDomain: "contoso.com",
            senderUsername: "noreply",
            subject: "Welcome",
            htmlBody: "<p>Hello</p>",
            plainTextBody: "Hello");

        var expectedMessageId = "op-id-123";
        SetupEmailClientSuccess(expectedMessageId, EmailSendStatus.Succeeded);

        // Act
        var result = await _sut.SendEmailAsync(request);

        // Assert
        Assert.Equal(expectedMessageId, result.MessageId);
        Assert.Equal("Succeeded", result.Status);
        _emailClientMock.Verify(c => c.SendAsync(
            It.IsAny<WaitUntil>(),
            It.Is<EmailMessage>(m => m.SenderAddress == "noreply@contoso.com"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WithMultipleRecipients_BuildsCorrectEmailMessage()
    {
        // Arrange
        var to = new List<EmailRecipientDto>
        {
            SendEmailRequestDtoFactory.CreateRecipient("user1@test.com", "User One"),
            SendEmailRequestDtoFactory.CreateRecipient("user2@test.com", "User Two")
        };
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(to: to);
        SetupEmailClientSuccess("msg-1", EmailSendStatus.Succeeded);

        // Act
        var result = await _sut.SendEmailAsync(request);

        // Assert
        Assert.Equal("Succeeded", result.Status);
        _emailClientMock.Verify(c => c.SendAsync(
            It.IsAny<WaitUntil>(),
            It.Is<EmailMessage>(m => m.Recipients.To.Count == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WithCcAndBcc_IncludesCcAndBccRecipients()
    {
        // Arrange
        var cc = new List<EmailRecipientDto> { SendEmailRequestDtoFactory.CreateRecipient("cc@test.com") };
        var bcc = new List<EmailRecipientDto> { SendEmailRequestDtoFactory.CreateRecipient("bcc@test.com") };
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(cc: cc, bcc: bcc);
        SetupEmailClientSuccess("msg-2", EmailSendStatus.Succeeded);

        // Act
        var result = await _sut.SendEmailAsync(request);

        // Assert
        Assert.Equal("Succeeded", result.Status);
        _emailClientMock.Verify(c => c.SendAsync(
            It.IsAny<WaitUntil>(),
            It.Is<EmailMessage>(m =>
                m.Recipients.CC.Count == 1 &&
                m.Recipients.BCC.Count == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WithNullCcAndBcc_DoesNotAddCcOrBcc()
    {
        // Arrange
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(cc: null, bcc: null);
        SetupEmailClientSuccess("msg-3", EmailSendStatus.Succeeded);

        // Act
        var result = await _sut.SendEmailAsync(request);

        // Assert
        Assert.Equal("Succeeded", result.Status);
        _emailClientMock.Verify(c => c.SendAsync(
            It.IsAny<WaitUntil>(),
            It.Is<EmailMessage>(m =>
                m.Recipients.CC.Count == 0 &&
                m.Recipients.BCC.Count == 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WithHtmlBodyOnly_SetsHtmlContent()
    {
        // Arrange
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(
            htmlBody: "<h1>HTML Only</h1>",
            plainTextBody: null);
        SetupEmailClientSuccess("msg-4", EmailSendStatus.Succeeded);

        // Act
        var result = await _sut.SendEmailAsync(request);

        // Assert
        Assert.Equal("Succeeded", result.Status);
        _emailClientMock.Verify(c => c.SendAsync(
            It.IsAny<WaitUntil>(),
            It.Is<EmailMessage>(m =>
                m.Content.Html == "<h1>HTML Only</h1>" &&
                m.Content.PlainText == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WithPlainTextBodyOnly_SetsPlainTextContent()
    {
        // Arrange
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(
            htmlBody: null,
            plainTextBody: "Plain text only");
        SetupEmailClientSuccess("msg-5", EmailSendStatus.Succeeded);

        // Act
        var result = await _sut.SendEmailAsync(request);

        // Assert
        Assert.Equal("Succeeded", result.Status);
        _emailClientMock.Verify(c => c.SendAsync(
            It.IsAny<WaitUntil>(),
            It.Is<EmailMessage>(m =>
                m.Content.PlainText == "Plain text only" &&
                m.Content.Html == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_TracksTelemetryEvent_WithCorrectProperties()
    {
        // Arrange
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(
            senderDomain: "telemetry-test.com",
            subject: "Telemetry Test");
        SetupEmailClientSuccess("telemetry-msg", EmailSendStatus.Succeeded);

        // Act
        await _sut.SendEmailAsync(request);
        _telemetryClient.Flush();

        // Assert
        var eventTelemetry = Assert.Single(_telemetryItems.OfType<EventTelemetry>());
        Assert.Equal("EmailSend", eventTelemetry.Name);
        Assert.Equal("telemetry-test.com", eventTelemetry.Properties["SenderDomain"]);
        Assert.Equal("Telemetry Test", eventTelemetry.Properties["Subject"]);
        Assert.Equal("1", eventTelemetry.Properties["RecipientCount"]);
        Assert.Equal("telemetry-msg", eventTelemetry.Properties["MessageId"]);
        Assert.Equal("Succeeded", eventTelemetry.Properties["Status"]);
    }

    [Fact]
    public async Task SendEmailAsync_WhenSendFails_RetriesWithPolly()
    {
        // Arrange
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest();
        var callCount = 0;

        _emailClientMock
            .Setup(c => c.SendAsync(
                It.IsAny<WaitUntil>(),
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                if (callCount < 3)
                    throw new RequestFailedException("Transient error");

                return EmailSendOperationHelper.CreateSuccessOperation("retry-msg", EmailSendStatus.Succeeded);
            });

        // Act
        var result = await _sut.SendEmailAsync(request);

        // Assert
        Assert.Equal("retry-msg", result.MessageId);
        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task SendEmailAsync_WhenAllRetriesExhausted_ThrowsException()
    {
        // Arrange
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest();

        _emailClientMock
            .Setup(c => c.SendAsync(
                It.IsAny<WaitUntil>(),
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RequestFailedException("Persistent error"));

        // Act & Assert
        await Assert.ThrowsAsync<RequestFailedException>(() => _sut.SendEmailAsync(request));

        // Verify retries occurred (initial + 3 retries = 4 calls)
        _emailClientMock.Verify(c => c.SendAsync(
            It.IsAny<WaitUntil>(),
            It.IsAny<EmailMessage>(),
            It.IsAny<CancellationToken>()), Times.Exactly(4));
    }

    [Fact]
    public async Task SendEmailAsync_ConstructsSenderAddress_FromUsernameAndDomain()
    {
        // Arrange
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(
            senderUsername: "support",
            senderDomain: "example.org");
        SetupEmailClientSuccess("addr-msg", EmailSendStatus.Succeeded);

        // Act
        await _sut.SendEmailAsync(request);

        // Assert
        _emailClientMock.Verify(c => c.SendAsync(
            It.IsAny<WaitUntil>(),
            It.Is<EmailMessage>(m => m.SenderAddress == "support@example.org"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_SetsEmailSubject_InContent()
    {
        // Arrange
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(subject: "Important Update");
        SetupEmailClientSuccess("subj-msg", EmailSendStatus.Succeeded);

        // Act
        await _sut.SendEmailAsync(request);

        // Assert
        _emailClientMock.Verify(c => c.SendAsync(
            It.IsAny<WaitUntil>(),
            It.Is<EmailMessage>(m => m.Content.Subject == "Important Update"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendEmailAsync_WhenOperationHasNoValue_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest();

        _emailClientMock
            .Setup(c => c.SendAsync(
                It.IsAny<WaitUntil>(),
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailSendOperationHelper.CreateNoValueOperation("no-value-op"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SendEmailAsync(request));
        Assert.Contains("no result", exception.Message);
        Assert.Contains("no-value-op", exception.Message);
    }

    private void SetupEmailClientSuccess(string operationId, EmailSendStatus status)
    {
        _emailClientMock
            .Setup(c => c.SendAsync(
                It.IsAny<WaitUntil>(),
                It.IsAny<EmailMessage>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailSendOperationHelper.CreateSuccessOperation(operationId, status));
    }
}
