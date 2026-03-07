using System.Reflection;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.OpenApi;

namespace MX.Platform.Notifications.FuncApp.OpenApi;

public static class OpenApiDocumentGenerator
{
    public static Task<string> GenerateAsync()
    {
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        return GenerateAsync(assembly);
    }

    public static async Task<string> GenerateAsync(Assembly assembly)
    {
        var document = new OpenApiDocument
        {
            Info = new OpenApiInfo
            {
                Title = "Notifications API",
                Description = "Platform Notifications API v1",
                Version = "1.0"
            },
            Paths = new OpenApiPaths(),
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>(),
                SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
                {
                    ["bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT"
                    }
                }
            }
        };

        document.Security =
        [
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("bearer", document)] = new List<string>()
            }
        ];

        DiscoverHttpTriggers(document, assembly);

        using var stream = new MemoryStream();
        var writer = new OpenApiJsonWriter(new StreamWriter(stream));
        document.SerializeAsV3(writer);
        await writer.FlushAsync();
        stream.Position = 0;
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    private static void DiscoverHttpTriggers(OpenApiDocument document, Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var functionAttr = method.GetCustomAttribute<FunctionAttribute>();
                if (functionAttr == null) continue;

                var httpTriggerParam = method.GetParameters()
                    .FirstOrDefault(p => p.GetCustomAttribute<HttpTriggerAttribute>() != null);
                if (httpTriggerParam == null) continue;

                var httpTrigger = httpTriggerParam.GetCustomAttribute<HttpTriggerAttribute>()!;

                if (httpTrigger.AuthLevel == AuthorizationLevel.Function) continue;
                var route = httpTrigger.Route ?? functionAttr.Name;
                if (route.StartsWith("openapi", StringComparison.OrdinalIgnoreCase)) continue;

                var httpMethods = httpTrigger.Methods ?? ["get"];

                var cleanRoute = route;
                if (cleanRoute.StartsWith("v1/", StringComparison.OrdinalIgnoreCase))
                    cleanRoute = cleanRoute.Substring(3);
                var path = "/" + cleanRoute.TrimStart('/');

                var bodyType = FindRequestBodyType(method);

                if (!document.Paths.TryGetValue(path, out var pathItem))
                    pathItem = new OpenApiPathItem { Operations = new Dictionary<HttpMethod, OpenApiOperation>() };

                foreach (var httpMethod in httpMethods)
                {
                    var operation = new OpenApiOperation
                    {
                        Summary = functionAttr.Name,
                        OperationId = functionAttr.Name,
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse { Description = "Success" }
                        }
                    };

                    if (bodyType != null)
                    {
                        var schemaName = bodyType.Name;
                        if (!document.Components!.Schemas!.ContainsKey(schemaName))
                        {
                            document.Components!.Schemas![schemaName] = BuildSchema(bodyType);
                        }

                        operation.RequestBody = new OpenApiRequestBody
                        {
                            Required = true,
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/json"] = new()
                                {
                                    Schema = new OpenApiSchemaReference(schemaName, document)
                                }
                            }
                        };
                    }

                    var httpMethodObj = httpMethod.ToUpperInvariant() switch
                    {
                        "GET" => HttpMethod.Get,
                        "POST" => HttpMethod.Post,
                        "PUT" => HttpMethod.Put,
                        "DELETE" => HttpMethod.Delete,
                        "PATCH" => HttpMethod.Patch,
                        "HEAD" => HttpMethod.Head,
                        _ => HttpMethod.Get
                    };

                    pathItem!.Operations![httpMethodObj] = operation;
                }

                document.Paths[path] = pathItem;
            }
        }
    }

    private static Type? FindRequestBodyType(MethodInfo method)
    {
        var abstractionsAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "MX.Platform.Notifications.Abstractions.V1");

        if (abstractionsAssembly == null) return null;

        // Look for HttpRequestData parameter and check if method reads a known DTO type
        var reqParam = method.GetParameters()
            .FirstOrDefault(p => p.ParameterType == typeof(HttpRequestData));
        if (reqParam == null) return null;

        // Match by function name convention: SendEmail -> SendEmailRequestDto
        var functionAttr = method.GetCustomAttribute<FunctionAttribute>();
        if (functionAttr == null) return null;

        var dtoName = $"{functionAttr.Name}RequestDto";
        return abstractionsAssembly.GetTypes()
            .FirstOrDefault(t => string.Equals(t.Name, dtoName, StringComparison.OrdinalIgnoreCase));
    }

    private static OpenApiSchema BuildSchema(Type type)
    {
        var properties = new Dictionary<string, IOpenApiSchema>();
        var required = new HashSet<string>();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propName = char.ToLowerInvariant(prop.Name[0]) + prop.Name[1..];
            properties[propName] = MapPropertyType(prop.PropertyType);
        }

        return new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = properties,
            Required = required
        };
    }

    private static OpenApiSchema MapPropertyType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;

        if (underlying == typeof(string))
            return new OpenApiSchema { Type = JsonSchemaType.String };
        if (underlying == typeof(int) || underlying == typeof(long))
            return new OpenApiSchema { Type = JsonSchemaType.Integer };
        if (underlying == typeof(float) || underlying == typeof(double) || underlying == typeof(decimal))
            return new OpenApiSchema { Type = JsonSchemaType.Number };
        if (underlying == typeof(bool))
            return new OpenApiSchema { Type = JsonSchemaType.Boolean };
        if (underlying == typeof(DateTime) || underlying == typeof(DateTimeOffset))
            return new OpenApiSchema { Type = JsonSchemaType.String, Format = "date-time" };
        if (underlying == typeof(Guid))
            return new OpenApiSchema { Type = JsonSchemaType.String, Format = "uuid" };

        // Handle List<T> and similar generic collections
        if (underlying.IsGenericType && typeof(System.Collections.IEnumerable).IsAssignableFrom(underlying))
        {
            var itemType = underlying.GetGenericArguments().FirstOrDefault();
            if (itemType != null)
            {
                return new OpenApiSchema
                {
                    Type = JsonSchemaType.Array,
                    Items = MapPropertyType(itemType)
                };
            }
        }

        return new OpenApiSchema { Type = JsonSchemaType.String };
    }
}
