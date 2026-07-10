namespace HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

public sealed record ModelManifestResponse(
    long AssetId,
    long VersionId,
    string AssetCode,
    string AssetName,
    string VersionStatus,
    ModelManifestLevelsResponse Levels,
    ModelTransformResponse Transform,
    IReadOnlyList<ModelManifestMovablePartResponse> MovableParts);

public sealed record ModelManifestLevelsResponse(
    string? Source,
    string? High,
    string? Medium,
    string? Low,
    string? Proxy);

public sealed record ModelTransformResponse(
    ManifestVector3Response Position,
    ManifestVector3Response RotationDeg,
    ManifestVector3Response Scale);

public sealed record ManifestVector3Response(
    decimal X,
    decimal Y,
    decimal Z);

public sealed record ModelManifestMovablePartResponse(
    long PartId,
    string PartCode,
    string BusinessName,
    string MotionType,
    string AxisMode,
    string Axis,
    IReadOnlyList<ModelManifestMotionTargetResponse> Targets);

public sealed record ModelManifestMotionTargetResponse(
    long TargetId,
    string TargetCode,
    string TargetName,
    decimal? TargetValue);
