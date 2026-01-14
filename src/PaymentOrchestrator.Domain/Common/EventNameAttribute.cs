namespace PaymentOrchestrator.Domain.Common;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class EventNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}