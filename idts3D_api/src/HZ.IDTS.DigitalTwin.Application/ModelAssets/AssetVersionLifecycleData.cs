using System.Net;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;
using HZ.IDTS.DigitalTwin.Domain.Enums;

namespace HZ.IDTS.DigitalTwin.Application.ModelAssets;

public enum AssetVersionLifecycleOperation
{
    MarkReady,
    Publish,
    Archive,
    Rollback
}

public sealed record AssetVersionLifecycleCommand(
    long AssetId,
    long VersionId,
    AssetVersionLifecycleOperation Operation,
    string? Remark);

public sealed record AssetVersionLifecycleRepositoryResult(
    bool AssetExists,
    bool VersionExists,
    VersionStatus? VersionStatus,
    DateTime? ChangedTime,
    string? FailureCode,
    string? FailureMessage,
    IReadOnlyList<ApiErrorItem> Errors)
{
    public bool IsSuccess => FailureCode is null && VersionStatus.HasValue && ChangedTime.HasValue;
}

public sealed record AssetVersionStatusChangeResult(
    HttpStatusCode StatusCode,
    ApiResponse<AssetVersionResponse> Response);
