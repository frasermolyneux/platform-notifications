using System.Net;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using Moq;

using MX.Platform.Notifications.FuncApp.Functions;

namespace MX.Platform.Notifications.FuncApp.Tests.Functions;

public class HealthFunctionTests
{
    private readonly Mock<HealthCheckService> _healthCheckServiceMock;
    private readonly HealthFunction _sut;

    public HealthFunctionTests()
    {
        _healthCheckServiceMock = new Mock<HealthCheckService>();
        _sut = new HealthFunction(_healthCheckServiceMock.Object);
    }

    [Fact]
    public async Task Run_WhenHealthy_ReturnsOk()
    {
        // Arrange
        var healthReport = new HealthReport(
            entries: new Dictionary<string, HealthReportEntry>(),
            totalDuration: TimeSpan.FromMilliseconds(50));

        _healthCheckServiceMock
            .Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthReport);

        var context = FunctionTestHelpers.CreateFunctionContext();
        var request = FunctionTestHelpers.CreateHttpRequestData(context);

        // Act
        var response = await _sut.Run(request, context);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Run_WhenUnhealthy_ReturnsServiceUnavailable()
    {
        // Arrange
        var entries = new Dictionary<string, HealthReportEntry>
        {
            ["database"] = new HealthReportEntry(
                status: HealthStatus.Unhealthy,
                description: "Cannot connect",
                duration: TimeSpan.FromMilliseconds(100),
                exception: null,
                data: null)
        };
        var healthReport = new HealthReport(entries, totalDuration: TimeSpan.FromMilliseconds(100));

        _healthCheckServiceMock
            .Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthReport);

        var context = FunctionTestHelpers.CreateFunctionContext();
        var request = FunctionTestHelpers.CreateHttpRequestData(context);

        // Act
        var response = await _sut.Run(request, context);

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    [Fact]
    public async Task Run_WhenDegraded_ReturnsServiceUnavailable()
    {
        // Arrange
        var entries = new Dictionary<string, HealthReportEntry>
        {
            ["cache"] = new HealthReportEntry(
                status: HealthStatus.Degraded,
                description: "Slow response",
                duration: TimeSpan.FromMilliseconds(500),
                exception: null,
                data: null)
        };
        var healthReport = new HealthReport(entries, totalDuration: TimeSpan.FromMilliseconds(500));

        _healthCheckServiceMock
            .Setup(h => h.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthReport);

        var context = FunctionTestHelpers.CreateFunctionContext();
        var request = FunctionTestHelpers.CreateHttpRequestData(context);

        // Act
        var response = await _sut.Run(request, context);

        // Assert
        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }
}
