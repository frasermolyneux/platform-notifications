using Microsoft.Extensions.DependencyInjection;

using MX.Platform.Notifications.Abstractions.V1.Interfaces;

using MX.Api.Client.Extensions;

namespace MX.Platform.Notifications.Api.Client.V1;

/// <summary>
/// Extension methods for configuring Notifications API client services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Notifications API client services with custom configuration
    /// </summary>
    /// <param name="serviceCollection">The service collection</param>
    /// <param name="configureOptions">Action to configure the Notifications API options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddNotificationsApiClient(
        this IServiceCollection serviceCollection,
        Action<NotificationsApiOptionsBuilder> configureOptions)
    {
        serviceCollection.AddTypedApiClient<ISendEmailApi, SendEmailApi, NotificationsApiClientOptions, NotificationsApiOptionsBuilder>(configureOptions);

        serviceCollection.AddScoped<INotificationsApiClient, NotificationsApiClient>();

        return serviceCollection;
    }
}
