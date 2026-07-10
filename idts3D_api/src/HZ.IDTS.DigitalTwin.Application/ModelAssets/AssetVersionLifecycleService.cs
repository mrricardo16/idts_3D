using System.Net;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

namespace HZ.IDTS.DigitalTwin.Application.ModelAssets;

public sealed class AssetVersionLifecycleService : IAssetVersionLifecycleService
{
    private readonly IModelAssetRepository _repository;

    public AssetVersionLifecycleService(IModelAssetRepository repository)
    {
        _repository = repository;
    }

    public Task<AssetVersionStatusChangeResult> MarkReadyAsync(
        long assetId,
        long versionId,
        ChangeVersionStatusRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(assetId, versionId, request, AssetVersionLifecycleOperation.MarkReady, cancellationToken);

    public Task<AssetVersionStatusChangeResult> PublishAsync(
        long assetId,
        long versionId,
        ChangeVersionStatusRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(assetId, versionId, request, AssetVersionLifecycleOperation.Publish, cancellationToken);

    public Task<AssetVersionStatusChangeResult> ArchiveAsync(
        long assetId,
        long versionId,
        ChangeVersionStatusRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(assetId, versionId, request, AssetVersionLifecycleOperation.Archive, cancellationToken);

    public Task<AssetVersionStatusChangeResult> RollbackAsync(
        long assetId,
        long versionId,
        ChangeVersionStatusRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(assetId, versionId, request, AssetVersionLifecycleOperation.Rollback, cancellationToken);

    private async Task<AssetVersionStatusChangeResult> ExecuteAsync(
        long assetId,
        long versionId,
        ChangeVersionStatusRequest request,
        AssetVersionLifecycleOperation operation,
        CancellationToken cancellationToken)
    {
        if (request.Remark?.Length > 500)
        {
            return Failure(
                HttpStatusCode.BadRequest,
                ErrorCode.ValidationFailed,
                "remark 长度不能超过 500。",
                new[] { new ApiErrorItem("remark", "remark 最大长度为 500。") });
        }

        var execution = await _repository.ExecuteVersionLifecycleAsync(
            new AssetVersionLifecycleCommand(assetId, versionId, operation, request.Remark),
            cancellationToken);

        if (execution.IsSuccess)
        {
            return new AssetVersionStatusChangeResult(
                HttpStatusCode.OK,
                ApiResponse<AssetVersionResponse>.Ok(new AssetVersionResponse(
                    assetId,
                    versionId,
                    execution.VersionStatus!.Value.ToString(),
                    execution.ChangedTime!.Value)));
        }

        if (!execution.AssetExists)
        {
            return Failure(
                HttpStatusCode.NotFound,
                ErrorCode.AssetNotFound,
                "model asset 不存在。",
                new[] { new ApiErrorItem("assetId", $"未找到 assetId={assetId}。") });
        }

        if (!execution.VersionExists)
        {
            return Failure(
                HttpStatusCode.NotFound,
                ErrorCode.VersionNotFound,
                "asset version 不存在。",
                new[] { new ApiErrorItem("versionId", $"未找到 versionId={versionId}。") });
        }

        return Failure(
            execution.FailureCode ?? ErrorCode.InternalError,
            execution.FailureMessage ?? "资产版本状态操作失败。",
            execution.Errors);
    }

    private static AssetVersionStatusChangeResult Failure(
        string code,
        string message,
        IReadOnlyList<ApiErrorItem> errors) =>
        Failure(GetStatusCode(code), code, message, errors);

    private static AssetVersionStatusChangeResult Failure(
        HttpStatusCode statusCode,
        string code,
        string message,
        IReadOnlyList<ApiErrorItem> errors) =>
        new(statusCode, ApiResponse<AssetVersionResponse>.Failure(code, message, errors));

    private static HttpStatusCode GetStatusCode(string code) => code switch
    {
        ErrorCode.ManifestRequired or ErrorCode.ObjectTreeRequired or ErrorCode.ModelStatsRequired or ErrorCode.ValidationFailed => HttpStatusCode.BadRequest,
        ErrorCode.VersionStatusInvalid or ErrorCode.Conflict => HttpStatusCode.Conflict,
        _ => HttpStatusCode.InternalServerError
    };
}
