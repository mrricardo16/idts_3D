using System.Net;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.MovableParts;
using HZ.IDTS.DigitalTwin.Domain.Enums;

namespace HZ.IDTS.DigitalTwin.Application.MovableParts;

public sealed record MovablePartResult(HttpStatusCode StatusCode, ApiResponse<MovablePartResponse> Response);
public sealed record MovablePartListResult(HttpStatusCode StatusCode, ApiResponse<MovablePartListResponse> Response);
public sealed record MovablePartDeleteResult(HttpStatusCode StatusCode, ApiResponse<DeleteMovablePartResponse> Response);

public sealed class MovablePartService : IMovablePartService
{
    private const string MonitorMode = "monitor";
    private const string EditMode = "edit";
    private readonly IMovablePartRepository _repository;

    public MovablePartService(IMovablePartRepository repository) => _repository = repository;

    public async Task<MovablePartListResult> GetMovablePartsAsync(GetMovablePartsRequest request, CancellationToken cancellationToken)
    {
        if (!ArePositive(request.AssetId, request.VersionId))
        {
            return ListFailure(HttpStatusCode.BadRequest, ErrorCode.ValidationFailed, "assetId 和 versionId 必须大于 0。", "assetId", "必须大于 0。");
        }

        var mode = NormalizeMode(request.Mode);
        if (mode is null)
        {
            return ListFailure(HttpStatusCode.BadRequest, ErrorCode.ValidationFailed, "mode 参数无效。", "mode", "只允许 monitor 或 edit。");
        }

        var versionFailure = await GetVersionFailureAsync(request.AssetId, request.VersionId, cancellationToken);
        if (versionFailure is not null)
        {
            return versionFailure;
        }

        var version = await _repository.GetVersionAsync(request.AssetId, request.VersionId, cancellationToken);
        if (!IsReadable(mode, version.VersionStatus!.Value))
        {
            return ListFailure(HttpStatusCode.Conflict, ErrorCode.VersionStatusInvalid, mode == MonitorMode ? "monitor 模式只能读取 Published 版本。" : "edit 模式只能读取 Draft、Ready、Published 版本。", "versionId", "版本状态不允许当前读取模式。");
        }

        var items = await _repository.GetListAsync(request.AssetId, request.VersionId, request.Enabled, cancellationToken);
        return new MovablePartListResult(HttpStatusCode.OK, ApiResponse<MovablePartListResponse>.Ok(new MovablePartListResponse(items.Select(ToResponse).ToList())));
    }

    public Task<MovablePartResult> CreateAsync(long assetId, long versionId, CreateMovablePartRequest request, CancellationToken cancellationToken) =>
        ExecuteWriteAsync(assetId, versionId, null, request, cancellationToken);

    public Task<MovablePartResult> UpdateAsync(long assetId, long versionId, long partId, UpdateMovablePartRequest request, CancellationToken cancellationToken) =>
        partId <= 0
            ? Task.FromResult(Failure(HttpStatusCode.BadRequest, ErrorCode.ValidationFailed, "partId 必须大于 0。", "partId", "必须大于 0。"))
            : ExecuteWriteAsync(assetId, versionId, partId, new CreateMovablePartRequest(request.ObjectUuid, request.ObjectPath, request.BusinessName, request.PartCode, request.MotionType, request.AxisMode, request.Axis, request.CustomAxisX, request.CustomAxisY, request.CustomAxisZ, request.MinValue, request.MaxValue, request.HomeValue, request.CurrentValue, request.DefaultSpeed, request.Enabled), cancellationToken);

    public async Task<MovablePartDeleteResult> DeleteAsync(long assetId, long versionId, long partId, CancellationToken cancellationToken)
    {
        if (!ArePositive(assetId, versionId, partId))
        {
            return DeleteFailure(HttpStatusCode.BadRequest, ErrorCode.ValidationFailed, "assetId、versionId 和 partId 必须大于 0。", "partId", "必须大于 0。");
        }

        var writeResult = await _repository.DeleteAsync(new DeleteMovablePartCommand(assetId, versionId, partId), cancellationToken);
        if (writeResult.Failure != MovablePartWriteFailure.None)
        {
            return DeleteFailure(writeResult, partId);
        }

        return new MovablePartDeleteResult(HttpStatusCode.OK, ApiResponse<DeleteMovablePartResponse>.Ok(new DeleteMovablePartResponse(partId, true)));
    }

