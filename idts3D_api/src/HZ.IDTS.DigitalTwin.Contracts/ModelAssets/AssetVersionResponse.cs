namespace HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

public sealed record AssetVersionResponse(
    long AssetId,
    long VersionId,
    string VersionStatus,
    DateTime ChangedTime);
