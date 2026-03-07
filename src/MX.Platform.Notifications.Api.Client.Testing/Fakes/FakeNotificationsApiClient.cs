using MX.Platform.Notifications.Abstractions.V1.Interfaces;

namespace MX.Platform.Notifications.Api.Client.Testing.Fakes;

/// <summary>
/// In-memory fake of <see cref="INotificationsApiClient"/> for unit and integration tests.
/// Eliminates the need for nested mock hierarchies.
/// </summary>
/// <example>
/// <code>
/// var fakeClient = new FakeNotificationsApiClient();
/// // Send an email - it will be tracked in fakeClient.EmailApi.SentEmails
/// </code>
/// </example>
public class FakeNotificationsApiClient : INotificationsApiClient
{
    /// <summary>
    /// The send email API fake. Use to configure canned responses and inspect sent emails.
    /// </summary>
    public FakeSendEmailApi EmailApi { get; } = new();

    public ISendEmailApi Email => EmailApi;

    /// <summary>
    /// Resets all fakes to their initial state, clearing configured responses
    /// and tracking state.
    /// </summary>
    public FakeNotificationsApiClient Reset()
    {
        EmailApi.Reset();
        return this;
    }
}
