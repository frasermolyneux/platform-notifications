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

        using var provider = services.BuildServiceProvider();

        var sendEmailApi = provider.GetService<ISendEmailApi>();
        var client = provider.GetService<INotificationsApiClient>();

        Assert.NotNull(sendEmailApi);
        Assert.NotNull(client);
    }
}
