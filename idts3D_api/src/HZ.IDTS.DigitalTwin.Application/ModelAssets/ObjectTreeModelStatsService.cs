using System.Net;
using System.Text.Json;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;
using HZ.IDTS.DigitalTwin.Domain.Enums;

namespace HZ.IDTS.DigitalTwin.Application.ModelAssets;

public sealed class ObjectTreeModelStatsService : IObjectTreeModelStatsService
{
    private const string MonitorMode = "monitor";
    private const string EditMode = "edit";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IModelAssetRepository _repository;

    public ObjectTreeModelStatsService(IModelAssetRepository repository)
    {
        _repository = repository;
    }

    public async Task<ObjectTreeResult> SaveObjectTreeAsync(
        long assetId,
        long versionId,
        SaveObjectTreeRequest request,
        CancellationToken cancellationToken)
    {
        var validationErrors = ValidateObjectTree(request);
        if (validationErrors.Count > 0)
        {
            return ObjectTreeFailure(HttpStatusCode.BadRequest, ErrorCode.ValidationFailed, "object tree 参数无效。", validationErrors);
        }

        var version = await GetAssetVersionAsync(assetId, versionId, cancellationToken);
        if (version is null)
        {
            return await ObjectTreeVersionNotFoundAsync(assetId, versionId, cancellationToken);
        }

        if (version.VersionStatus == VersionStatus.Published)
        {
            return ObjectTreeFailure(HttpStatusCode.Conflict, ErrorCode.VersionStatusInvalid, "Published 版本不能直接覆盖 object tree。", Array.Empty<ApiErrorItem>());
        }

        var savedTime = await _repository.ReplaceObjectTreeAsync(assetId, versionId, request.Nodes!, cancellationToken);
        var response = new ObjectTreeResponse(assetId, versionId, request.Nodes!.Count, null, savedTime);
        return new ObjectTreeResult(HttpStatusCode.OK, ApiResponse<ObjectTreeResponse>.Ok(response));
    }

    public async Task<ObjectTreeResult> GetObjectTreeAsync(
        GetObjectTreeRequest request,
        CancellationToken cancellationToken)
    {
        var mode = NormalizeMode(request.Mode);
        if (mode is null)
        {
            return ObjectTreeFailure(HttpStatusCode.BadRequest, ErrorCode.ValidationFailed, "mode 参数无效。", new[] { new ApiErrorItem("mode", "只允许 monitor 或 edit。") });
        }

        var version = await GetAssetVersionAsync(request.AssetId, request.VersionId, cancellationToken, mode == MonitorMode);
        if (version is null)
        {
            return await ObjectTreeVersionNotFoundAsync(request.AssetId, request.VersionId, cancellationToken);
        }

        if (!IsVersionAllowed(mode, version.VersionStatus))
        {
            return ObjectTreeFailure(HttpStatusCode.Conflict, ErrorCode.VersionStatusInvalid, mode == MonitorMode ? "monitor 模式只能读取 Published 版本。" : "edit 模式只能读取 Draft / Ready / Published 版本。", Array.Empty<ApiErrorItem>());
        }

        var nodes = await _repository.GetObjectTreeAsync(request.AssetId, version.VersionId, cancellationToken);
        var response = new ObjectTreeResponse(request.AssetId, version.VersionId, null, nodes.Select(ToObjectTreeNodeResponse).ToList(), null);
        return new ObjectTreeResult(HttpStatusCode.OK, ApiResponse<ObjectTreeResponse>.Ok(response));
    }

    public async Task<ModelStatsResult> SaveModelStatsAsync(
        long assetId,
        long versionId,
        SaveModelStatsRequest request,
        CancellationToken cancellationToken)
    {
        var validationErrors = ValidateModelStats(request);
        if (validationErrors.Count > 0)
        {
            return ModelStatsFailure(HttpStatusCode.BadRequest, ErrorCode.ValidationFailed, "model stats 参数无效。", validationErrors);
        }

        var version = await GetAssetVersionAsync(assetId, versionId, cancellationToken);
        if (version is null)
        {
            return await ModelStatsVersionNotFoundAsync(assetId, versionId, cancellationToken);
        }

        if (version.VersionStatus == VersionStatus.Published)
        {
            return ModelStatsFailure(HttpStatusCode.Conflict, ErrorCode.VersionStatusInvalid, "Published 版本不能直接覆盖 model stats。", Array.Empty<ApiErrorItem>());
        }

        var saved = await _repository.SaveModelStatsAsync(assetId, versionId, request, cancellationToken);
        if (saved is null)
        {
            return ModelStatsFailure(HttpStatusCode.NotFound, ErrorCode.NotFound, "source model asset variant 不存在。", new[] { new ApiErrorItem("versionId", $"assetVersionId={versionId} 没有 source variant。") });
        }

        return new ModelStatsResult(HttpStatusCode.OK, ApiResponse<ModelStatsResponse>.Ok(ToModelStatsResponse(assetId, versionId, request, saved.SavedTime)));
    }

