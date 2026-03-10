using System.Reflection;

using Azure.Communication.Email;

using Moq;

namespace MX.Platform.Notifications.FuncApp.Tests;

/// <summary>
/// Helper for creating mock EmailSendOperation instances in tests.
/// </summary>
internal static class EmailSendOperationHelper
{
    public static EmailSendOperation CreateSuccessOperation(string operationId, EmailSendStatus status)
    {
        var result = CreateEmailSendResult(operationId, status);

        var mockOperation = new Mock<EmailSendOperation>();
        mockOperation.Setup(o => o.Id).Returns(operationId);
        mockOperation.Setup(o => o.HasValue).Returns(true);
        mockOperation.Setup(o => o.Value).Returns(result);
        return mockOperation.Object;
    }

    public static EmailSendOperation CreateNoValueOperation(string operationId)
    {
        var mockOperation = new Mock<EmailSendOperation>();
        mockOperation.Setup(o => o.Id).Returns(operationId);
        mockOperation.Setup(o => o.HasValue).Returns(false);
        mockOperation.Setup(o => o.Value).Throws(new InvalidOperationException("Operation has no value"));
        return mockOperation.Object;
    }

    private static EmailSendResult CreateEmailSendResult(string id, EmailSendStatus status)
    {
        // EmailSendResult has an internal constructor: EmailSendResult(string id, EmailSendStatus status, ErrorDetail error)
        var ctor = typeof(EmailSendResult).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
            .First(c => c.GetParameters().Length == 3);
        return (EmailSendResult)ctor.Invoke([id, status, null]);
    }
}
