namespace MX.Platform.Notifications.Abstractions.V1.Interfaces;

public interface INotificationsApiClient
{
    ISendEmailApi Email { get; }
}
