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

    public ModelAssetsController(IModelAssetUploadService uploadService)
    {
        _uploadService = uploadService;
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
