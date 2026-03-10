namespace MX.Platform.Notifications.Abstractions.V1.Interfaces;

/// <summary>
/// Unified client interface for the Platform Notifications service.
/// Register via <c>services.AddNotificationsApiClient()</c> from the Api.Client.V1 package.
/// </summary>
public interface INotificationsApiClient
{
    /// <summary>
    /// Gets the email sending API.
    /// </summary>
    ISendEmailApi Email { get; }
}
