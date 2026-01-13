using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using PaymentOrchestrator.Domain.Common;

namespace PaymentOrchestrator.Api.Infrastructure;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Ocurrió una excepción no controlada: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path
        };

        switch (exception)
        {
            case ValidationException validationEx:
                problemDetails.Title = "Errores de validación";
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Detail = "Se encontraron uno o más errores de validación.";
                problemDetails.Extensions["errors"] = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray()
                    );
                break;

            case DomainException domainEx:
                problemDetails.Title = "Error de Dominio";
                problemDetails.Status = StatusCodes.Status400BadRequest; // O 422 UnprocessableEntity
                problemDetails.Detail = domainEx.Message;
                break;

            // Agrega aquí KeyNotFoundException si tu repositorio la lanza para devolver 404
            // case KeyNotFoundException _:
            //     problemDetails.Title = "Recurso no encontrado";
            //     problemDetails.Status = StatusCodes.Status404NotFound;
            //     break;

            default:
                problemDetails.Title = "Error interno del servidor";
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Detail = "Ha ocurrido un error inesperado.";
                break;
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}