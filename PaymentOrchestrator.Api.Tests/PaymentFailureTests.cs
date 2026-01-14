// PaymentFailureTests.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace PaymentOrchestrator.Api.Tests;

public sealed class PaymentFailureTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Capture_WithoutAuthorization_Returns422UnprocessableEntity()
    {
        // 1. Crear Pago (Estado: Created)
        var createReq = new { clientId = "test-fail", amount = 100m, currency = "USD" };
        var createRes = await _client.PostAsJsonAsync("/payments", createReq);
        var createBody = await createRes.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        var paymentId = createBody!["paymentId"];

        // 2. Intentar Capturar directamente (Saltando Authorize)
        // Esto debería disparar una DomainException en el Dominio
        var captureReq = new { amount = 100m };
        var captureRes = await _client.PostAsJsonAsync($"/payments/{paymentId}/capture", captureReq);

        // 3. Verificar que GlobalExceptionHandler lo transformó a 422 ProblemDetails
        Assert.Equal(HttpStatusCode.UnprocessableEntity, captureRes.StatusCode);

        var problem = await captureRes.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Domain validation error", problem!.Title);
        Assert.NotNull(problem.Detail); // Asegurar que hay una explicación
    }

    [Fact]
    public async Task Create_WithInvalidCurrency_Returns400BadRequest()
    {
        // FluentValidation debería atrapar esto antes del dominio
        var req = new { clientId = "test-val", amount = 100m, currency = "XX" };
        var res = await _client.PostAsJsonAsync("/payments", req);

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);

        var problem = await res.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal("Validation error", problem!.Title);
        Assert.True(problem.Extensions.ContainsKey("errors"));
    }
}