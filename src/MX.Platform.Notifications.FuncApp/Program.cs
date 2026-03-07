using System.Reflection;
using System.Text.Json;

using Azure.Communication.Email;
using Azure.Identity;
using Azure.Storage.Blobs;

using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MX.Platform.Notifications.FuncApp;
using MX.Platform.Notifications.FuncApp.Services;

var host = new HostBuilder()
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddEnvironmentVariables();
        builder.AddUserSecrets(Assembly.GetExecutingAssembly(), true);
    })
    .ConfigureFunctionsWorkerDefaults(builder =>
    {
        builder.Services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
    })
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        services.AddLogging();
        services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // ACS Email client using managed identity
        var acsEndpoint = configuration["ACS__Endpoint"]
            ?? throw new InvalidOperationException("ACS__Endpoint configuration is required");
        services.AddSingleton(new EmailClient(new Uri(acsEndpoint), new DefaultAzureCredential()));

        // Blob storage for claim-check pattern (uses managed identity)
        var blobServiceUri = configuration["StorageAccount__BlobServiceUri"];
        if (!string.IsNullOrWhiteSpace(blobServiceUri))
        {
            services.AddSingleton(new BlobServiceClient(new Uri(blobServiceUri), new DefaultAzureCredential()));
        }

        // Services
        services.AddSingleton<IEmailSenderService, EmailSenderService>();
        services.AddSingleton<IClaimCheckService, ClaimCheckService>();

        services.AddHealthChecks();
    })
    .Build();

await host.RunAsync();
