namespace HZ.IDTS.DigitalTwin.Contracts.MotionTargets;

public sealed record GetMotionTargetsRequest(long PartId, bool? Enabled, string? Mode);

public sealed record CreateMotionTargetRequest(
    string? TargetCode,
    string? TargetName,
    decimal? TargetValue,
    decimal? TargetX,
    decimal? TargetY,
    decimal? TargetZ,
    int? SortNo,
    bool? Enabled);

public sealed record UpdateMotionTargetRequest(
    string? TargetCode,
    string? TargetName,
    decimal? TargetValue,
    decimal? TargetX,
    decimal? TargetY,
    decimal? TargetZ,
    int? SortNo,
    bool? Enabled);

public sealed record MotionTargetResponse(
    long TargetId,
    long PartId,
    string TargetCode,
    string TargetName,
    decimal TargetValue,
    decimal? TargetX,
    decimal? TargetY,
    decimal TargetZ,
    int SortNo,
    bool Enabled);

public sealed record MotionTargetListResponse(long PartId, IReadOnlyList<MotionTargetResponse> Items);

public sealed record DeleteMotionTargetResponse(long TargetId, bool Deleted);
