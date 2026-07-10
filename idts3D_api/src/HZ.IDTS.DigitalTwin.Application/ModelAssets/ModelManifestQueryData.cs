using HZ.IDTS.DigitalTwin.Domain.Enums;

namespace HZ.IDTS.DigitalTwin.Application.ModelAssets;

public sealed record ModelManifestQueryData(
    long AssetId,
    long VersionId,
    string AssetCode,
    string AssetName,
    VersionStatus VersionStatus,
    IReadOnlyList<ModelManifestVariantData> Variants,
    string? ManifestJson,
    IReadOnlyList<ModelManifestMovablePartData> MovableParts);

public sealed record ModelManifestVariantData(
    VariantLevel VariantLevel,
    string FileUrl);

public sealed record ModelManifestMovablePartData(
    long PartId,
    string PartCode,
    string BusinessName,
    MotionType MotionType,
    AxisMode AxisMode,
    Axis Axis,
    IReadOnlyList<ModelManifestMotionTargetData> Targets);

public sealed record ModelManifestMotionTargetData(
    long TargetId,
    string TargetCode,
    string TargetName,
    decimal? TargetValue);
