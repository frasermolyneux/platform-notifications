using MX.Platform.Notifications.Abstractions.V1.Models;
using MX.Platform.Notifications.Api.Client.Testing.Factories;

namespace MX.Platform.Notifications.FuncApp.Tests.Factories;

public class SendEmailRequestDtoFactoryTests
{
    [Fact]
    public void CreateSendEmailRequest_WithDefaults_ReturnsValidRequest()
    {
        // Arrange & Act
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest();

        // Assert
        Assert.Equal("contoso.com", request.SenderDomain);
        Assert.Equal("Contoso Notifications", request.SenderDisplayName);
        Assert.Equal("noreply", request.SenderUsername);
        Assert.Equal("Test Email", request.Subject);
        Assert.Equal("<p>Test email body</p>", request.HtmlBody);
        Assert.Equal("Test email body", request.PlainTextBody);
        Assert.Single(request.To);
        Assert.Null(request.Cc);
        Assert.Null(request.Bcc);
    }

    [Fact]
    public void CreateSendEmailRequest_WithCustomValues_ReturnsRequestWithCustomValues()
    {
        // Arrange
        var to = new List<EmailRecipientDto>
        {
            SendEmailRequestDtoFactory.CreateRecipient("a@b.com"),
            SendEmailRequestDtoFactory.CreateRecipient("c@d.com")
        };
        var cc = new List<EmailRecipientDto>
        {
            SendEmailRequestDtoFactory.CreateRecipient("cc@test.com")
        };
        var bcc = new List<EmailRecipientDto>
        {
            SendEmailRequestDtoFactory.CreateRecipient("bcc@test.com")
        };

        // Act
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(
            senderDomain: "custom.org",
            senderDisplayName: "Custom Display",
            senderUsername: "info",
            subject: "Custom Subject",
            htmlBody: "<b>Bold</b>",
            plainTextBody: "Bold",
            to: to,
            cc: cc,
            bcc: bcc);

        // Assert
        Assert.Equal("custom.org", request.SenderDomain);
        Assert.Equal("Custom Display", request.SenderDisplayName);
        Assert.Equal("info", request.SenderUsername);
        Assert.Equal("Custom Subject", request.Subject);
        Assert.Equal("<b>Bold</b>", request.HtmlBody);
        Assert.Equal("Bold", request.PlainTextBody);
        Assert.Equal(2, request.To.Count);
        Assert.NotNull(request.Cc);
        Assert.Single(request.Cc);
        Assert.NotNull(request.Bcc);
        Assert.Single(request.Bcc);
    }

    [Fact]
    public void CreateSendEmailRequest_WithNullBodies_ReturnsRequestWithNullBodies()
    {
        // Arrange & Act
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest(
            htmlBody: null,
            plainTextBody: null);

        // Assert
        Assert.Null(request.HtmlBody);
        Assert.Null(request.PlainTextBody);
    }

    [Fact]
    public void CreateRecipient_WithDefaults_ReturnsValidRecipient()
    {
        // Arrange & Act
        var recipient = SendEmailRequestDtoFactory.CreateRecipient();

        // Assert
        Assert.Equal("test@example.com", recipient.EmailAddress);
        Assert.Equal("Test User", recipient.DisplayName);
    }

    [Fact]
    public void CreateRecipient_WithCustomValues_ReturnsRecipientWithCustomValues()
    {
        // Arrange & Act
        var recipient = SendEmailRequestDtoFactory.CreateRecipient(
            emailAddress: "custom@example.org",
            displayName: "Custom Name");

        // Assert
        Assert.Equal("custom@example.org", recipient.EmailAddress);
        Assert.Equal("Custom Name", recipient.DisplayName);
    }

    [Fact]
    public void CreateRecipient_WithNullDisplayName_ReturnsRecipientWithNullDisplayName()
    {
        // Arrange & Act
        var recipient = SendEmailRequestDtoFactory.CreateRecipient(
            emailAddress: "nodisplay@test.com",
            displayName: null);

        // Assert
        Assert.Equal("nodisplay@test.com", recipient.EmailAddress);
        Assert.Null(recipient.DisplayName);
    }

    [Fact]
    public void CreateSendEmailResponse_WithDefaults_ReturnsValidResponse()
    {
        // Arrange & Act
        var response = SendEmailRequestDtoFactory.CreateSendEmailResponse();

        // Assert
        Assert.Equal("test-message-id", response.MessageId);
        Assert.Equal("Succeeded", response.Status);
    }

    [Fact]
    public void CreateSendEmailResponse_WithCustomValues_ReturnsResponseWithCustomValues()
    {
        // Arrange & Act
        var response = SendEmailRequestDtoFactory.CreateSendEmailResponse(
            messageId: "custom-id-456",
            status: "Failed");

        // Assert
        Assert.Equal("custom-id-456", response.MessageId);
        Assert.Equal("Failed", response.Status);
    }

    [Fact]
    public void CreateSendEmailRequest_DefaultTo_ContainsDefaultRecipient()
    {
        // Arrange & Act
        var request = SendEmailRequestDtoFactory.CreateSendEmailRequest();

        // Assert
        var defaultRecipient = Assert.Single(request.To);
        Assert.Equal("test@example.com", defaultRecipient.EmailAddress);
        Assert.Equal("Test User", defaultRecipient.DisplayName);
    }
}