    private async Task<MovablePartResult> ExecuteWriteAsync(long assetId, long versionId, long? partId, CreateMovablePartRequest request, CancellationToken cancellationToken)
    {
        var normalized = NormalizeAndValidate(assetId, versionId, request);
        if (normalized.Errors.Count > 0)
        {
            return Failure(HttpStatusCode.BadRequest, ErrorCode.ValidationFailed, "可动部件参数无效。", normalized.Errors);
        }

        var version = await _repository.GetVersionAsync(assetId, versionId, cancellationToken);
        var versionFailure = WriteVersionFailure(version);
        if (versionFailure is not null)
        {
            return versionFailure;
        }

        var objectResolution = await _repository.ResolveCanonicalObjectAsync(assetId, versionId, normalized.ObjectUuid, normalized.ObjectPath, cancellationToken);
        var objectFailure = ObjectFailure(objectResolution);
        if (objectFailure is not null)
        {
            return objectFailure;
        }

        var command = new CreateMovablePartCommand(assetId, versionId, normalized.ObjectUuid, normalized.ObjectPath, normalized.BusinessName!, normalized.PartCode!, MotionType.linear, AxisMode.world, Axis.z, null, null, null, normalized.MinValue!.Value, normalized.MaxValue!.Value, normalized.HomeValue!.Value, normalized.CurrentValue!.Value, normalized.DefaultSpeed!.Value, BindingStatus.active, normalized.Enabled!.Value);
        var writeResult = partId.HasValue
            ? await _repository.UpdateAsync(new UpdateMovablePartCommand(command.AssetId, command.VersionId, partId.Value, command.ObjectUuid, command.ObjectPath, command.BusinessName, command.PartCode, command.MotionType, command.AxisMode, command.Axis, null, null, null, command.MinValue, command.MaxValue, command.HomeValue, command.CurrentValue, command.DefaultSpeed, command.BindingStatus, command.Enabled), cancellationToken)
            : await _repository.CreateAsync(command, cancellationToken);

        return writeResult.Failure == MovablePartWriteFailure.None && writeResult.Data is not null
            ? new MovablePartResult(HttpStatusCode.OK, ApiResponse<MovablePartResponse>.Ok(ToResponse(writeResult.Data)))
            : Failure(writeResult);
    }

    private async Task<MovablePartListResult?> GetVersionFailureAsync(long assetId, long versionId, CancellationToken cancellationToken)
    {
        var version = await _repository.GetVersionAsync(assetId, versionId, cancellationToken);
        if (!version.AssetExists)
        {
            return ListFailure(HttpStatusCode.NotFound, ErrorCode.AssetNotFound, "model asset 不存在。", "assetId", "未找到 asset。 ");
        }

        return version.VersionExists ? null : ListFailure(HttpStatusCode.NotFound, ErrorCode.VersionNotFound, "asset version 不存在。", "versionId", "未找到 version。");
    }

    private static MovablePartResult? WriteVersionFailure(MovablePartVersionLookup version)
    {
        if (!version.AssetExists) return Failure(HttpStatusCode.NotFound, ErrorCode.AssetNotFound, "model asset 不存在。", "assetId", "未找到 asset。");
        if (!version.VersionExists) return Failure(HttpStatusCode.NotFound, ErrorCode.VersionNotFound, "asset version 不存在。", "versionId", "未找到 version。");
        return version.VersionStatus is VersionStatus.Draft or VersionStatus.Ready ? null : Failure(HttpStatusCode.Conflict, ErrorCode.VersionStatusInvalid, "当前版本不允许写入可动部件配置。", "versionId", "只允许 Draft 或 Ready。");
    }

