using Microsoft.OpenApi;
using PaymentOrchestrator.Api.Infrastructure.Correlation;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PaymentOrchestrator.Api.Infrastructure.Swagger;

public sealed class RequiredHeadersOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // CORRECCIÓN 1: Usar List<IOpenApiParameter> en lugar de inferir o usar concreta
        operation.Parameters ??= [];

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = CorrelationIdMiddleware.HeaderName,
            In = ParameterLocation.Header,
            Required = false,
            Description = "Correlation ID for tracing. If omitted, server generates one.",
            // CORRECCIÓN 2: Usar JsonSchemaType.String (Enum) en lugar de "string"
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        });

        if (context.ApiDescription.RelativePath?.Equals("payments", StringComparison.OrdinalIgnoreCase) == true
            && context.ApiDescription.HttpMethod?.Equals("POST", StringComparison.OrdinalIgnoreCase) == true)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Idempotency-Key",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Safe retry key. Same body + same key returns same response.",
                // CORRECCIÓN 2: Mismo cambio aquí
                Schema = new OpenApiSchema { Type = JsonSchemaType.String, MaxLength = 200 }
            });
        }
    }
}