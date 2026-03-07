using System.Net;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

using MX.Platform.Notifications.FuncApp.OpenApi;

namespace MX.Platform.Notifications.FuncApp.Functions;

public class OpenApiFunction
{
    [Function(nameof(OpenApiFunction))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "openapi/{filename}")] HttpRequestData req,
        FunctionContext context, string filename)
    {
        if (!string.Equals(filename, "v1.json", StringComparison.OrdinalIgnoreCase))
        {
            return req.CreateResponse(HttpStatusCode.NotFound);
        }

        var content = await OpenApiDocumentGenerator.GenerateAsync();

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json");
        await response.WriteStringAsync(content);
        return response;
    }
}
