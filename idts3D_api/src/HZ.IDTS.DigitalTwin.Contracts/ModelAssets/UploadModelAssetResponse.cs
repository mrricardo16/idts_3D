namespace HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

public sealed record UploadModelAssetResponse(
    long AssetId,
    long VersionId,
    long JobId,
    string AssetCode,
    string VersionStatus,
    string ProcessingStatus,
    string SourceFileHash,
    string SourceFileUrl);
