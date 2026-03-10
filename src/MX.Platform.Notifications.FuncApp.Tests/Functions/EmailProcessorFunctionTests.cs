using Microsoft.Extensions.Logging;

using Moq;

using MX.Platform.Notifications.Abstractions.V1.Models;
using MX.Platform.Notifications.Api.Client.Testing.Factories;
using MX.Platform.Notifications.FuncApp.Functions;
using MX.Platform.Notifications.FuncApp.Services;

using Newtonsoft.Json;

namespace MX.Platform.Notifications.FuncApp.Tests.Functions;

public class EmailProcessorFunctionTests
{
    private readonly Mock<ILogger<EmailProcessorFunction>> _loggerMock;
    private readonly Mock<IEmailSenderService> _emailSenderServiceMock;
    private readonly EmailProcessorFunction _sut;

    public EmailProcessorFunctionTests()
    {
        _loggerMock = new Mock<ILogger<EmailProcessorFunction>>();
        _emailSenderServiceMock = new Mock<IEmailSenderService>();
        _sut = new EmailProcessorFunction(_loggerMock.Object, _emailSenderServiceMock.Object);
    }

    [Fact]
    public async Task Run_WithValidMessage_CallsSendEmailAsync()
    {
        // Arrange
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(
            subject: "Welcome",
            senderDomain: "contoso.com");
        var message = JsonConvert.SerializeObject(request);

        _emailSenderServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<SendEmailRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SendEmailRequestDtoFactory.CreateSendEmailResponse("msg-123", "Succeeded"));

        // Act
        await _sut.Run(message);

        // Assert
        _emailSenderServiceMock.Verify(
            s => s.SendEmailAsync(
                It.Is<SendEmailRequestDto>(r =>
                    r.Subject == "Welcome" &&
                    r.SenderDomain == "contoso.com"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WithValidMessage_DeserializesAllFields()
    {
        // Arrange
        var to = new List<EmailRecipientDto>
        {
            SendEmailRequestDtoFactory.CreateRecipient("user@test.com", "User")
        };
        var cc = new List<EmailRecipientDto>
        {
            SendEmailRequestDtoFactory.CreateRecipient("cc@test.com", "CC User")
        };
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(
            senderDomain: "example.com",
            senderUsername: "support",
            subject: "Full Test",
            htmlBody: "<p>Hello</p>",
            plainTextBody: "Hello",
            to: to,
            cc: cc);
        var message = JsonConvert.SerializeObject(request);

        _emailSenderServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<SendEmailRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SendEmailRequestDtoFactory.CreateSendEmailResponse());

        // Act
        await _sut.Run(message);

        // Assert
        _emailSenderServiceMock.Verify(
            s => s.SendEmailAsync(
                It.Is<SendEmailRequestDto>(r =>
                    r.SenderDomain == "example.com" &&
                    r.SenderUsername == "support" &&
                    r.Subject == "Full Test" &&
                    r.HtmlBody == "<p>Hello</p>" &&
                    r.PlainTextBody == "Hello" &&
                    r.To.Count == 1 &&
                    r.To[0].EmailAddress == "user@test.com" &&
                    r.Cc != null && r.Cc.Count == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WithInvalidJson_ThrowsInvalidOperationException()
    {
        // Arrange
        var invalidMessage = "this is not valid json";

        // Act & Assert
        await Assert.ThrowsAsync<JsonReaderException>(() => _sut.Run(invalidMessage));
    }

    [Fact]
    public async Task Run_WithNullDeserializationResult_ThrowsInvalidOperationException()
    {
        // Arrange
        var nullMessage = "null";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Run(nullMessage));
    }

    [Fact]
    public async Task Run_WithEmptyJsonObject_CallsSendEmailAsync()
    {
        // Arrange
        var emptyObject = "{}";

        _emailSenderServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<SendEmailRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SendEmailRequestDtoFactory.CreateSendEmailResponse());

        // Act — empty JSON object deserializes to a valid DTO with default values
        await _sut.Run(emptyObject);

        // Assert
        _emailSenderServiceMock.Verify(
            s => s.SendEmailAsync(It.IsAny<SendEmailRequestDto>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Run_WhenSendEmailThrows_ExceptionPropagates()
    {
        // Arrange
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest();
        var message = JsonConvert.SerializeObject(request);

        _emailSenderServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<SendEmailRequestDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("ACS send failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.Run(message));
        Assert.Equal("ACS send failed", exception.Message);
    }

    [Fact]
    public async Task Run_WithValidMessage_LogsProcessingAndCompletionInfo()
    {
        // Arrange
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(subject: "Log Test");
        var message = JsonConvert.SerializeObject(request);

        _emailSenderServiceMock
            .Setup(s => s.SendEmailAsync(It.IsAny<SendEmailRequestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(SendEmailRequestDtoFactory.CreateSendEmailResponse("log-msg", "Succeeded"));

        // Act
        await _sut.Run(message);

        // Assert — verify logging was called (processing + processed = at least 2 info logs)
        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeast(2));
    }
}
