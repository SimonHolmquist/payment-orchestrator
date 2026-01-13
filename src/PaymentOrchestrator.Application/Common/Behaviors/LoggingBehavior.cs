using MediatR;
using Microsoft.Extensions.Logging;

namespace PaymentOrchestrator.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Log pre-execution
        logger.LogInformation("Processing Command {CommandName}", requestName);

        var response = await next();

        // Log post-execution
        logger.LogInformation("Processed Command {CommandName}", requestName);

        return response;
    }
}