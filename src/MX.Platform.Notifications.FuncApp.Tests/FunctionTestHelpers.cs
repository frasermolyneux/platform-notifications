using System.Net;
using System.Text.Json;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Moq;

namespace MX.Platform.Notifications.FuncApp.Tests;

/// <summary>
/// Helper for creating mock HttpRequestData and FunctionContext instances in tests.
/// </summary>
internal static class FunctionTestHelpers
{
    public static FunctionContext CreateFunctionContext()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        });
        serviceCollection.AddSingleton(Options.Create(new WorkerOptions
        {
            Serializer = new Azure.Core.Serialization.JsonObjectSerializer(
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
        }));
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var mock = new Mock<FunctionContext>();
        mock.Setup(c => c.InstanceServices).Returns(serviceProvider);
        return mock.Object;
    }

    public static HttpRequestData CreateHttpRequestData(
        FunctionContext context,
        string? queryString = null)
    {
        var uri = queryString is not null
            ? new Uri($"https://localhost/api/test?{queryString}")
            : new Uri("https://localhost/api/test");

        var mock = new Mock<HttpRequestData>(context);
        mock.Setup(r => r.Url).Returns(uri);
        mock.Setup(r => r.Headers).Returns(new HttpHeadersCollection());

        var queryCollection = System.Web.HttpUtility.ParseQueryString(uri.Query);
        mock.Setup(r => r.Query).Returns(queryCollection);

        mock.Setup(r => r.CreateResponse()).Returns(() => new FakeHttpResponseData(context));

        return mock.Object;
    }
}

/// <summary>
/// Concrete implementation of HttpResponseData for testing, since some members are non-virtual.
/// </summary>
internal class FakeHttpResponseData : HttpResponseData
{
    public FakeHttpResponseData(FunctionContext functionContext) : base(functionContext)
    {
        Headers = new HttpHeadersCollection();
        Body = new MemoryStream();
    }

    public override HttpStatusCode StatusCode { get; set; }
    public override HttpHeadersCollection Headers { get; set; }
    public override Stream Body { get; set; }
    public override HttpCookies Cookies => throw new NotImplementedException();
}