    public async Task<ModelStatsResult> GetModelStatsAsync(
        GetModelStatsRequest request,
        CancellationToken cancellationToken)
    {
        var version = await GetAssetVersionAsync(request.AssetId, request.VersionId, cancellationToken);
        if (version is null)
        {
            return await ModelStatsVersionNotFoundAsync(request.AssetId, request.VersionId, cancellationToken);
        }

        var statsJson = await _repository.GetModelStatsJsonAsync(request.AssetId, request.VersionId, cancellationToken);
        if (string.IsNullOrWhiteSpace(statsJson))
        {
            return ModelStatsFailure(HttpStatusCode.NotFound, ErrorCode.NotFound, "model stats 不存在。", Array.Empty<ApiErrorItem>());
        }

        try
        {
            var stats = JsonSerializer.Deserialize<SaveModelStatsRequest>(statsJson, JsonOptions);
            if (stats is null)
            {
                return ModelStatsFailure(HttpStatusCode.NotFound, ErrorCode.NotFound, "model stats 不存在。", Array.Empty<ApiErrorItem>());
            }

            var savedTime = await _repository.GetModelStatsUpdatedTimeAsync(request.AssetId, request.VersionId, cancellationToken);
            return new ModelStatsResult(HttpStatusCode.OK, ApiResponse<ModelStatsResponse>.Ok(ToModelStatsResponse(request.AssetId, request.VersionId, stats, savedTime ?? DateTime.MinValue)));
        }
        catch (JsonException)
        {
            return ModelStatsFailure(HttpStatusCode.NotFound, ErrorCode.NotFound, "model stats 格式无效。", Array.Empty<ApiErrorItem>());
        }
    }

    private async Task<AssetVersionAccessData?> GetAssetVersionAsync(
        long assetId,
        long? versionId,
        CancellationToken cancellationToken,
        bool usePublishedBaseline = false)
    {
        return await _repository.GetAssetVersionAsync(assetId, versionId, usePublishedBaseline, cancellationToken);
    }

    private async Task<ObjectTreeResult> ObjectTreeVersionNotFoundAsync(long assetId, long? versionId, CancellationToken cancellationToken)
    {
        if (!await _repository.AssetExistsByIdAsync(assetId, cancellationToken))
        {
            return ObjectTreeFailure(HttpStatusCode.NotFound, ErrorCode.AssetNotFound, "model asset 不存在。", new[] { new ApiErrorItem("assetId", $"未找到 assetId={assetId}。") });
        }

        return ObjectTreeFailure(HttpStatusCode.NotFound, ErrorCode.VersionNotFound, "asset version 不存在。", new[] { new ApiErrorItem("versionId", versionId.HasValue ? $"未找到 versionId={versionId.Value}。" : "未找到可用 asset version。") });
    }

    private async Task<ModelStatsResult> ModelStatsVersionNotFoundAsync(long assetId, long versionId, CancellationToken cancellationToken)
    {
        if (!await _repository.AssetExistsByIdAsync(assetId, cancellationToken))
        {
            return ModelStatsFailure(HttpStatusCode.NotFound, ErrorCode.AssetNotFound, "model asset 不存在。", new[] { new ApiErrorItem("assetId", $"未找到 assetId={assetId}。") });
        }

        return ModelStatsFailure(HttpStatusCode.NotFound, ErrorCode.VersionNotFound, "asset version 不存在。", new[] { new ApiErrorItem("versionId", $"未找到 versionId={versionId}。") });
    }

    private static List<ApiErrorItem> ValidateObjectTree(SaveObjectTreeRequest request)
    {
        var errors = new List<ApiErrorItem>();
        if (request.Nodes is null || request.Nodes.Count == 0)
        {
            errors.Add(new ApiErrorItem("nodes", "至少需要一个节点。"));
            return errors;
        }

        for (var index = 0; index < request.Nodes.Count; index++)
        {
            var node = request.Nodes[index];
            if (string.IsNullOrWhiteSpace(node.ObjectUuid)) errors.Add(new ApiErrorItem($"nodes[{index}].objectUuid", "objectUuid 不能为空。"));
            if (string.IsNullOrWhiteSpace(node.ObjectName)) errors.Add(new ApiErrorItem($"nodes[{index}].objectName", "objectName 不能为空。"));
            if (string.IsNullOrWhiteSpace(node.ObjectPath)) errors.Add(new ApiErrorItem($"nodes[{index}].objectPath", "objectPath 不能为空。"));
            if (string.IsNullOrWhiteSpace(node.ObjectType)) errors.Add(new ApiErrorItem($"nodes[{index}].objectType", "objectType 不能为空。"));
        }

        return errors;
    }

