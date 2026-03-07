using Azure.Communication.Email;

using FluentAssertions;

using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;

using Moq;

using MX.Platform.Notifications.Api.Client.Testing.Factories;
using MX.Platform.Notifications.FuncApp.Services;

namespace MX.Platform.Notifications.FuncApp.Tests.Services;

public class EmailSenderServiceTests
{
    private readonly Mock<ILogger<EmailSenderService>> _loggerMock;
    private readonly TelemetryClient _telemetryClient;

    public EmailSenderServiceTests()
    {
        _loggerMock = new Mock<ILogger<EmailSenderService>>();
        var telemetryConfig = new TelemetryConfiguration
        {
            TelemetryChannel = new Mock<ITelemetryChannel>().Object
        };
        _telemetryClient = new TelemetryClient(telemetryConfig);
    }

    [Fact]
    public void Constructor_ShouldNotThrow()
    {
        var emailClientMock = new Mock<EmailClient>();

        var act = () => new EmailSenderService(
            emailClientMock.Object,
            _loggerMock.Object,
            _telemetryClient);

        act.Should().NotThrow();
    }

    [Fact]
    public void SendEmailRequestDtoFactory_ShouldCreateValidRequest()
    {
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(
            subject: "Test Subject",
            senderDomain: "test.com");

        request.Subject.Should().Be("Test Subject");
        request.SenderDomain.Should().Be("test.com");
        request.To.Should().HaveCount(1);
        request.SenderUsername.Should().Be("noreply");
    }
}
