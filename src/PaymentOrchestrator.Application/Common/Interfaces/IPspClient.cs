// src/PaymentOrchestrator.Application/Common/Interfaces/IPspClient.cs
namespace PaymentOrchestrator.Application.Common.Interfaces;

public interface IPspClient
{
    // Solo definimos el contrato base por ahora
    Task<bool> AuthorizeAsync(string paymentId, decimal amount, string currency, CancellationToken ct);
}