using Microsoft.OpenApi.Models; // <--- ESTE ERA EL MISSING USING PRINCIPAL
using PaymentOrchestrator.Api.Infrastructure.Correlation;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PaymentOrchestrator.Api.Infrastructure.Swagger;

public sealed class RequiredHeadersOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Asegurar que la lista de parámetros esté inicializada
        operation.Parameters ??= [];

        // 1. Header de Correlación (Global)
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = CorrelationIdMiddleware.HeaderName,
            In = ParameterLocation.Header,
            Required = false,
            Description = "Correlation ID for tracing. If omitted, server generates one.",
            Schema = new OpenApiSchema
            {
                Type = "string" // En Swashbuckle/OpenApi estándar se usa string en minúscula
            }
        });

        // 2. Header de Idempotencia (Solo para POST en /payments)
        // Detectamos si es un endpoint relevante
        var isPaymentPost = context.ApiDescription.RelativePath?.Equals("payments", StringComparison.OrdinalIgnoreCase) == true
                            && context.ApiDescription.HttpMethod?.Equals("POST", StringComparison.OrdinalIgnoreCase) == true;

        // O si decoraste el endpoint con [Idempotent], podrías buscar el atributo en context.MethodInfo
        // pero por simplicidad mantengamos la lógica por path o generalizamos a todos los POSTs de escritura:

        if (isPaymentPost || IsIdempotentOperation(context))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Idempotency-Key",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Safe retry key. Same body + same key returns same response.",
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    MaxLength = 100
                }
            });
        }
    }

    private static bool IsIdempotentOperation(OperationFilterContext context)
    {
        // Si quieres ser más preciso, busca si el método tiene el atributo [Idempotent]
        var attributes = context.MethodInfo.GetCustomAttributes(true);
        // Asumiendo que IdempotentAttribute está en el namespace correcto
        return attributes.Any(a => a.GetType().Name == "IdempotentAttribute");
    }
}