    private static MovablePartResult? ObjectFailure(MovablePartObjectResolution resolution) => resolution.Kind switch
    {
        MovablePartObjectResolutionKind.Found => null,
        MovablePartObjectResolutionKind.Conflict => Failure(HttpStatusCode.BadRequest, ErrorCode.ValidationFailed, "objectUuid 和 objectPath 必须命中同一对象。", "objectPath", "对象标识不一致。"),
        _ => Failure(HttpStatusCode.NotFound, ErrorCode.NotFound, "object tree 中找不到该对象。", "objectUuid", "未找到对象。")
    };

    private static bool IsReadable(string mode, VersionStatus versionStatus) =>
        mode == MonitorMode ? versionStatus == VersionStatus.Published : versionStatus is VersionStatus.Draft or VersionStatus.Ready or VersionStatus.Published;

    private static string? NormalizeMode(string? mode)
    {
        var value = string.IsNullOrWhiteSpace(mode) ? MonitorMode : mode.Trim().ToLowerInvariant();
        return value is MonitorMode or EditMode ? value : null;
    }

    private static NormalizedRequest NormalizeAndValidate(long assetId, long versionId, CreateMovablePartRequest request)
    {
        var errors = new List<ApiErrorItem>();
        if (!ArePositive(assetId, versionId)) errors.Add(new ApiErrorItem("assetId", "assetId 和 versionId 必须大于 0。"));
        var objectUuid = Normalize(request.ObjectUuid);
        var objectPath = Normalize(request.ObjectPath);
        var businessName = Normalize(request.BusinessName);
        var partCode = Normalize(request.PartCode)?.ToUpperInvariant();
        if (objectUuid is null && objectPath is null) errors.Add(new ApiErrorItem("objectUuid", "objectUuid 和 objectPath 至少提供一个。"));
        if (objectUuid?.Length > 100) errors.Add(new ApiErrorItem("objectUuid", "长度不能超过 100。"));
        if (objectPath?.Length > 1000) errors.Add(new ApiErrorItem("objectPath", "长度不能超过 1000。"));
        if (string.IsNullOrWhiteSpace(businessName)) errors.Add(new ApiErrorItem("businessName", "不能为空。")); else if (businessName.Length > 200) errors.Add(new ApiErrorItem("businessName", "长度不能超过 200。"));
        if (string.IsNullOrWhiteSpace(partCode)) errors.Add(new ApiErrorItem("partCode", "不能为空。")); else if (partCode.Length > 100) errors.Add(new ApiErrorItem("partCode", "长度不能超过 100。"));
        if (!string.Equals(Normalize(request.MotionType), "linear", StringComparison.OrdinalIgnoreCase)) errors.Add(new ApiErrorItem("motionType", "MVP-07 只允许 linear。"));
        if (!string.Equals(Normalize(request.AxisMode), "world", StringComparison.OrdinalIgnoreCase)) errors.Add(new ApiErrorItem("axisMode", "MVP-07 只允许 world。"));
        if (!string.Equals(Normalize(request.Axis), "z", StringComparison.OrdinalIgnoreCase)) errors.Add(new ApiErrorItem("axis", "MVP-07 只允许 z。"));
        if (request.CustomAxisX.HasValue || request.CustomAxisY.HasValue || request.CustomAxisZ.HasValue) errors.Add(new ApiErrorItem("customAxisX", "MVP-07 不允许 custom axis。"));
        if (!request.MinValue.HasValue || !request.MaxValue.HasValue || !request.HomeValue.HasValue || !request.CurrentValue.HasValue || !request.DefaultSpeed.HasValue) errors.Add(new ApiErrorItem("minValue", "运动数值均为必填项。"));
        else
        {
            if (request.MinValue > request.HomeValue || request.HomeValue > request.MaxValue) errors.Add(new ApiErrorItem("homeValue", "必须满足 minValue <= homeValue <= maxValue。"));
            if (request.MinValue > request.CurrentValue || request.CurrentValue > request.MaxValue) errors.Add(new ApiErrorItem("currentValue", "必须在 minValue 和 maxValue 之间。"));
            if (request.DefaultSpeed <= 0) errors.Add(new ApiErrorItem("defaultSpeed", "必须大于 0。"));
        }
        if (!request.Enabled.HasValue) errors.Add(new ApiErrorItem("enabled", "必须显式提供。"));
        return new NormalizedRequest(objectUuid, objectPath, businessName, partCode, request.MinValue, request.MaxValue, request.HomeValue, request.CurrentValue, request.DefaultSpeed, request.Enabled, errors);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static bool ArePositive(params long[] values) => values.All(x => x > 0);
    private static MovablePartResponse ToResponse(MovablePartData value) => new(value.PartId, value.AssetId, value.VersionId, value.ObjectUuid, value.ObjectName, value.ObjectPath, value.ParentUuid, value.ParentPath, value.BusinessName, value.PartCode, value.MotionType.ToString(), value.AxisMode.ToString(), value.Axis.ToString(), value.CustomAxisX, value.CustomAxisY, value.CustomAxisZ, value.MinValue, value.MaxValue, value.HomeValue, value.CurrentValue, value.DefaultSpeed, value.BindingStatus.ToString(), value.Enabled);

    private static MovablePartResult Failure(MovablePartWriteResult result) => result.Failure switch
    {
        MovablePartWriteFailure.AssetNotFound => Failure(HttpStatusCode.NotFound, ErrorCode.AssetNotFound, "model asset 不存在。", "assetId", "未找到 asset。"),
        MovablePartWriteFailure.VersionNotFound => Failure(HttpStatusCode.NotFound, ErrorCode.VersionNotFound, "asset version 不存在。", "versionId", "未找到 version。"),
        MovablePartWriteFailure.VersionNotWritable => Failure(HttpStatusCode.Conflict, ErrorCode.VersionStatusInvalid, "当前版本不允许写入可动部件配置。", "versionId", "只允许 Draft 或 Ready。"),
        MovablePartWriteFailure.ObjectNotFound => Failure(HttpStatusCode.NotFound, ErrorCode.NotFound, "object tree 中找不到该对象。", "objectUuid", "未找到对象。"),
        MovablePartWriteFailure.ObjectConflict => Failure(HttpStatusCode.BadRequest, ErrorCode.ValidationFailed, "objectUuid 和 objectPath 必须命中同一对象。", "objectPath", "对象标识不一致。"),
        MovablePartWriteFailure.DuplicatePartCode => Failure(HttpStatusCode.Conflict, ErrorCode.DuplicatePartCode, "partCode 已存在。", "partCode", "当前版本中已存在。"),
        MovablePartWriteFailure.PartNotFound => Failure(HttpStatusCode.NotFound, ErrorCode.NotFound, "movable part 不存在。", "partId", "未找到可动部件。"),
        _ => Failure(HttpStatusCode.InternalServerError, ErrorCode.InternalError, "movable part 写入失败。", "", "未返回写入结果。")
    };

    private static MovablePartDeleteResult DeleteFailure(MovablePartWriteResult result, long partId)
    {
        var failure = Failure(result);
        return new MovablePartDeleteResult(failure.StatusCode, ApiResponse<DeleteMovablePartResponse>.Failure(failure.Response.Code, failure.Response.Message, failure.Response.Errors));
    }

    private static MovablePartResult Failure(HttpStatusCode statusCode, string code, string message, string field, string error) => Failure(statusCode, code, message, new[] { new ApiErrorItem(field, error) });
    private static MovablePartResult Failure(HttpStatusCode statusCode, string code, string message, IReadOnlyList<ApiErrorItem> errors) => new(statusCode, ApiResponse<MovablePartResponse>.Failure(code, message, errors));
    private static MovablePartListResult ListFailure(HttpStatusCode statusCode, string code, string message, string field, string error) => new(statusCode, ApiResponse<MovablePartListResponse>.Failure(code, message, new[] { new ApiErrorItem(field, error) }));
    private static MovablePartDeleteResult DeleteFailure(HttpStatusCode statusCode, string code, string message, string field, string error) => new(statusCode, ApiResponse<DeleteMovablePartResponse>.Failure(code, message, new[] { new ApiErrorItem(field, error) }));
    private sealed record NormalizedRequest(string? ObjectUuid, string? ObjectPath, string? BusinessName, string? PartCode, decimal? MinValue, decimal? MaxValue, decimal? HomeValue, decimal? CurrentValue, decimal? DefaultSpeed, bool? Enabled, IReadOnlyList<ApiErrorItem> Errors);
}
