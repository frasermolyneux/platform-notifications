using MX.Platform.Notifications.Abstractions.V1.Interfaces;
using MX.Platform.Notifications.Api.Client.Testing.Factories;
using MX.Platform.Notifications.Api.Client.Testing.Fakes;

namespace MX.Platform.Notifications.FuncApp.Tests.Fakes;

public class FakeNotificationsApiClientTests
{
    private readonly FakeNotificationsApiClient _sut;

    public FakeNotificationsApiClientTests()
    {
        _sut = new FakeNotificationsApiClient();
    }

    [Fact]
    public void Email_ReturnsSendEmailApi()
    {
        // Arrange & Act
        var emailApi = _sut.Email;

        // Assert
        Assert.NotNull(emailApi);
        Assert.IsAssignableFrom<ISendEmailApi>(emailApi);
    }

    [Fact]
    public void EmailApi_ReturnsFakeSendEmailApi()
    {
        // Arrange & Act
        var fakeApi = _sut.EmailApi;

        // Assert
        Assert.NotNull(fakeApi);
        Assert.IsType<FakeSendEmailApi>(fakeApi);
    }

    [Fact]
    public void Email_And_EmailApi_ReturnSameInstance()
    {
        // Arrange & Act & Assert
        Assert.Same(_sut.EmailApi, _sut.Email);
    }

    [Fact]
    public void ImplementsINotificationsApiClient()
    {
        // Arrange & Act & Assert
        Assert.IsAssignableFrom<INotificationsApiClient>(_sut);
    }

    [Fact]
    public async Task Reset_ClearsEmailApiState()
    {
        // Arrange
        await _sut.Email.SendEmail(SendEmailRequestDtoFactory.CreateSendEmailRequest());
        Assert.Single(_sut.EmailApi.SentEmails);

        // Act
        _sut.Reset();

        // Assert
        Assert.Empty(_sut.EmailApi.SentEmails);
    }

    [Fact]
    public void Reset_ReturnsSameInstance_ForFluent()
    {
        // Arrange & Act
        var returned = _sut.Reset();

        // Assert
        Assert.Same(_sut, returned);
    }

    [Fact]
    public async Task CanConfigureAndUseEndToEnd()
    {
        // Arrange
        var customResponse = SendEmailRequestDtoFactory.CreateSendEmailResponse(
            messageId: "e2e-msg-123",
            status: "Succeeded");
        _sut.EmailApi.WithSuccessResponse(customResponse);

        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(
            subject: "E2E Test",
            senderDomain: "test.org");

        // Act
        var result = await _sut.Email.SendEmail(request);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("e2e-msg-123", result.Result!.Data.MessageId);
        var sentEmail = Assert.Single(_sut.EmailApi.SentEmails);
        Assert.Equal("E2E Test", sentEmail.Subject);
        Assert.Equal("test.org", sentEmail.SenderDomain);
    }
}
