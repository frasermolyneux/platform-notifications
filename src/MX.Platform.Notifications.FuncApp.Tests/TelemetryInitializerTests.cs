using Microsoft.ApplicationInsights.DataContracts;

using MX.Platform.Notifications.FuncApp;

namespace MX.Platform.Notifications.FuncApp.Tests;

public class TelemetryInitializerTests
{
    [Fact]
    public void Initialize_SetsCloudRoleName()
    {
        // Arrange
        var initializer = new TelemetryInitializer();
        var telemetry = new RequestTelemetry();

        // Act
        initializer.Initialize(telemetry);

        // Assert
        Assert.Equal("platform-notifications", telemetry.Context.Cloud.RoleName);
    }

    [Fact]
    public void Initialize_SetsRoleName_ForDifferentTelemetryTypes()
    {
        // Arrange
        var initializer = new TelemetryInitializer();
        var eventTelemetry = new EventTelemetry("TestEvent");
        var traceTelemetry = new TraceTelemetry("Test trace");

        // Act
        initializer.Initialize(eventTelemetry);
        initializer.Initialize(traceTelemetry);

        // Assert
        Assert.Equal("platform-notifications", eventTelemetry.Context.Cloud.RoleName);
        Assert.Equal("platform-notifications", traceTelemetry.Context.Cloud.RoleName);
    }
}
