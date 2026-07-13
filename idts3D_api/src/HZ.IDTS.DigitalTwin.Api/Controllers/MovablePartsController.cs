using HZ.IDTS.DigitalTwin.Application.MovableParts;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.MovableParts;
using Microsoft.AspNetCore.Mvc;

namespace HZ.IDTS.DigitalTwin.Api.Controllers;

[ApiController]
[Route("api/model-assets/{assetId:long}/versions/{versionId:long}/movable-parts")]
public sealed class MovablePartsController : ControllerBase
{
    private readonly IMovablePartService _service;

    public MovablePartsController(IMovablePartService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<MovablePartListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MovablePartListResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<MovablePartListResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<MovablePartListResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<MovablePartListResponse>>> Get([FromRoute] long assetId, [FromRoute] long versionId, [FromQuery] bool? enabled, [FromQuery] string? mode, CancellationToken cancellationToken)
    {
        var result = await _service.GetMovablePartsAsync(new GetMovablePartsRequest(assetId, versionId, enabled, mode), cancellationToken);
        return StatusCode((int)result.StatusCode, result.Response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<MovablePartResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MovablePartResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<MovablePartResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<MovablePartResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<MovablePartResponse>>> Create([FromRoute] long assetId, [FromRoute] long versionId, [FromBody] CreateMovablePartRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(assetId, versionId, request, cancellationToken);
        return StatusCode((int)result.StatusCode, result.Response);
    }

    [HttpPut("{partId:long}")]
    [ProducesResponseType(typeof(ApiResponse<MovablePartResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<MovablePartResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<MovablePartResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<MovablePartResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<MovablePartResponse>>> Update([FromRoute] long assetId, [FromRoute] long versionId, [FromRoute] long partId, [FromBody] UpdateMovablePartRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(assetId, versionId, partId, request, cancellationToken);
        return StatusCode((int)result.StatusCode, result.Response);
    }

    [HttpDelete("{partId:long}")]
    [ProducesResponseType(typeof(ApiResponse<DeleteMovablePartResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<DeleteMovablePartResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<DeleteMovablePartResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<DeleteMovablePartResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<DeleteMovablePartResponse>>> Delete([FromRoute] long assetId, [FromRoute] long versionId, [FromRoute] long partId, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(assetId, versionId, partId, cancellationToken);
        return StatusCode((int)result.StatusCode, result.Response);
    }
}
