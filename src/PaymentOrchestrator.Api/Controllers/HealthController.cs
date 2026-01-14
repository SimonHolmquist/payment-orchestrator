using Microsoft.AspNetCore.Mvc;

namespace PaymentOrchestrator.Api.Controllers;

[ApiController]
public sealed class HealthController : ControllerBase
{
    [HttpGet("/health/live")]
    public IActionResult Live() => Ok(new { status = "live" });

    [HttpGet("/health/ready")]
    public IActionResult Ready() => Ok(new { status = "ready" });
}
