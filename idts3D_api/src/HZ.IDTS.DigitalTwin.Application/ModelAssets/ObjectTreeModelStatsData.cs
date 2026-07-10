using HZ.IDTS.DigitalTwin.Domain.Enums;

namespace HZ.IDTS.DigitalTwin.Application.ModelAssets;

public sealed record AssetVersionAccessData(
    long AssetId,
    long VersionId,
    VersionStatus VersionStatus);

public sealed record ModelObjectIndexData(
    string ObjectUuid,
    string ObjectName,
    string ObjectPath,
    string? ParentUuid,
    string? ParentPath,
    string ObjectType,
    decimal? BoundingBoxMinX,
    decimal? BoundingBoxMinY,
    decimal? BoundingBoxMinZ,
    decimal? BoundingBoxMaxX,
    decimal? BoundingBoxMaxY,
    decimal? BoundingBoxMaxZ,
    string? MeshFingerprint);

public sealed record SaveModelStatsData(
    DateTime SavedTime);
