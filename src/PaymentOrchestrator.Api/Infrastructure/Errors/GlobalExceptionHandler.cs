using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaymentOrchestrator.Application.Common.Exceptions;
using PaymentOrchestrator.Domain.Common;

namespace PaymentOrchestrator.Api.Infrastructure.Errors;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception ex, CancellationToken ct)
    {
        var (status, title, type) = ex switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation error", "https://httpstatuses.com/400"),
            NotFoundException => (StatusCodes.Status404NotFound, "Not found", "https://httpstatuses.com/404"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict", "https://httpstatuses.com/409"),
            DomainException => (StatusCodes.Status422UnprocessableEntity, "Domain validation error", "https://httpstatuses.com/422"), // Mejor 422 para dominio
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Concurrency Conflict", "https://httpstatuses.com/409"), // <--- NUEVO
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error", "https://httpstatuses.com/500")
        };

        logger.LogError(ex, "Unhandled exception");

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = type,
            Detail = ex.Message,
            Instance = http.Request.Path
        };

        // Para ValidationException, agregamos errores
        if (ex is ValidationException vex)
        {
            problem.Extensions["errors"] = vex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());
        }

        http.Response.StatusCode = status;
        http.Response.ContentType = "application/problem+json";
        await http.Response.WriteAsJsonAsync(problem, ct);

        return true;
    }
}
