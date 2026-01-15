using PaymentOrchestrator.Infrastructure;
using PaymentOrchestrator.Workers.OutboxPublisher;

var builder = Host.CreateApplicationBuilder(args);

// Registrar Infraestructura (DbContext, etc.)
builder.Services.AddInfrastructure(builder.Configuration);

// Registrar el Worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();