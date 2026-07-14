using System.Net;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.MotionTargets;
using HZ.IDTS.DigitalTwin.Domain.Enums;

namespace HZ.IDTS.DigitalTwin.Application.MotionTargets;

public sealed record MotionTargetData(long TargetId, long PartId, string TargetCode, string TargetName, decimal TargetValue, decimal? TargetX, decimal? TargetY, decimal TargetZ, int SortNo, bool Enabled);
public sealed record MotionTargetPartLookup(bool PartExists, bool VersionExists, VersionStatus? VersionStatus, BindingStatus? BindingStatus, MotionType? MotionType, AxisMode? AxisMode, Axis? Axis, decimal? MinValue, decimal? MaxValue, bool? PartEnabled);
public sealed record CreateMotionTargetCommand(long PartId, string TargetCode, string TargetName, decimal TargetValue, decimal TargetZ, int SortNo, bool Enabled);
public sealed record UpdateMotionTargetCommand(long PartId, long TargetId, string TargetCode, string TargetName, decimal TargetValue, decimal TargetZ, int SortNo, bool Enabled);
public sealed record DeleteMotionTargetCommand(long PartId, long TargetId);

public enum MotionTargetWriteFailure { None, PartNotFound, VersionNotFound, VersionNotWritable, PartNotConfigurable, TargetNotFound, DuplicateTargetCode }
public sealed record MotionTargetWriteResult(MotionTargetWriteFailure Failure, MotionTargetData? Data);
public sealed record MotionTargetResult(HttpStatusCode StatusCode, ApiResponse<MotionTargetResponse> Response);
public sealed record MotionTargetListResult(HttpStatusCode StatusCode, ApiResponse<MotionTargetListResponse> Response);
public sealed record MotionTargetDeleteResult(HttpStatusCode StatusCode, ApiResponse<DeleteMotionTargetResponse> Response);
