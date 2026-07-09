using Microsoft.AspNetCore.Mvc;

namespace HZ.IDTS.DigitalTwin.Api.Models;

public sealed class UploadModelAssetFormRequest
{
    [FromForm(Name = "file")]
    public IFormFile? File { get; set; }

    [FromForm(Name = "assetCode")]
    public string AssetCode { get; set; } = string.Empty;

    [FromForm(Name = "assetName")]
    public string AssetName { get; set; } = string.Empty;

    [FromForm(Name = "assetType")]
    public string AssetType { get; set; } = string.Empty;

    [FromForm(Name = "sourceFileType")]
    public string SourceFileType { get; set; } = string.Empty;
}
