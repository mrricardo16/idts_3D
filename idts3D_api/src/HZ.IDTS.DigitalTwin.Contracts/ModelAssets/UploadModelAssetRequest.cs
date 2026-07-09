namespace HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

public sealed record UploadModelAssetRequest(
    string AssetCode,
    string AssetName,
    string AssetType,
    string SourceFileType);
