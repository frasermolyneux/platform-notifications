using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MX.Platform.Notifications.Abstractions.V1.Interfaces;
using MX.Platform.Notifications.Api.Client.Testing.Fakes;

namespace MX.Platform.Notifications.Api.Client.Testing;

/// <summary>
/// DI extensions for registering fake notification services in integration tests.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Replaces the real <see cref="INotificationsApiClient"/> and all related services
    /// with in-memory fakes. Use the optional <paramref name="configure"/> callback to
    /// set up canned responses.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddFakeNotificationsApiClient(client =>
    /// {
    ///     // Emails will be tracked in client.EmailApi.SentEmails
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddFakeNotificationsApiClient(
        this IServiceCollection services,
        Action<FakeNotificationsApiClient>? configure = null)
    {
        var fakeClient = new FakeNotificationsApiClient();
        configure?.Invoke(fakeClient);

        services.RemoveAll<INotificationsApiClient>();
        services.RemoveAll<ISendEmailApi>();

        services.AddSingleton<INotificationsApiClient>(fakeClient);
        services.AddSingleton<ISendEmailApi>(fakeClient.EmailApi);

        return services;
    }
}
