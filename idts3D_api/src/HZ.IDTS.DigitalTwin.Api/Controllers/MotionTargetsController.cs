using HZ.IDTS.DigitalTwin.Application.MotionTargets;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.MotionTargets;
using Microsoft.AspNetCore.Mvc;

namespace HZ.IDTS.DigitalTwin.Api.Controllers;

[ApiController]
[Route("api/movable-parts/{partId:long}/motion-targets")]
public sealed class MotionTargetsController : ControllerBase
{
    private readonly IMotionTargetService _service;

    public MotionTargetsController(IMotionTargetService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<MotionTargetListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MotionTargetListResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<MotionTargetListResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<MotionTargetListResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<MotionTargetListResponse>>> Get([FromRoute] long partId, [FromQuery] bool? enabled, [FromQuery] string? mode, CancellationToken cancellationToken)
    {
        var result = await _service.GetMotionTargetsAsync(new GetMotionTargetsRequest(partId, enabled, mode), cancellationToken);
        return StatusCode((int)result.StatusCode, result.Response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<MotionTargetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MotionTargetResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<MotionTargetResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<MotionTargetResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<MotionTargetResponse>>> Create([FromRoute] long partId, [FromBody] CreateMotionTargetRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(partId, request, cancellationToken);
        return StatusCode((int)result.StatusCode, result.Response);
    }

    [HttpPut("{targetId:long}")]
    [ProducesResponseType(typeof(ApiResponse<MotionTargetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MotionTargetResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<MotionTargetResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<MotionTargetResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<MotionTargetResponse>>> Update([FromRoute] long partId, [FromRoute] long targetId, [FromBody] UpdateMotionTargetRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(partId, targetId, request, cancellationToken);
        return StatusCode((int)result.StatusCode, result.Response);
    }

    [HttpDelete("{targetId:long}")]
    [ProducesResponseType(typeof(ApiResponse<DeleteMotionTargetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DeleteMotionTargetResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<DeleteMotionTargetResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<DeleteMotionTargetResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<DeleteMotionTargetResponse>>> Delete([FromRoute] long partId, [FromRoute] long targetId, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(partId, targetId, cancellationToken);
        return StatusCode((int)result.StatusCode, result.Response);
    }
}
