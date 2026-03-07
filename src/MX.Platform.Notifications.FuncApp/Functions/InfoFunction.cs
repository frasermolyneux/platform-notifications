using System.Net;
using System.Reflection;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

using MX.Platform.Notifications.Abstractions.V1.Models;

namespace MX.Platform.Notifications.FuncApp.Functions;

public class InfoFunction
{
    [Function(nameof(InfoFunction))]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/info")] HttpRequestData req,
        FunctionContext context)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
        var assemblyVersion = assembly.GetName().Version?.ToString() ?? "unknown";

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new
        {
            version = informationalVersion,
            buildVersion = informationalVersion.Split('+')[0],
            assemblyVersion
        });
        return response;
    }
}
