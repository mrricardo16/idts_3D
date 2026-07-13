namespace HZ.IDTS.DigitalTwin.Contracts.MovableParts;

public sealed record GetMovablePartsRequest(long AssetId, long VersionId, bool? Enabled, string? Mode);

public sealed record CreateMovablePartRequest(
    string? ObjectUuid,
    string? ObjectPath,
    string? BusinessName,
    string? PartCode,
    string? MotionType,
    string? AxisMode,
    string? Axis,
    decimal? CustomAxisX,
    decimal? CustomAxisY,
    decimal? CustomAxisZ,
    decimal? MinValue,
    decimal? MaxValue,
    decimal? HomeValue,
    decimal? CurrentValue,
    decimal? DefaultSpeed,
    bool? Enabled);

public sealed record UpdateMovablePartRequest(
    string? ObjectUuid,
    string? ObjectPath,
    string? BusinessName,
    string? PartCode,
    string? MotionType,
    string? AxisMode,
    string? Axis,
    decimal? CustomAxisX,
    decimal? CustomAxisY,
    decimal? CustomAxisZ,
    decimal? MinValue,
    decimal? MaxValue,
    decimal? HomeValue,
    decimal? CurrentValue,
    decimal? DefaultSpeed,
    bool? Enabled);

public sealed record MovablePartResponse(
    long PartId,
    long AssetId,
    long VersionId,
    string? ObjectUuid,
    string ObjectName,
    string? ObjectPath,
    string? ParentUuid,
    string? ParentPath,
    string BusinessName,
    string PartCode,
    string MotionType,
    string AxisMode,
    string Axis,
    decimal? CustomAxisX,
    decimal? CustomAxisY,
    decimal? CustomAxisZ,
    decimal MinValue,
    decimal MaxValue,
    decimal HomeValue,
    decimal CurrentValue,
    decimal DefaultSpeed,
    string BindingStatus,
    bool Enabled);

public sealed record MovablePartListResponse(IReadOnlyList<MovablePartResponse> Items);

public sealed record DeleteMovablePartResponse(long PartId, bool Deleted);
