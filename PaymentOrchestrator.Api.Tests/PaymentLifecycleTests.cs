using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace PaymentOrchestrator.Api.Tests;

public sealed class PaymentLifecycleTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task FullFlow_Create_Authorize_Capture_Get()
    {
        // 1. Crear
        var createReq = new { clientId = "tester", amount = 1000m, currency = "EUR" };
        var createRes = await _client.PostAsJsonAsync("/payments", createReq);
        createRes.EnsureSuccessStatusCode();

        var createBody = await createRes.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        var paymentId = Guid.Parse(createBody!["paymentId"]);

        // 2. Autorizar
        var authReq = new { pspReference = "psp-123" };
        var authRes = await _client.PostAsJsonAsync($"/payments/{paymentId}/authorize", authReq);
        Assert.Equal(HttpStatusCode.Accepted, authRes.StatusCode);

        // 3. Capturar
        var captureReq = new { amount = 1000m };
        var captureRes = await _client.PostAsJsonAsync($"/payments/{paymentId}/capture", captureReq);
        Assert.Equal(HttpStatusCode.Accepted, captureRes.StatusCode);

        // 4. Verificar Estado Final (GET)
        var getRes = await _client.GetAsync($"/payments/{paymentId}");
        getRes.EnsureSuccessStatusCode();

        var payment = await getRes.Content.ReadFromJsonAsync<PaymentDto>();

        Assert.NotNull(payment);
        Assert.Equal("Captured", payment.Status);
        Assert.Equal(1000m, payment.Amount);
        Assert.Equal("psp-123", payment.PspReference);
    }

    // DTO local para el test
    record PaymentDto(Guid Id, string Status, decimal Amount, string PspReference);
}