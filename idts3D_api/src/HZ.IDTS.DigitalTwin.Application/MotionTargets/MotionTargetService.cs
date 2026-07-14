using System.Net;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.MotionTargets;
using HZ.IDTS.DigitalTwin.Domain.Enums;

namespace HZ.IDTS.DigitalTwin.Application.MotionTargets;

public sealed class MotionTargetService : IMotionTargetService
{
    private const string MonitorMode = "monitor";
    private const string EditMode = "edit";
    private readonly IMotionTargetRepository _repository;

    public MotionTargetService(IMotionTargetRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public async Task<MotionTargetListResult> GetMotionTargetsAsync(GetMotionTargetsRequest request, CancellationToken cancellationToken)
    {
        if (request.PartId <= 0)
        {
            return ListFailure(HttpStatusCode.BadRequest, ErrorCode.ValidationFailed, "partId 必须大于 0。", "partId", "必须大于 0。");
        }

        var mode = NormalizeMode(request.Mode);
        if (mode is null)
        {
            return ListFailure(HttpStatusCode.BadRequest, ErrorCode.ValidationFailed, "mode 参数无效。", "mode", "只允许 monitor 或 edit。");
        }

        var part = await _repository.GetPartAsync(request.PartId, cancellationToken);
        var lookupFailure = ListLookupFailure(part);
        if (lookupFailure is not null) return lookupFailure;
        if (!IsReadable(mode, part.VersionStatus!.Value))
        {
            return ListFailure(HttpStatusCode.Conflict, ErrorCode.VersionStatusInvalid, "当前版本状态不允许读取 motion target。", "mode", "monitor 仅允许 Published；edit 仅允许 Draft、Ready、Published。");
        }

        var items = await _repository.GetListAsync(request.PartId, request.Enabled, cancellationToken);
        return new MotionTargetListResult(HttpStatusCode.OK, ApiResponse<MotionTargetListResponse>.Ok(new MotionTargetListResponse(request.PartId, items.Select(ToResponse).ToList())));
    }

    public Task<MotionTargetResult> CreateAsync(long partId, CreateMotionTargetRequest request, CancellationToken cancellationToken) =>
        ExecuteWriteAsync(partId, null, request, cancellationToken);

    public Task<MotionTargetResult> UpdateAsync(long partId, long targetId, UpdateMotionTargetRequest request, CancellationToken cancellationToken) =>
        targetId <= 0
            ? Task.FromResult(Failure(HttpStatusCode.BadRequest, ErrorCode.ValidationFailed, "targetId 必须大于 0。", "targetId", "必须大于 0。"))
            : ExecuteWriteAsync(partId, targetId, new CreateMotionTargetRequest(request.TargetCode, request.TargetName, request.TargetValue, request.TargetX, request.TargetY, request.TargetZ, request.SortNo, request.Enabled), cancellationToken);

    public async Task<MotionTargetDeleteResult> DeleteAsync(long partId, long targetId, CancellationToken cancellationToken)
    {
        if (partId <= 0 || targetId <= 0)
        {
            return DeleteFailure(HttpStatusCode.BadRequest, ErrorCode.ValidationFailed, "partId 和 targetId 必须大于 0。", "targetId", "必须大于 0。");
        }

        var part = await _repository.GetPartAsync(partId, cancellationToken);
        var failure = WriteLookupFailure(part);
        if (failure is not null) return DeleteFailure(failure.StatusCode, failure.Response.Code, failure.Response.Message, failure.Response.Errors);

        var result = await _repository.DeleteAsync(new DeleteMotionTargetCommand(partId, targetId), cancellationToken);
        if (result.Failure != MotionTargetWriteFailure.None)
        {
            var writeFailure = Failure(result);
            return DeleteFailure(writeFailure.StatusCode, writeFailure.Response.Code, writeFailure.Response.Message, writeFailure.Response.Errors);
        }

        return new MotionTargetDeleteResult(HttpStatusCode.OK, ApiResponse<DeleteMotionTargetResponse>.Ok(new DeleteMotionTargetResponse(targetId, true)));
    }

    private async Task<MotionTargetResult> ExecuteWriteAsync(long partId, long? targetId, CreateMotionTargetRequest request, CancellationToken cancellationToken)
    {
        var normalized = NormalizeAndValidate(partId, request);
        if (normalized.Errors.Count > 0)
        {
            return Failure(HttpStatusCode.BadRequest, ErrorCode.ValidationFailed, "motion target 参数无效。", normalized.Errors);
        }

        var part = await _repository.GetPartAsync(partId, cancellationToken);
        var lookupFailure = WriteLookupFailure(part);
        if (lookupFailure is not null) return lookupFailure;
        if (normalized.TargetValue < part.MinValue || normalized.TargetValue > part.MaxValue)
        {
            return Failure(HttpStatusCode.BadRequest, ErrorCode.ValidationFailed, "targetValue 超出 movable part 运动范围。", "targetValue", "必须在 minValue 和 maxValue 之间。");
        }

        var result = targetId.HasValue
            ? await _repository.UpdateAsync(new UpdateMotionTargetCommand(partId, targetId.Value, normalized.TargetCode!, normalized.TargetName!, normalized.TargetValue!.Value, normalized.TargetValue.Value, normalized.SortNo!.Value, normalized.Enabled!.Value), cancellationToken)
            : await _repository.CreateAsync(new CreateMotionTargetCommand(partId, normalized.TargetCode!, normalized.TargetName!, normalized.TargetValue!.Value, normalized.TargetValue.Value, normalized.SortNo!.Value, normalized.Enabled!.Value), cancellationToken);

        return result.Failure == MotionTargetWriteFailure.None && result.Data is not null
            ? new MotionTargetResult(HttpStatusCode.OK, ApiResponse<MotionTargetResponse>.Ok(ToResponse(result.Data)))
            : Failure(result);
    }

    private static MotionTargetListResult? ListLookupFailure(MotionTargetPartLookup part)
    {
        if (!part.PartExists) return ListFailure(HttpStatusCode.NotFound, ErrorCode.MovablePartNotFound, "movable part 不存在。", "partId", "未找到 movable part。");
        if (!part.VersionExists) return ListFailure(HttpStatusCode.NotFound, ErrorCode.NotFound, "movable part 所属版本不存在。", "partId", "未找到所属版本。");
        return null;
    }

    private static MotionTargetResult? WriteLookupFailure(MotionTargetPartLookup part)
    {
        if (!part.PartExists) return Failure(HttpStatusCode.NotFound, ErrorCode.MovablePartNotFound, "movable part 不存在。", "partId", "未找到 movable part。");
        if (!part.VersionExists) return Failure(HttpStatusCode.NotFound, ErrorCode.NotFound, "movable part 所属版本不存在。", "partId", "未找到所属版本。");
        if (part.VersionStatus is not VersionStatus.Draft and not VersionStatus.Ready) return Failure(HttpStatusCode.Conflict, ErrorCode.VersionStatusInvalid, "当前版本不允许写入 motion target。", "partId", "只允许 Draft 或 Ready。");
        if (part.BindingStatus != BindingStatus.active) return Failure(HttpStatusCode.Conflict, ErrorCode.VersionStatusInvalid, "movable part 的 bindingStatus 必须为 active。", "partId", "bindingStatus 必须为 active。");
        if (part.MotionType != MotionType.linear || part.AxisMode != AxisMode.world || part.Axis != Axis.z) return Failure(HttpStatusCode.BadRequest, ErrorCode.ValidationFailed, "MVP-08 仅支持 linear + world + z。", "partId", "movable part 运动配置不受支持。");
        if (!part.MinValue.HasValue || !part.MaxValue.HasValue) return Failure(HttpStatusCode.BadRequest, ErrorCode.ValidationFailed, "movable part 缺少运动范围。", "partId", "minValue 和 maxValue 必须存在。");
        return null;
    }

    private static NormalizedRequest NormalizeAndValidate(long partId, CreateMotionTargetRequest request)
    {
        var errors = new List<ApiErrorItem>();
        var targetCode = Normalize(request.TargetCode)?.ToUpperInvariant();
        var targetName = Normalize(request.TargetName);
        if (partId <= 0) errors.Add(new ApiErrorItem("partId", "必须大于 0。"));
        if (string.IsNullOrWhiteSpace(targetCode)) errors.Add(new ApiErrorItem("targetCode", "不能为空。")); else if (targetCode.Length > 100) errors.Add(new ApiErrorItem("targetCode", "长度不能超过 100。"));
        if (string.IsNullOrWhiteSpace(targetName)) errors.Add(new ApiErrorItem("targetName", "不能为空。")); else if (targetName.Length > 200) errors.Add(new ApiErrorItem("targetName", "长度不能超过 200。"));
        if (!request.TargetValue.HasValue) errors.Add(new ApiErrorItem("targetValue", "必须显式提供。"));
        if (request.TargetX.HasValue) errors.Add(new ApiErrorItem("targetX", "MVP-08 必须为 null。"));
        if (request.TargetY.HasValue) errors.Add(new ApiErrorItem("targetY", "MVP-08 必须为 null。"));
        if (request.TargetZ.HasValue && request.TargetValue.HasValue && request.TargetZ.Value != request.TargetValue.Value) errors.Add(new ApiErrorItem("targetZ", "必须等于 targetValue。"));
        if (!request.SortNo.HasValue) errors.Add(new ApiErrorItem("sortNo", "必须显式提供。")); else if (request.SortNo < 0) errors.Add(new ApiErrorItem("sortNo", "必须大于或等于 0。"));
        if (!request.Enabled.HasValue) errors.Add(new ApiErrorItem("enabled", "必须显式提供。"));
        return new NormalizedRequest(targetCode, targetName, request.TargetValue, request.SortNo, request.Enabled, errors);
    }

    private static bool IsReadable(string mode, VersionStatus status) =>
        mode == MonitorMode ? status == VersionStatus.Published : status is VersionStatus.Draft or VersionStatus.Ready or VersionStatus.Published;

    private static string? NormalizeMode(string? mode)
    {
        var value = string.IsNullOrWhiteSpace(mode) ? MonitorMode : mode.Trim().ToLowerInvariant();
        return value is MonitorMode or EditMode ? value : null;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static MotionTargetResponse ToResponse(MotionTargetData item) => new(item.TargetId, item.PartId, item.TargetCode, item.TargetName, item.TargetValue, item.TargetX, item.TargetY, item.TargetZ, item.SortNo, item.Enabled);

    private static MotionTargetResult Failure(MotionTargetWriteResult result) => result.Failure switch
    {
        MotionTargetWriteFailure.PartNotFound => Failure(HttpStatusCode.NotFound, ErrorCode.MovablePartNotFound, "movable part 不存在。", "partId", "未找到 movable part。"),
        MotionTargetWriteFailure.VersionNotFound => Failure(HttpStatusCode.NotFound, ErrorCode.NotFound, "movable part 所属版本不存在。", "partId", "未找到所属版本。"),
        MotionTargetWriteFailure.VersionNotWritable or MotionTargetWriteFailure.PartNotConfigurable => Failure(HttpStatusCode.Conflict, ErrorCode.VersionStatusInvalid, "当前状态不允许写入 motion target。", "partId", "版本或 movable part 状态不允许写入。"),
        MotionTargetWriteFailure.TargetNotFound => Failure(HttpStatusCode.NotFound, ErrorCode.NotFound, "motion target 不存在或不属于该 movable part。", "targetId", "未找到 motion target。"),
        MotionTargetWriteFailure.DuplicateTargetCode => Failure(HttpStatusCode.Conflict, ErrorCode.DuplicateTargetCode, "targetCode 已存在。", "targetCode", "当前 movable part 中已存在。"),
        _ => Failure(HttpStatusCode.InternalServerError, ErrorCode.InternalError, "motion target 写入失败。", "", "未返回写入结果。")
    };

    private static MotionTargetResult Failure(HttpStatusCode statusCode, string code, string message, string field, string error) => Failure(statusCode, code, message, new[] { new ApiErrorItem(field, error) });
    private static MotionTargetResult Failure(HttpStatusCode statusCode, string code, string message, IReadOnlyList<ApiErrorItem> errors) => new(statusCode, ApiResponse<MotionTargetResponse>.Failure(code, message, errors));
    private static MotionTargetListResult ListFailure(HttpStatusCode statusCode, string code, string message, string field, string error) => new(statusCode, ApiResponse<MotionTargetListResponse>.Failure(code, message, new[] { new ApiErrorItem(field, error) }));
    private static MotionTargetDeleteResult DeleteFailure(HttpStatusCode statusCode, string code, string message, string field, string error) => DeleteFailure(statusCode, code, message, new[] { new ApiErrorItem(field, error) });
    private static MotionTargetDeleteResult DeleteFailure(HttpStatusCode statusCode, string code, string message, IReadOnlyList<ApiErrorItem> errors) => new(statusCode, ApiResponse<DeleteMotionTargetResponse>.Failure(code, message, errors));

    private sealed record NormalizedRequest(string? TargetCode, string? TargetName, decimal? TargetValue, int? SortNo, bool? Enabled, IReadOnlyList<ApiErrorItem> Errors);
}
