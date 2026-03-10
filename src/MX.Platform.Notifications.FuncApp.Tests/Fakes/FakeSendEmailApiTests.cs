using System.Net;

using MX.Platform.Notifications.Api.Client.Testing.Factories;
using MX.Platform.Notifications.Api.Client.Testing.Fakes;

namespace MX.Platform.Notifications.FuncApp.Tests.Fakes;

public class FakeSendEmailApiTests
{
    private readonly FakeSendEmailApi _sut;

    public FakeSendEmailApiTests()
    {
        _sut = new FakeSendEmailApi();
    }

    [Fact]
    public async Task SendEmail_ByDefault_ReturnsSuccessResult()
    {
        // Arrange
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest();

        // Act
        var result = await _sut.SendEmail(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Result?.Data);
        Assert.Equal("Succeeded", result.Result.Data.Status);
    }

    [Fact]
    public async Task SendEmail_TracksRequestInSentEmails()
    {
        // Arrange
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(subject: "Tracked Email");

        // Act
        await _sut.SendEmail(request);

        // Assert
        var sent = Assert.Single(_sut.SentEmails);
        Assert.Equal("Tracked Email", sent.Subject);
    }

    [Fact]
    public async Task SendEmail_MultipleCalls_TracksAllRequests()
    {
        // Arrange
        var request1 = SendEmailRequestDtoFactory.CreateSendEmailRequest(subject: "First");
        var request2 = SendEmailRequestDtoFactory.CreateSendEmailRequest(subject: "Second");
        var request3 = SendEmailRequestDtoFactory.CreateSendEmailRequest(subject: "Third");

        // Act
        await _sut.SendEmail(request1);
        await _sut.SendEmail(request2);
        await _sut.SendEmail(request3);

        // Assert
        Assert.Equal(3, _sut.SentEmails.Count);
    }

    [Fact]
    public async Task SendEmail_WithConfiguredSuccessResponse_ReturnsConfiguredResponse()
    {
        // Arrange
        var customResponse = SendEmailRequestDtoFactory.CreateSendEmailResponse(
            messageId: "custom-123",
            status: "Queued");
        _sut.WithSuccessResponse(customResponse);
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest();

        // Act
        var result = await _sut.SendEmail(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("custom-123", result.Result!.Data.MessageId);
        Assert.Equal("Queued", result.Result.Data.Status);
    }

    [Fact]
    public async Task SendEmail_WithConfiguredFailure_ReturnsFailureResult()
    {
        // Arrange
        _sut.WithFailure(
            statusCode: HttpStatusCode.TooManyRequests,
            errorCode: "RATE_LIMITED",
            errorMessage: "Too many requests");
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest();

        // Act
        var result = await _sut.SendEmail(request);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.TooManyRequests, result.StatusCode);
    }

    [Fact]
    public async Task SendEmail_WithConfiguredFailure_StillTracksRequest()
    {
        // Arrange
        _sut.WithFailure();
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(subject: "Failed but tracked");

        // Act
        await _sut.SendEmail(request);

        // Assert
        var sent = Assert.Single(_sut.SentEmails);
        Assert.Equal("Failed but tracked", sent.Subject);
    }

    [Fact]
    public async Task Reset_ClearsSentEmails()
    {
        // Arrange
        await _sut.SendEmail(SendEmailRequestDtoFactory.CreateSendEmailRequest());
        await _sut.SendEmail(SendEmailRequestDtoFactory.CreateSendEmailRequest());
        Assert.Equal(2, _sut.SentEmails.Count);

        // Act
        _sut.Reset();

        // Assert
        Assert.Empty(_sut.SentEmails);
    }

    [Fact]
    public async Task Reset_ResetsToSuccessBehavior()
    {
        // Arrange
        _sut.WithFailure();
        var failRequest = SendEmailRequestDtoFactory.CreateSendEmailRequest();
        var failResult = await _sut.SendEmail(failRequest);
        Assert.False(failResult.IsSuccess);

        // Act
        _sut.Reset();

        // Assert
        var successResult = await _sut.SendEmail(SendEmailRequestDtoFactory.CreateSendEmailRequest());
        Assert.True(successResult.IsSuccess);
    }

    [Fact]
    public async Task WithSuccessResponse_ReturnsSameInstance_ForFluent()
    {
        // Arrange & Act
        var returned = _sut.WithSuccessResponse(SendEmailRequestDtoFactory.CreateSendEmailResponse());

        // Assert
        Assert.Same(_sut, returned);
    }

    [Fact]
    public async Task WithFailure_ReturnsSameInstance_ForFluent()
    {
        // Arrange & Act
        var returned = _sut.WithFailure();

        // Assert
        Assert.Same(_sut, returned);
    }
}
