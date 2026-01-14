using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net.Http.Json;

namespace PaymentOrchestrator.Api.Tests;

public sealed class PaymentsIdempotencyTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task PostPayments_SameKeySameBody_ReturnsSamePaymentId()
    {
        var req = new { clientId = "client-123", amount = 100m, currency = "USD" };
        var idempotencyKey = Guid.NewGuid().ToString();

        using var m1 = new HttpRequestMessage(HttpMethod.Post, "/payments");
        m1.Headers.Add("Idempotency-Key", idempotencyKey);
        m1.Content = JsonContent.Create(req);

        var r1 = await _client.SendAsync(m1);
        r1.EnsureSuccessStatusCode();
        var body1 = await r1.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        var p1 = body1!["paymentId"];

        using var m2 = new HttpRequestMessage(HttpMethod.Post, "/payments");
        m2.Headers.Add("Idempotency-Key", idempotencyKey);
        m2.Content = JsonContent.Create(req);

        var r2 = await _client.SendAsync(m2);
        r2.EnsureSuccessStatusCode();
        var body2 = await r2.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        var p2 = body2!["paymentId"];

        Assert.Equal(p1, p2);
    }
}