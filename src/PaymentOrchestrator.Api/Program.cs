using PaymentOrchestrator.Api.Infrastructure;
using PaymentOrchestrator.Application;
using PaymentOrchestrator.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar servicios de la API (Controllers + ProblemDetails)
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// Registrar el manejador de excepciones global personalizado
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// 2. Configurar OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Payment Orchestrator API", Version = "v1" });
    // Aquí se puede configurar XML comments si se habilitan en el csproj
});

// 3. Agregar capas de Aplicación e Infraestructura
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

// 4. Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Usar el middleware de manejo de excepciones (DEBE ir al principio)
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthorization(); // Aunque aún no hay auth, es buena práctica dejarlo

app.MapControllers();

app.Run();