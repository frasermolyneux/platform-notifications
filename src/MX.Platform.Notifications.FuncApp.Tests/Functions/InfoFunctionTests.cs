using System.Net;

using MX.Platform.Notifications.FuncApp.Functions;

namespace MX.Platform.Notifications.FuncApp.Tests.Functions;

public class InfoFunctionTests
{
    private readonly InfoFunction _sut;

    public InfoFunctionTests()
    {
        _sut = new InfoFunction();
    }

    [Fact]
    public async Task Run_ReturnsOkStatusCode()
    {
        // Arrange
        var context = FunctionTestHelpers.CreateFunctionContext();
        var request = FunctionTestHelpers.CreateHttpRequestData(context);

        // Act
        var response = await _sut.Run(request, context);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Run_ReturnsNonEmptyBody()
    {
        // Arrange
        var context = FunctionTestHelpers.CreateFunctionContext();
        var request = FunctionTestHelpers.CreateHttpRequestData(context);

        // Act
        var response = await _sut.Run(request, context);

        // Assert
        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body);
        var body = await reader.ReadToEndAsync();
        Assert.NotEmpty(body);
    }
}
