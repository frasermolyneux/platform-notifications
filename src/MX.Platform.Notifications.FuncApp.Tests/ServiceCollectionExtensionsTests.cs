using Microsoft.Extensions.DependencyInjection;

using MX.Platform.Notifications.Abstractions.V1.Interfaces;
using MX.Platform.Notifications.Api.Client.V1;

namespace MX.Platform.Notifications.FuncApp.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNotificationsApiClient_RegistersSendEmailApi()
    {
        var services = new ServiceCollection();

        services.AddNotificationsApiClient(options => options
            .WithBaseUrl("https://notifications.example.test")
            .WithApiKeyAuthentication("test-key"));

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();

        var sendEmailApi = scope.ServiceProvider.GetService<ISendEmailApi>();
        var client = scope.ServiceProvider.GetService<INotificationsApiClient>();

        Assert.NotNull(sendEmailApi);
        Assert.NotNull(client);
    }
}
