namespace HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

public sealed record GetModelManifestRequest(
    long AssetId,
    long? VersionId,
    string? Mode);
