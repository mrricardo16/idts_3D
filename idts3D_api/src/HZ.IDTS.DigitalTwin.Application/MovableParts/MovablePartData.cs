using HZ.IDTS.DigitalTwin.Domain.Enums;

namespace HZ.IDTS.DigitalTwin.Application.MovableParts;

public sealed record MovablePartVersionLookup(bool AssetExists, bool VersionExists, VersionStatus? VersionStatus);

public enum MovablePartObjectResolutionKind
{
    Found,
    NotFound,
    Conflict
}

public sealed record MovablePartCanonicalObject(string ObjectUuid, string ObjectName, string ObjectPath, string? ParentUuid, string? ParentPath);
public sealed record MovablePartObjectResolution(MovablePartObjectResolutionKind Kind, MovablePartCanonicalObject? Object);

public sealed record MovablePartData(
    long PartId, long AssetId, long VersionId, string? ObjectUuid, string ObjectName, string? ObjectPath,
    string? ParentUuid, string? ParentPath, string BusinessName, string PartCode, MotionType MotionType,
    AxisMode AxisMode, Axis Axis, decimal? CustomAxisX, decimal? CustomAxisY, decimal? CustomAxisZ,
    decimal MinValue, decimal MaxValue, decimal HomeValue, decimal CurrentValue, decimal DefaultSpeed,
    BindingStatus BindingStatus, bool Enabled);

public sealed record CreateMovablePartCommand(
    long AssetId, long VersionId, string? ObjectUuid, string? ObjectPath, string BusinessName, string PartCode,
    MotionType MotionType, AxisMode AxisMode, Axis Axis, decimal? CustomAxisX, decimal? CustomAxisY,
    decimal? CustomAxisZ, decimal MinValue, decimal MaxValue, decimal HomeValue, decimal CurrentValue,
    decimal DefaultSpeed, BindingStatus BindingStatus, bool Enabled);

public sealed record UpdateMovablePartCommand(
    long AssetId, long VersionId, long PartId, string? ObjectUuid, string? ObjectPath, string BusinessName,
    string PartCode, MotionType MotionType, AxisMode AxisMode, Axis Axis, decimal? CustomAxisX,
    decimal? CustomAxisY, decimal? CustomAxisZ, decimal MinValue, decimal MaxValue, decimal HomeValue,
    decimal CurrentValue, decimal DefaultSpeed, BindingStatus BindingStatus, bool Enabled);

public sealed record DeleteMovablePartCommand(long AssetId, long VersionId, long PartId);

public enum MovablePartWriteFailure
{
    None,
    AssetNotFound,
    VersionNotFound,
    VersionNotWritable,
    ObjectNotFound,
    ObjectConflict,
    DuplicatePartCode,
    PartNotFound
}

public sealed record MovablePartWriteResult(MovablePartWriteFailure Failure, MovablePartData? Data);
