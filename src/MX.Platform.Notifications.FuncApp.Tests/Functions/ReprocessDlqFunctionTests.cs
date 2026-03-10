using System.Net;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Moq;

using MX.Platform.Notifications.FuncApp.Functions;

namespace MX.Platform.Notifications.FuncApp.Tests.Functions;

public class ReprocessDlqFunctionTests
{
    private readonly Mock<ILogger<ReprocessDlqFunction>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly ReprocessDlqFunction _sut;

    public ReprocessDlqFunctionTests()
    {
        _loggerMock = new Mock<ILogger<ReprocessDlqFunction>>();
        _configurationMock = new Mock<IConfiguration>();
        _sut = new ReprocessDlqFunction(_loggerMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task Run_WithoutQueueName_ReturnsBadRequest()
    {
        // Arrange
        var context = FunctionTestHelpers.CreateFunctionContext();
        var request = FunctionTestHelpers.CreateHttpRequestData(context);

        // Act
        var response = await _sut.Run(request, context);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        Assert.Contains("queueName", body);
    }

    [Fact]
    public async Task Run_WithEmptyQueueName_ReturnsBadRequest()
    {
        // Arrange
        var context = FunctionTestHelpers.CreateFunctionContext();
        var request = FunctionTestHelpers.CreateHttpRequestData(context, "queueName=");

        // Act
        var response = await _sut.Run(request, context);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Run_WithQueueNameButNoServiceBusConfig_ReturnsInternalServerError()
    {
        // Arrange
        _configurationMock
            .Setup(c => c[It.Is<string>(k => k == "ServiceBusConnection__fullyQualifiedNamespace")])
            .Returns((string?)null);

        var context = FunctionTestHelpers.CreateFunctionContext();
        var request = FunctionTestHelpers.CreateHttpRequestData(context, "queueName=email_send_queue");

        // Act
        var response = await _sut.Run(request, context);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await ReadResponseBodyAsync(response);
        Assert.Contains("Service Bus connection is not configured", body);
    }

    [Fact]
    public async Task Run_WithEmptyServiceBusConfig_ReturnsInternalServerError()
    {
        // Arrange
        _configurationMock
            .Setup(c => c[It.Is<string>(k => k == "ServiceBusConnection__fullyQualifiedNamespace")])
            .Returns("  ");

        var context = FunctionTestHelpers.CreateFunctionContext();
        var request = FunctionTestHelpers.CreateHttpRequestData(context, "queueName=email_send_queue");

        // Act
        var response = await _sut.Run(request, context);

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    private static async Task<string> ReadResponseBodyAsync(HttpResponseData response)
    {
        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body);
        return await reader.ReadToEndAsync();
    }
}
