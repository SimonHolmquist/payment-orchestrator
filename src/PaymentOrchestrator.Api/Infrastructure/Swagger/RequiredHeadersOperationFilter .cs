using Microsoft.OpenApi;
using PaymentOrchestrator.Api.Infrastructure.Correlation;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PaymentOrchestrator.Api.Infrastructure.Swagger;

public sealed class RequiredHeadersOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = CorrelationIdMiddleware.HeaderName,
            In = ParameterLocation.Header,
            Required = false,
            Description = "Correlation ID for tracing. If omitted, server generates one.",
            Schema = new OpenApiSchema { Type = "string" }
        });

        // Solo documentamos idempotency en POST /payments (M3)
        if (context.ApiDescription.RelativePath?.Equals("payments", StringComparison.OrdinalIgnoreCase) == true
            && context.ApiDescription.HttpMethod?.Equals("POST", StringComparison.OrdinalIgnoreCase) == true)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Idempotency-Key",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Safe retry key. Same body + same key returns same response.",
                Schema = new OpenApiSchema { Type = "string", MaxLength = 200 }
            });
        }
    }
}
