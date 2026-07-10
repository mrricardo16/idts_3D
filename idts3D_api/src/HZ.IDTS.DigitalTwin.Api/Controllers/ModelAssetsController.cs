using HZ.IDTS.DigitalTwin.Api.Models;
using HZ.IDTS.DigitalTwin.Application.ModelAssets;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;
using Microsoft.AspNetCore.Mvc;

namespace HZ.IDTS.DigitalTwin.Api.Controllers;

[ApiController]
[Route("api/model-assets")]
public sealed class ModelAssetsController : ControllerBase
{
    private readonly IModelAssetUploadService _uploadService;
    private readonly IModelManifestService _manifestService;
    private readonly IObjectTreeModelStatsService _objectTreeModelStatsService;

    public ModelAssetsController(
        IModelAssetUploadService uploadService,
        IModelManifestService manifestService,
        IObjectTreeModelStatsService objectTreeModelStatsService)
    {
        _uploadService = uploadService;
        _manifestService = manifestService;
        _objectTreeModelStatsService = objectTreeModelStatsService;
    }

    [HttpGet("{assetId:long}/manifest")]
    [ProducesResponseType(typeof(ApiResponse<ModelManifestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ModelManifestResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ModelManifestResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<ModelManifestResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<ModelManifestResponse>>> GetManifest(
        [FromRoute] long assetId,
        [FromQuery] long? versionId,
        [FromQuery] string? mode,
        CancellationToken cancellationToken)
    {
        var request = new GetModelManifestRequest(
            assetId,
            versionId,
            mode);

        var result = await _manifestService.GetManifestAsync(
            request,
            cancellationToken);

        return StatusCode((int)result.StatusCode, result.Response);
    }

    [HttpPut("{assetId:long}/versions/{versionId:long}/object-tree")]
    public async Task<ActionResult<ApiResponse<ObjectTreeResponse>>> SaveObjectTree(
        [FromRoute] long assetId,
        [FromRoute] long versionId,
        [FromBody] SaveObjectTreeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _objectTreeModelStatsService.SaveObjectTreeAsync(assetId, versionId, request, cancellationToken);
        return StatusCode((int)result.StatusCode, result.Response);
    }

    [HttpGet("{assetId:long}/object-tree")]
    public async Task<ActionResult<ApiResponse<ObjectTreeResponse>>> GetObjectTree(
        [FromRoute] long assetId,
        [FromQuery] long? versionId,
        [FromQuery] string? mode,
        CancellationToken cancellationToken)
    {
        var result = await _objectTreeModelStatsService.GetObjectTreeAsync(new GetObjectTreeRequest(assetId, versionId, mode), cancellationToken);
        return StatusCode((int)result.StatusCode, result.Response);
    }

    [HttpPut("{assetId:long}/versions/{versionId:long}/model-stats")]
    public async Task<ActionResult<ApiResponse<ModelStatsResponse>>> SaveModelStats(
        [FromRoute] long assetId,
        [FromRoute] long versionId,
        [FromBody] SaveModelStatsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _objectTreeModelStatsService.SaveModelStatsAsync(assetId, versionId, request, cancellationToken);
        return StatusCode((int)result.StatusCode, result.Response);
    }

    [HttpGet("{assetId:long}/versions/{versionId:long}/model-stats")]
    public async Task<ActionResult<ApiResponse<ModelStatsResponse>>> GetModelStats(
        [FromRoute] long assetId,
        [FromRoute] long versionId,
        CancellationToken cancellationToken)
    {
        var result = await _objectTreeModelStatsService.GetModelStatsAsync(new GetModelStatsRequest(assetId, versionId), cancellationToken);
        return StatusCode((int)result.StatusCode, result.Response);
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<UploadModelAssetResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UploadModelAssetResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<UploadModelAssetResponse>), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResponse<UploadModelAssetResponse>>> Upload(
        [FromForm] UploadModelAssetFormRequest form,
        CancellationToken cancellationToken)
    {
        if (form.File is null)
        {
            var response = ApiResponse<UploadModelAssetResponse>.Failure(
                ErrorCode.ValidationFailed,
                "上传参数无效。",
                new[] { new ApiErrorItem("file", "文件不能为空。") });

            return BadRequest(response);
        }

        var request = new UploadModelAssetRequest(
            form.AssetCode,
            form.AssetName,
            form.AssetType,
            form.SourceFileType);

        await using var fileContent = form.File.OpenReadStream();
        var result = await _uploadService.UploadAsync(
            request,
            fileContent,
            form.File.FileName,
            form.File.Length,
            cancellationToken);

        return StatusCode((int)result.StatusCode, result.Response);
    }
}
