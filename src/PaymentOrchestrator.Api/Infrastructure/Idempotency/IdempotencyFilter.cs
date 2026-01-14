using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PaymentOrchestrator.Application.Common.Exceptions;
using PaymentOrchestrator.Application.Common.Interfaces;

namespace PaymentOrchestrator.Api.Infrastructure.Idempotency;

public sealed class IdempotencyFilter(
    IIdempotencyStore store,
    IClock clock) : IAsyncActionFilter
{
    private const string HeaderName = "Idempotency-Key";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // 1. Verificar si el endpoint tiene el atributo
        var attribute = context.ActionDescriptor.EndpointMetadata
            .OfType<IdempotentAttribute>()
            .FirstOrDefault();

        if (attribute is null)
        {
            await next();
            return;
        }

        // 2. Verificar cabecera
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var keyVal) || string.IsNullOrWhiteSpace(keyVal))
        {
            await next();
            return;
        }

        var idempotencyKey = keyVal.ToString();
        var operation = attribute.Operation;

        // Asumimos que el ClientId viene de un claim o, para este MVP, del cuerpo (esto podría refactorizarse para extraerlo del Auth token)
        // Por simplicidad y consistencia con tu controlador actual, intentaremos leerlo del body si es posible, 
        // pero idealmente debería venir de context.HttpContext.User.
        // Para no romper tu flujo actual M3 sin Auth, usaremos un valor fijo o lo extraeremos si el request es CreatePaymentRequest.
        // *Nota Enterprise*: En prod, el ClientId SIEMPRE viene del token JWT.
        string clientId = "unknown-client";
        string bodyJson = "{}";

        // Hack para leer el body y resetear el stream (requiere EnableBuffering en Program.cs)
        context.HttpContext.Request.EnableBuffering();
        using (var reader = new StreamReader(context.HttpContext.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            bodyJson = await reader.ReadToEndAsync();
            context.HttpContext.Request.Body.Position = 0;
        }

        // Intentar sacar ClientId del JSON (solo para M3, en M11 usar Auth)
        try
        {
            var doc = JsonDocument.Parse(bodyJson);
            if (doc.RootElement.TryGetProperty("clientId", out var elem)) clientId = elem.GetString() ?? clientId;
            else if (doc.RootElement.TryGetProperty("ClientId", out elem)) clientId = elem.GetString() ?? clientId;
        }
        catch { /* Ignorar error de parseo */ }

        var requestHash = Sha256Hex(bodyJson);

        // 3. Consultar Store
        var existing = await store.GetAsync(clientId, idempotencyKey, operation, context.HttpContext.RequestAborted);

        if (existing is not null)
        {
            // Validar integridad (mismo key, mismo body)
            if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
            {
                context.Result = new ConflictObjectResult(new ProblemDetails
                {
                    Title = "Idempotency Conflict",
                    Detail = "Idempotency-Key reused with different request payload."
                });
                return;
            }

            // Si ya hay respuesta guardada, devolverla
            if (existing.Response is not null)
            {
                var contentResult = new ContentResult
                {
                    Content = existing.Response.Body,
                    ContentType = existing.Response.ContentType,
                    StatusCode = existing.Response.StatusCode
                };
                context.Result = contentResult;
                return;
            }

            // Si existe pero no hay respuesta (está procesando), podríamos devolver 409 o 425 (Too Early).
            // O dejar pasar para reintentar (manejo de bloqueo optimista). Dejamos pasar.
        }
        else
        {
            // 4. Crear registro de intención (Lock)
            var record = new IdempotencyRecord(clientId, idempotencyKey, operation, requestHash, clock.UtcNow);
            var created = await store.TryCreateAsync(record, context.HttpContext.RequestAborted);

            if (!created)
            {
                // Carrera: alguien insertó justo antes. Releer.
                existing = await store.GetAsync(clientId, idempotencyKey, operation, context.HttpContext.RequestAborted);
                if (existing?.Response is not null)
                {
                    context.Result = new ContentResult
                    {
                        Content = existing.Response.Body,
                        ContentType = existing.Response.ContentType,
                        StatusCode = existing.Response.StatusCode
                    };
                    return;
                }
                // Si falla crear y no hay respuesta, es un conflicto de concurrencia grave.
                context.Result = new ConflictObjectResult(new ProblemDetails { Title = "Concurrent processing detected" });
                return;
            }
        }

        // 5. Ejecutar Controlador
        var executedContext = await next();

        // 6. Guardar Respuesta (si fue exitosa)
        if (executedContext.Result is ObjectResult objResult && objResult.StatusCode >= 200 && objResult.StatusCode < 300)
        {
            // Serializar respuesta
            var responseBody = JsonSerializer.Serialize(objResult.Value);
            var snapshot = new IdempotencyResponseSnapshot(
                objResult.StatusCode ?? 200,
                "application/json",
                responseBody);

            await store.SaveResponseAsync(clientId, idempotencyKey, operation, snapshot, clock.UtcNow, context.HttpContext.RequestAborted);
        }
        else if (executedContext.Result is StatusCodeResult statusResult && statusResult.StatusCode >= 200 && statusResult.StatusCode < 300)
        {
            // Caso Accepted (202) sin body
            var snapshot = new IdempotencyResponseSnapshot(
               statusResult.StatusCode,
               "application/json",
               "{}");
            await store.SaveResponseAsync(clientId, idempotencyKey, operation, snapshot, clock.UtcNow, context.HttpContext.RequestAborted);
        }
    }

    private static string Sha256Hex(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}