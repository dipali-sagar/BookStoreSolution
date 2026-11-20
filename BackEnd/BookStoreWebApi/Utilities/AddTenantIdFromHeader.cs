using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json.Nodes;

namespace BookStoreApi.Utilities
{
    /// <summary>
    /// Swagger Operation Filter.
    /// Injects a 'Tenant-Id' header into the Swagger UI for testing Multi-Tenancy.
    /// This version avoids specific OpenAPI value wrappers for a cleaner implementation.
    /// </summary>
    public class AddTenantIdFromHeader : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Ensure the parameters list is initialized
            operation.Parameters ??= [];

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Tenant-Id",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Guid for the Tenant (Store). Default is Amazon Store.",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    // Note: We use a simple string here to avoid dependency on JsonValue / OpenApiAny
                    Default = JsonValue.Create("226DC44F-5A83-4D39-901E-87F7A790197B")
                }
            });
        }
    }
}
