using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PaymentOrchestrator.Api.Infrastructure.Correlation;
using PaymentOrchestrator.Api.Infrastructure.Errors;
using PaymentOrchestrator.Api.Infrastructure.Idempotency;
using PaymentOrchestrator.Api.Infrastructure.Swagger;
using PaymentOrchestrator.Api.Infrastructure.Time;
using PaymentOrchestrator.Application;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Infrastructure;
using PaymentOrchestrator.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// [Item 2] Seguridad: Configurar límites de Kestrel
// Esto protege contra ataques DoS por agotamiento de memoria al usar EnableBuffering en el IdempotencyFilter.
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 1 * 1024 * 1024; // Límite de 1MB (suficiente para JSONs de pagos)
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<IdempotencyFilter>();
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// [Item 1] Estandarización: Registrar servicios de Health Checks nativos
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PaymentOrchestratorDbContext>();

builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<RequiredHeadersOperationFilter>();

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);

// Infrastructure: Correlation + Clock
builder.Services.AddSingleton<CorrelationContext>();
builder.Services.AddScoped<ICorrelationContext>(_ => new CorrelationContext());
builder.Services.AddSingleton<IClock, SystemClock>();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

// [Item 1] Estandarización: Endpoints nativos de Health Checks
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    // Liveness: Responde 200 si la app arrancó. Ignora dependencias.
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    // Readiness: Verifica que la app puede recibir tráfico (incluye dependencias si se registran).
    Predicate = _ => true
});

app.MapControllers();

app.Run();

// Necesario para los tests de integración (WebApplicationFactory)
public partial class Program { }