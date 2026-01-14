using Microsoft.AspNetCore.Diagnostics.HealthChecks; // [Item 1] Necesario para MapHealthChecks
using PaymentOrchestrator.Api.Infrastructure.Correlation;
using PaymentOrchestrator.Api.Infrastructure.Errors;
using PaymentOrchestrator.Api.Infrastructure.Idempotency;
using PaymentOrchestrator.Api.Infrastructure.Swagger;
using PaymentOrchestrator.Api.Infrastructure.Time;
using PaymentOrchestrator.Application;
using PaymentOrchestrator.Application.Common.Interfaces;
using PaymentOrchestrator.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// [Item 2] Configurar límites de Kestrel para proteger el buffering del IdempotencyFilter
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Límite de 1MB para requests. Suficiente para payloads de pagos, evita DoS por memoria.
    serverOptions.Limits.MaxRequestBodySize = 1 * 1024 * 1024;
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<IdempotencyFilter>();
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// [Item 1] Registrar servicios de Health Checks
builder.Services.AddHealthChecks();
// Nota Enterprise: Aquí deberías añadir .AddSqlServer() y .AddRabbitMQ() usando los paquetes
// 'AspNetCore.HealthChecks.SqlServer' y 'AspNetCore.HealthChecks.Rabbitmq'.

builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<RequiredHeadersOperationFilter>();

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
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

// [Item 1] Mapear endpoints de Health Checks nativos
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    // Liveness: Responde 200 OK si el proceso de .NET está corriendo.
    // Predicate = _ => false excluye chequeos de dependencias (BD, etc.)
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    // Readiness: Ejecuta todos los chequeos registrados para confirmar que puede recibir tráfico.
    Predicate = _ => true
});

app.MapControllers();

app.Run();

public partial class Program { } // Para tests de integración