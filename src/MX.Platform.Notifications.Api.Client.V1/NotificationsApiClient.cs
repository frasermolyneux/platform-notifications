using MX.Platform.Notifications.Abstractions.V1.Interfaces;

namespace MX.Platform.Notifications.Api.Client.V1;

/// <summary>
/// Implementation of the Notifications API client that provides access to API endpoints
/// </summary>
public class NotificationsApiClient : INotificationsApiClient
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationsApiClient"/> class
    /// </summary>
    /// <param name="sendEmailApi">The send email API</param>
    public NotificationsApiClient(ISendEmailApi sendEmailApi)
    {
        Email = sendEmailApi;
    }

    /// <summary>
    /// Gets the send email API
    /// </summary>
    public ISendEmailApi Email { get; }
}
