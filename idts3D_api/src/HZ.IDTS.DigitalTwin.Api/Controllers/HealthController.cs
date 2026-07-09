using HZ.IDTS.DigitalTwin.Contracts.Common;
using Microsoft.AspNetCore.Mvc;

namespace HZ.IDTS.DigitalTwin.Api.Controllers;

[ApiController]
[Route("api/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<HealthResponse>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<HealthResponse>> Get()
    {
        var response = new HealthResponse("Healthy", DateTimeOffset.UtcNow);

        return Ok(ApiResponse<HealthResponse>.Ok(response));
    }
}
