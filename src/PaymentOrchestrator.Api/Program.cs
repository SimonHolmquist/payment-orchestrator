using PaymentOrchestrator.Api.Infrastructure;
using PaymentOrchestrator.Api.Infrastructure.Correlation;
using PaymentOrchestrator.Api.Infrastructure.Errors;
using PaymentOrchestrator.Api.Infrastructure.Swagger;
using PaymentOrchestrator.Api.Infrastructure.Time;
using PaymentOrchestrator.Application;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<RequiredHeadersOperationFilter>();
});

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);

// Correlation + clock
builder.Services.AddSingleton<CorrelationContext>();
builder.Services.AddScoped<ICorrelationContext>(_ => new CorrelationContext());
builder.Services.AddSingleton<IClock, SystemClock>();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();

public partial class Program { } // para WebApplicationFactory tests