    private static List<ApiErrorItem> ValidateModelStats(SaveModelStatsRequest request)
    {
        var errors = new List<ApiErrorItem>();
        if (request.FileSizeMb < 0) errors.Add(new ApiErrorItem("fileSizeMb", "fileSizeMb 不能小于 0。"));
        if (request.MeshCount < 0) errors.Add(new ApiErrorItem("meshCount", "meshCount 不能小于 0。"));
        if (request.MaterialCount < 0) errors.Add(new ApiErrorItem("materialCount", "materialCount 不能小于 0。"));
        if (request.TextureCount < 0) errors.Add(new ApiErrorItem("textureCount", "textureCount 不能小于 0。"));
        if (request.VertexCount < 0) errors.Add(new ApiErrorItem("vertexCount", "vertexCount 不能小于 0。"));
        if (request.TriangleCount < 0) errors.Add(new ApiErrorItem("triangleCount", "triangleCount 不能小于 0。"));
        if (request.DrawCallEstimate < 0) errors.Add(new ApiErrorItem("drawCallEstimate", "drawCallEstimate 不能小于 0。"));
        if (request.MaxTextureSize < 0) errors.Add(new ApiErrorItem("maxTextureSize", "maxTextureSize 不能小于 0。"));
        return errors;
    }

    private static string? NormalizeMode(string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode)) return MonitorMode;
        if (string.Equals(mode.Trim(), MonitorMode, StringComparison.OrdinalIgnoreCase)) return MonitorMode;
        return string.Equals(mode.Trim(), EditMode, StringComparison.OrdinalIgnoreCase) ? EditMode : null;
    }

    private static bool IsVersionAllowed(string mode, VersionStatus versionStatus) => mode switch
    {
        MonitorMode => versionStatus == VersionStatus.Published,
        EditMode => versionStatus is VersionStatus.Draft or VersionStatus.Ready or VersionStatus.Published,
        _ => false
    };

    private static ObjectTreeNodeResponse ToObjectTreeNodeResponse(ModelObjectIndexData node)
    {
        var boundingBox = node.BoundingBoxMinX.HasValue && node.BoundingBoxMinY.HasValue && node.BoundingBoxMinZ.HasValue && node.BoundingBoxMaxX.HasValue && node.BoundingBoxMaxY.HasValue && node.BoundingBoxMaxZ.HasValue
            ? new ObjectTreeBoundingBoxResponse(new ObjectTreeVector3Response(node.BoundingBoxMinX.Value, node.BoundingBoxMinY.Value, node.BoundingBoxMinZ.Value), new ObjectTreeVector3Response(node.BoundingBoxMaxX.Value, node.BoundingBoxMaxY.Value, node.BoundingBoxMaxZ.Value))
            : null;

        return new ObjectTreeNodeResponse(node.ObjectUuid, node.ObjectName, node.ObjectPath, node.ParentUuid, node.ParentPath, node.ObjectType, boundingBox, node.MeshFingerprint);
    }

    private static ModelStatsResponse ToModelStatsResponse(long assetId, long versionId, SaveModelStatsRequest stats, DateTime savedTime) => new(assetId, versionId, stats.FileSizeMb, stats.MeshCount, stats.MaterialCount, stats.TextureCount, stats.VertexCount, stats.TriangleCount, stats.DrawCallEstimate, stats.MaxTextureSize, stats.HasMovableCandidates, stats.HasDuplicatedNames, stats.HasInvalidMaterials, stats.IsOverBudget, stats.BudgetMessages ?? Array.Empty<string>(), savedTime);

    private static ObjectTreeResult ObjectTreeFailure(HttpStatusCode statusCode, string code, string message, IReadOnlyList<ApiErrorItem> errors) => new(statusCode, ApiResponse<ObjectTreeResponse>.Failure(code, message, errors));
    private static ModelStatsResult ModelStatsFailure(HttpStatusCode statusCode, string code, string message, IReadOnlyList<ApiErrorItem> errors) => new(statusCode, ApiResponse<ModelStatsResponse>.Failure(code, message, errors));
}
