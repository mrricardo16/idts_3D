using System.Text.Json;
using HZ.IDTS.DigitalTwin.Application.ModelAssets;
using HZ.IDTS.DigitalTwin.Application.Storage;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;
using HZ.IDTS.DigitalTwin.Domain.Entities;
using HZ.IDTS.DigitalTwin.Domain.Enums;
using HZ.IDTS.DigitalTwin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HZ.IDTS.DigitalTwin.Infrastructure.ModelAssets;

public sealed class ModelAssetRepository : IModelAssetRepository
{
    private static readonly JsonSerializerOptions AuditJsonOptions = new(JsonSerializerDefaults.Web);
    private readonly DigitalTwinDbContext _dbContext;

    public ModelAssetRepository(DigitalTwinDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> AssetCodeExistsAsync(
        string assetCode,
        CancellationToken cancellationToken)
    {
        return _dbContext.ModelAssets
            .AsNoTracking()
            .AnyAsync(x => x.AssetCode == assetCode, cancellationToken);
    }

    public Task<bool> SourceFileHashExistsAsync(
        string sourceFileHash,
        CancellationToken cancellationToken)
    {
        return _dbContext.ModelAssets
            .AsNoTracking()
            .AnyAsync(x => x.SourceFileHash == sourceFileHash, cancellationToken);
    }

    public Task<bool> AssetExistsByIdAsync(
        long assetId,
        CancellationToken cancellationToken)
    {
        return _dbContext.ModelAssets
            .AsNoTracking()
            .AnyAsync(x => x.Id == assetId, cancellationToken);
    }

    public async Task<ModelManifestQueryData?> GetModelManifestAsync(
        long assetId,
        long? versionId,
        bool usePublishedBaseline,
        CancellationToken cancellationToken)
    {
        var asset = await _dbContext.ModelAssets
            .AsNoTracking()
            .Where(x => x.Id == assetId)
            .Select(x => new
            {
                x.Id,
                x.AssetCode,
                x.AssetName,
                x.CurrentVersionId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (asset is null)
        {
            return null;
        }

        var versionQuery = _dbContext.AssetVersions
            .AsNoTracking()
            .Where(x => x.ModelAssetId == assetId);

        var version = versionId.HasValue
            ? await SelectManifestVersionAsync(versionQuery.Where(x => x.Id == versionId.Value), cancellationToken)
            : usePublishedBaseline
                ? asset.CurrentVersionId.HasValue
                    ? await SelectManifestVersionAsync(versionQuery.Where(x => x.Id == asset.CurrentVersionId.Value), cancellationToken)
                    : null
                : await SelectManifestVersionAsync(versionQuery.OrderByDescending(x => x.VersionNo), cancellationToken);

        if (version is null)
        {
            return null;
        }

        var variants = await _dbContext.ModelAssetVariants
            .AsNoTracking()
            .Where(x => x.ModelAssetId == assetId && x.AssetVersionId == version.Id)
            .OrderBy(x => x.VariantLevel)
            .Select(x => new ModelManifestVariantData(
                x.VariantLevel,
                x.FileUrl))
            .ToListAsync(cancellationToken);

        var manifestJson = await _dbContext.AssetManifests
            .AsNoTracking()
            .Where(x => x.ModelAssetId == assetId && x.AssetVersionId == version.Id)
            .Select(x => x.ManifestJson)
            .SingleOrDefaultAsync(cancellationToken);

        var movableParts = await _dbContext.MovablePartBindings
            .AsNoTracking()
            .Where(x =>
                x.ModelAssetId == assetId &&
                x.AssetVersionId == version.Id &&
                x.BindingStatus == BindingStatus.active &&
                x.Enabled)
            .OrderBy(x => x.PartCode)
            .Select(x => new
            {
                x.Id,
                x.PartCode,
                x.BusinessName,
                x.MotionType,
                x.AxisMode,
                x.Axis
            })
            .ToListAsync(cancellationToken);

        var movablePartIds = movableParts.Select(x => x.Id).ToArray();
        var targets = movablePartIds.Length == 0
            ? new List<MotionTarget>()
            : await _dbContext.MotionTargets
                .AsNoTracking()
                .Where(x => movablePartIds.Contains(x.MovablePartId) && x.Enabled)
                .OrderBy(x => x.SortNo)
                .ThenBy(x => x.Id)
                .ToListAsync(cancellationToken);

        var targetLookup = targets
            .GroupBy(x => x.MovablePartId)
            .ToDictionary(
                x => x.Key,
                x => x.Select(target => new ModelManifestMotionTargetData(
                        target.Id,
                        target.TargetCode,
                        target.TargetName,
                        target.TargetValue))
                    .ToList());

        return new ModelManifestQueryData(
            asset.Id,
            version.Id,
            asset.AssetCode,
            asset.AssetName,
            version.VersionStatus,
            variants,
            manifestJson,
            movableParts.Select(x => new ModelManifestMovablePartData(
                    x.Id,
                    x.PartCode,
                    x.BusinessName,
                    x.MotionType,
                    x.AxisMode,
                    x.Axis,
                    targetLookup.TryGetValue(x.Id, out var partTargets)
                        ? partTargets
                        : Array.Empty<ModelManifestMotionTargetData>()))
                .ToList());
    }

    public async Task<AssetVersionAccessData?> GetAssetVersionAsync(
        long assetId,
        long? versionId,
        bool usePublishedBaseline,
        CancellationToken cancellationToken)
    {
        var asset = await _dbContext.ModelAssets
            .AsNoTracking()
            .Where(x => x.Id == assetId)
            .Select(x => new { x.Id, x.CurrentVersionId })
            .SingleOrDefaultAsync(cancellationToken);

        if (asset is null)
        {
            return null;
        }

        var versions = _dbContext.AssetVersions
            .AsNoTracking()
            .Where(x => x.ModelAssetId == assetId);

        var version = versionId.HasValue
            ? await SelectVersionAccessAsync(versions.Where(x => x.Id == versionId.Value), assetId, cancellationToken)
            : usePublishedBaseline
                ? asset.CurrentVersionId.HasValue
                    ? await SelectVersionAccessAsync(versions.Where(x => x.Id == asset.CurrentVersionId.Value), assetId, cancellationToken)
                    : null
                : await SelectVersionAccessAsync(versions.OrderByDescending(x => x.VersionNo), assetId, cancellationToken);

        return version;
    }

    private static Task<AssetVersionAccessData?> SelectVersionAccessAsync(
        IQueryable<AssetVersion> query,
        long assetId,
        CancellationToken cancellationToken)
    {
        return query
            .Select(x => new AssetVersionAccessData(assetId, x.Id, x.VersionStatus))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static Task<ManifestVersionData?> SelectManifestVersionAsync(
        IQueryable<AssetVersion> query,
        CancellationToken cancellationToken)
    {
        return query
            .Select(x => new ManifestVersionData(x.Id, x.VersionNo, x.VersionStatus))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ModelObjectIndexData>> GetObjectTreeAsync(
        long assetId,
        long versionId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ModelObjectIndexes
            .AsNoTracking()
            .Where(x => x.ModelAssetId == assetId && x.AssetVersionId == versionId)
            .OrderBy(x => x.ObjectPath)
            .ThenBy(x => x.ObjectUuid)
            .Select(x => new ModelObjectIndexData(
                x.ObjectUuid,
                x.ObjectName,
                x.ObjectPath,
                x.ParentUuid,
                x.ParentPath,
                x.ObjectType,
                x.BoundingBoxMinX,
                x.BoundingBoxMinY,
                x.BoundingBoxMinZ,
                x.BoundingBoxMaxX,
                x.BoundingBoxMaxY,
                x.BoundingBoxMaxZ,
                x.MeshFingerprint))
            .ToListAsync(cancellationToken);
    }

    public async Task<DateTime> ReplaceObjectTreeAsync(
        long assetId,
        long versionId,
        IReadOnlyList<ObjectTreeNodeRequest> nodes,
        CancellationToken cancellationToken)
    {
        var savedTime = DateTime.UtcNow;
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        await _dbContext.ModelObjectIndexes
            .Where(x => x.ModelAssetId == assetId && x.AssetVersionId == versionId)
            .ExecuteDeleteAsync(cancellationToken);

        var indexes = nodes.Select(node => new ModelObjectIndex
        {
            ModelAssetId = assetId,
            AssetVersionId = versionId,
            ObjectUuid = node.ObjectUuid,
            ObjectName = node.ObjectName,
            ObjectPath = node.ObjectPath,
            ParentUuid = node.ParentUuid,
            ParentPath = node.ParentPath,
            ObjectType = node.ObjectType,
            BoundingBoxMinX = node.BoundingBox?.Min.X,
            BoundingBoxMinY = node.BoundingBox?.Min.Y,
            BoundingBoxMinZ = node.BoundingBox?.Min.Z,
            BoundingBoxMaxX = node.BoundingBox?.Max.X,
            BoundingBoxMaxY = node.BoundingBox?.Max.Y,
            BoundingBoxMaxZ = node.BoundingBox?.Max.Z,
            MeshFingerprint = node.MeshFingerprint
        }).ToList();

        _dbContext.ModelObjectIndexes.AddRange(indexes);
        _dbContext.OperationAudits.Add(new OperationAudit
        {
            OperationType = OperationType.edit,
            TargetType = OperationTargetType.asset_version,
            TargetId = versionId,
            AfterJson = JsonSerializer.Serialize(new { assetId, versionId, nodeCount = indexes.Count }, AuditJsonOptions),
            CreatedTime = savedTime
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return savedTime;
    }

    public Task<string?> GetModelStatsJsonAsync(
        long assetId,
        long versionId,
        CancellationToken cancellationToken)
    {
        return _dbContext.AssetManifests
            .AsNoTracking()
            .Where(x => x.ModelAssetId == assetId && x.AssetVersionId == versionId)
            .Select(x => x.ModelStatsJson)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public Task<DateTime?> GetModelStatsUpdatedTimeAsync(
        long assetId,
        long versionId,
        CancellationToken cancellationToken)
    {
        return _dbContext.AssetManifests
            .AsNoTracking()
            .Where(x => x.ModelAssetId == assetId && x.AssetVersionId == versionId)
            .Select(x => (DateTime?)x.UpdatedTime)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<SaveModelStatsData?> SaveModelStatsAsync(
        long assetId,
        long versionId,
        SaveModelStatsRequest request,
        CancellationToken cancellationToken)
    {
        var savedTime = DateTime.UtcNow;
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var sourceVariant = await _dbContext.ModelAssetVariants
            .Where(x => x.ModelAssetId == assetId && x.AssetVersionId == versionId && x.VariantLevel == VariantLevel.source)
            .SingleOrDefaultAsync(cancellationToken);

        if (sourceVariant is null)
        {
            return null;
        }

        var manifest = await _dbContext.AssetManifests
            .SingleOrDefaultAsync(x => x.ModelAssetId == assetId && x.AssetVersionId == versionId, cancellationToken);

        var beforeStatsJson = manifest?.ModelStatsJson;
        var statsJson = JsonSerializer.Serialize(request, AuditJsonOptions);
        if (manifest is null)
        {
            manifest = new AssetManifest
            {
                ModelAssetId = assetId,
                AssetVersionId = versionId,
                ManifestJson = "{}",
                ModelStatsJson = statsJson,
                CreatedTime = savedTime,
                UpdatedTime = savedTime
            };
            _dbContext.AssetManifests.Add(manifest);
        }
        else
        {
            manifest.ModelStatsJson = statsJson;
            manifest.UpdatedTime = savedTime;
        }

        sourceVariant.TriangleCount = request.TriangleCount;
        sourceVariant.VertexCount = request.VertexCount;
        sourceVariant.MeshCount = request.MeshCount;
        sourceVariant.MaterialCount = request.MaterialCount;
        sourceVariant.TextureCount = request.TextureCount;

        _dbContext.OperationAudits.Add(new OperationAudit
        {
            OperationType = OperationType.edit,
            TargetType = OperationTargetType.asset_version,
            TargetId = versionId,
            BeforeJson = beforeStatsJson,
            AfterJson = statsJson,
            CreatedTime = savedTime
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new SaveModelStatsData(savedTime);
    }

    public async Task<AssetVersionLifecycleRepositoryResult> ExecuteVersionLifecycleAsync(
        AssetVersionLifecycleCommand command,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        // 通过 PostgreSQL 行锁串行化同一资产的生命周期操作，避免并发发布产生两个 Published 版本。
        var modelAsset = await _dbContext.ModelAssets
            .FromSqlInterpolated($"SELECT * FROM model_asset WHERE id = {command.AssetId} FOR UPDATE")
            .SingleOrDefaultAsync(cancellationToken);
        if (modelAsset is null)
        {
            return new AssetVersionLifecycleRepositoryResult(false, false, null, null, null, null, Array.Empty<ApiErrorItem>());
        }

        var assetVersion = await _dbContext.AssetVersions
            .SingleOrDefaultAsync(
                x => x.Id == command.VersionId && x.ModelAssetId == command.AssetId,
                cancellationToken);
        if (assetVersion is null)
        {
            return new AssetVersionLifecycleRepositoryResult(true, false, null, null, null, null, Array.Empty<ApiErrorItem>());
        }

        if (!IsOperationAllowed(command.Operation, assetVersion.VersionStatus))
        {
            return LifecycleFailure(
                assetVersion.VersionStatus,
                ErrorCode.VersionStatusInvalid,
                GetInvalidStatusMessage(command.Operation, assetVersion.VersionStatus),
                new[] { new ApiErrorItem("versionStatus", $"当前状态为 {assetVersion.VersionStatus}。") });
        }

        var now = DateTime.UtcNow;
        if (command.Operation is not AssetVersionLifecycleOperation.Archive)
        {
            var gateFailure = await ValidatePublicationGateAsync(
                command.AssetId,
                command.VersionId,
                command.Operation is AssetVersionLifecycleOperation.Publish or AssetVersionLifecycleOperation.Rollback,
                cancellationToken);
            if (gateFailure is not null)
            {
                return gateFailure;
            }
        }

        var previousStatus = assetVersion.VersionStatus;
        var previousCurrentVersionId = modelAsset.CurrentVersionId;
        var archivedPublishedVersionIds = new List<long>();
        var affectedBindings = new List<DeviceModelBinding>();

        switch (command.Operation)
        {
            case AssetVersionLifecycleOperation.MarkReady:
                assetVersion.VersionStatus = VersionStatus.Ready;
                break;

            case AssetVersionLifecycleOperation.Publish:
            case AssetVersionLifecycleOperation.Rollback:
                var publishedVersions = await _dbContext.AssetVersions
                    .Where(x =>
                        x.ModelAssetId == command.AssetId &&
                        x.VersionStatus == VersionStatus.Published &&
                        x.Id != command.VersionId)
                    .ToListAsync(cancellationToken);

                foreach (var publishedVersion in publishedVersions)
                {
                    publishedVersion.VersionStatus = VersionStatus.Archived;
                    publishedVersion.ArchivedTime = now;
                    archivedPublishedVersionIds.Add(publishedVersion.Id);
                }

                var bindingResult = await SynchronizeBindingsForPublishedVersionAsync(
                    command.AssetId,
                    command.VersionId,
                    now,
                    command.Operation == AssetVersionLifecycleOperation.Rollback,
                    cancellationToken);
                if (bindingResult.ConflictMessage is not null)
                {
                    return LifecycleFailure(
                        assetVersion.VersionStatus,
                        ErrorCode.Conflict,
                        bindingResult.ConflictMessage,
                        new[] { new ApiErrorItem("deviceModelBinding", bindingResult.ConflictMessage) });
                }

                affectedBindings.AddRange(bindingResult.AffectedBindings);
                assetVersion.VersionStatus = VersionStatus.Published;
                assetVersion.PublishedTime = now;
                assetVersion.ArchivedTime = null;
                modelAsset.CurrentVersionId = command.VersionId;
                modelAsset.UpdatedTime = now;
                break;

            case AssetVersionLifecycleOperation.Archive:
                assetVersion.VersionStatus = VersionStatus.Archived;
                assetVersion.ArchivedTime = now;
                affectedBindings.AddRange(await ArchiveBindingsForVersionAsync(command.VersionId, now, cancellationToken));
                if (modelAsset.CurrentVersionId == command.VersionId)
                {
                    modelAsset.CurrentVersionId = null;
                    modelAsset.UpdatedTime = now;
                }

                break;

            default:
                throw new InvalidOperationException($"Unsupported lifecycle operation: {command.Operation}.");
        }

        _dbContext.OperationAudits.Add(new OperationAudit
        {
            OperationType = GetAuditOperationType(command.Operation),
            TargetType = OperationTargetType.asset_version,
            TargetId = command.VersionId,
            BeforeJson = JsonSerializer.Serialize(
                new
                {
                    action = ToAuditAction(command.Operation),
                    assetId = command.AssetId,
                    versionId = command.VersionId,
                    oldStatus = previousStatus.ToString(),
                    currentVersionId = previousCurrentVersionId
                },
                AuditJsonOptions),
            AfterJson = JsonSerializer.Serialize(
                new
                {
                    action = ToAuditAction(command.Operation),
                    assetId = command.AssetId,
                    versionId = command.VersionId,
                    oldStatus = previousStatus.ToString(),
                    newStatus = assetVersion.VersionStatus.ToString(),
                    currentVersionId = modelAsset.CurrentVersionId,
                    archivedPublishedVersionIds,
                    affectedBindings = affectedBindings.Select(x => new { bindingId = x.Id, x.DeviceInstanceId, x.AssetVersionId, bindingStatus = x.BindingStatus.ToString() }),
                    command.Remark,
                    operationTime = now
                },
                AuditJsonOptions),
            CreatedTime = now
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new AssetVersionLifecycleRepositoryResult(
            true,
            true,
            assetVersion.VersionStatus,
            now,
            null,
            null,
            Array.Empty<ApiErrorItem>());
    }

    private async Task<AssetVersionLifecycleRepositoryResult?> ValidatePublicationGateAsync(
        long assetId,
        long versionId,
        bool enforceBudget,
        CancellationToken cancellationToken)
    {
        var manifestJson = await _dbContext.AssetManifests
            .AsNoTracking()
            .Where(x => x.ModelAssetId == assetId && x.AssetVersionId == versionId)
            .Select(x => x.ManifestJson)
            .SingleOrDefaultAsync(cancellationToken);
        if (!IsJsonObject(manifestJson))
        {
            return LifecycleFailure(
                null,
                ErrorCode.ManifestRequired,
                "发布前必须存在可解析的 manifest_json 对象。",
                Array.Empty<ApiErrorItem>());
        }

        var hasObjectTree = await _dbContext.ModelObjectIndexes
            .AsNoTracking()
            .AnyAsync(x => x.ModelAssetId == assetId && x.AssetVersionId == versionId, cancellationToken);
        if (!hasObjectTree)
        {
            return LifecycleFailure(
                null,
                ErrorCode.ObjectTreeRequired,
                "发布前必须保存 object tree。",
                Array.Empty<ApiErrorItem>());
        }

        var modelStatsJson = await _dbContext.AssetManifests
            .AsNoTracking()
            .Where(x => x.ModelAssetId == assetId && x.AssetVersionId == versionId)
            .Select(x => x.ModelStatsJson)
            .SingleOrDefaultAsync(cancellationToken);
        var modelStats = DeserializeModelStats(modelStatsJson);
        if (modelStats is null)
        {
            return LifecycleFailure(
                null,
                ErrorCode.ModelStatsRequired,
                "发布前必须保存可解析的 model stats。",
                Array.Empty<ApiErrorItem>());
        }

        if (enforceBudget && modelStats.IsOverBudget)
        {
            var budgetMessage = modelStats.BudgetMessages is { Count: > 0 }
                ? string.Join("；", modelStats.BudgetMessages)
                : "模型超出发布预算。";
            return LifecycleFailure(
                null,
                ErrorCode.ValidationFailed,
                $"模型超出发布预算：{budgetMessage}",
                new[] { new ApiErrorItem("budgetMessages", budgetMessage) });
        }

        var movableParts = await _dbContext.MovablePartBindings
            .AsNoTracking()
            .Where(x => x.ModelAssetId == assetId && x.AssetVersionId == versionId && x.Enabled)
            .ToListAsync(cancellationToken);
        if (movableParts.Count == 0)
        {
            return null;
        }

        var objectIndexes = await _dbContext.ModelObjectIndexes
            .AsNoTracking()
            .Where(x => x.ModelAssetId == assetId && x.AssetVersionId == versionId)
            .Select(x => new { x.ObjectUuid, x.ObjectPath })
            .ToListAsync(cancellationToken);
        var objectUuids = objectIndexes.Select(x => x.ObjectUuid).ToHashSet(StringComparer.Ordinal);
        var objectPaths = objectIndexes.Select(x => x.ObjectPath).ToHashSet(StringComparer.Ordinal);
        var movablePartIds = movableParts.Select(x => x.Id).ToArray();
        var partsWithEnabledTarget = await _dbContext.MotionTargets
            .AsNoTracking()
            .Where(x => movablePartIds.Contains(x.MovablePartId) && x.Enabled)
            .Select(x => x.MovablePartId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var targetPartIds = partsWithEnabledTarget.ToHashSet();

        foreach (var movablePart in movableParts)
        {
            if (movablePart.BindingStatus != BindingStatus.active)
            {
                return LifecycleFailure(
                    null,
                    ErrorCode.ValidationFailed,
                    $"enabled movable part {movablePart.PartCode} 的 binding_status 必须为 active。",
                    new[] { new ApiErrorItem("movablePart", movablePart.PartCode) });
            }

            var objectMatches =
                (!string.IsNullOrWhiteSpace(movablePart.ObjectUuid) && objectUuids.Contains(movablePart.ObjectUuid)) ||
                (!string.IsNullOrWhiteSpace(movablePart.ObjectPath) && objectPaths.Contains(movablePart.ObjectPath));
            if (!objectMatches)
            {
                return LifecycleFailure(
                    null,
                    ErrorCode.ValidationFailed,
                    $"enabled movable part {movablePart.PartCode} 未引用当前版本 object tree。",
                    new[] { new ApiErrorItem("movablePart", movablePart.PartCode) });
            }

            if (movablePart.MinValue > movablePart.HomeValue || movablePart.HomeValue > movablePart.MaxValue)
            {
                return LifecycleFailure(
                    null,
                    ErrorCode.ValidationFailed,
                    $"enabled movable part {movablePart.PartCode} 的 minValue、homeValue、maxValue 范围无效。",
                    new[] { new ApiErrorItem("movablePart", movablePart.PartCode) });
            }

            // 当前 Domain MotionType 没有 none 值，因此所有已启用可动部件都要求至少一个已启用目标点。
            if (!targetPartIds.Contains(movablePart.Id))
            {
                return LifecycleFailure(
                    null,
                    ErrorCode.ValidationFailed,
                    $"enabled movable part {movablePart.PartCode} 至少需要一个 enabled motion target。",
                    new[] { new ApiErrorItem("movablePart", movablePart.PartCode) });
            }
        }

        return null;
    }

    private async Task<BindingSynchronizationResult> SynchronizeBindingsForPublishedVersionAsync(
        long assetId,
        long targetVersionId,
        DateTime now,
        bool restoreArchivedTargetBindings,
        CancellationToken cancellationToken)
    {
        var activeBindings = await _dbContext.DeviceModelBindings
            .Where(x => x.ModelAssetId == assetId && x.BindingStatus == BindingStatus.active)
            .ToListAsync(cancellationToken);
        var archivedTargetBindings = restoreArchivedTargetBindings
            ? await _dbContext.DeviceModelBindings
                .Where(x => x.AssetVersionId == targetVersionId && x.BindingStatus == BindingStatus.archived)
                .ToListAsync(cancellationToken)
            : new List<DeviceModelBinding>();
        if (activeBindings.Count == 0 && archivedTargetBindings.Count == 0)
        {
            return BindingSynchronizationResult.Success(Array.Empty<DeviceModelBinding>());
        }

        var deviceInstanceIds = activeBindings
            .Select(x => x.DeviceInstanceId)
            .Concat(archivedTargetBindings.Select(x => x.DeviceInstanceId))
            .Distinct()
            .ToArray();
        var conflictingBinding = await _dbContext.DeviceModelBindings
            .AsNoTracking()
            .Where(x =>
                deviceInstanceIds.Contains(x.DeviceInstanceId) &&
                x.BindingStatus == BindingStatus.active &&
                x.ModelAssetId != assetId)
            .Select(x => new { x.Id, x.DeviceInstanceId })
            .FirstOrDefaultAsync(cancellationToken);
        if (conflictingBinding is not null)
        {
            return BindingSynchronizationResult.Conflict(
                $"deviceInstanceId={conflictingBinding.DeviceInstanceId} 已有其他模型资产的 active binding (bindingId={conflictingBinding.Id})。" );
        }

        var affectedBindings = new List<DeviceModelBinding>();
        foreach (var activeBinding in activeBindings.Where(x => x.AssetVersionId != targetVersionId))
        {
            activeBinding.BindingStatus = BindingStatus.archived;
            activeBinding.ActiveTo = now;
            affectedBindings.Add(activeBinding);
        }

        foreach (var deviceInstanceId in deviceInstanceIds)
        {
            var targetBinding = await _dbContext.DeviceModelBindings
                .SingleOrDefaultAsync(
                    x => x.DeviceInstanceId == deviceInstanceId && x.AssetVersionId == targetVersionId,
                    cancellationToken);
            if (targetBinding is null)
            {
                targetBinding = new DeviceModelBinding
                {
                    DeviceInstanceId = deviceInstanceId,
                    ModelAssetId = assetId,
                    AssetVersionId = targetVersionId,
                    BindingStatus = BindingStatus.active,
                    ActiveFrom = now,
                    ActiveTo = null
                };
                _dbContext.DeviceModelBindings.Add(targetBinding);
                affectedBindings.Add(targetBinding);
                continue;
            }

            if (targetBinding.BindingStatus != BindingStatus.active)
            {
                targetBinding.BindingStatus = BindingStatus.active;
                targetBinding.ActiveFrom = now;
                targetBinding.ActiveTo = null;
                affectedBindings.Add(targetBinding);
            }
        }

        return BindingSynchronizationResult.Success(affectedBindings);
    }

    private async Task<IReadOnlyList<DeviceModelBinding>> ArchiveBindingsForVersionAsync(
        long versionId,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var bindings = await _dbContext.DeviceModelBindings
            .Where(x => x.AssetVersionId == versionId && x.BindingStatus == BindingStatus.active)
            .ToListAsync(cancellationToken);
        foreach (var binding in bindings)
        {
            binding.BindingStatus = BindingStatus.archived;
            binding.ActiveTo = now;
        }

        return bindings;
    }

    private static bool IsOperationAllowed(AssetVersionLifecycleOperation operation, VersionStatus status) => operation switch
    {
        AssetVersionLifecycleOperation.MarkReady => status == VersionStatus.Draft,
        AssetVersionLifecycleOperation.Publish => status == VersionStatus.Ready,
        AssetVersionLifecycleOperation.Archive => status == VersionStatus.Published,
        AssetVersionLifecycleOperation.Rollback => status == VersionStatus.Archived,
        _ => false
    };

    private static string GetInvalidStatusMessage(AssetVersionLifecycleOperation operation, VersionStatus status) => operation switch
    {
        AssetVersionLifecycleOperation.MarkReady => "只有 Draft 版本可以标记为 Ready。",
        AssetVersionLifecycleOperation.Publish => "只有 Ready 版本可以发布。",
        AssetVersionLifecycleOperation.Archive => "只有 Published 版本可以归档。",
        AssetVersionLifecycleOperation.Rollback => "只有 Archived 版本可以回滚。",
        _ => $"当前状态 {status} 不允许该操作。"
    };

    private static OperationType GetAuditOperationType(AssetVersionLifecycleOperation operation) => operation switch
    {
        AssetVersionLifecycleOperation.MarkReady or AssetVersionLifecycleOperation.Archive => OperationType.edit,
        AssetVersionLifecycleOperation.Publish => OperationType.publish,
        AssetVersionLifecycleOperation.Rollback => OperationType.rollback,
        _ => throw new InvalidOperationException($"Unsupported lifecycle operation: {operation}.")
    };

    private static string ToAuditAction(AssetVersionLifecycleOperation operation) => operation switch
    {
        AssetVersionLifecycleOperation.MarkReady => "mark-ready",
        AssetVersionLifecycleOperation.Publish => "publish",
        AssetVersionLifecycleOperation.Archive => "archive",
        AssetVersionLifecycleOperation.Rollback => "rollback",
        _ => throw new InvalidOperationException($"Unsupported lifecycle operation: {operation}.")
    };

    private static bool IsJsonObject(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static SaveModelStatsRequest? DeserializeModelStats(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<SaveModelStatsRequest>(json, AuditJsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static AssetVersionLifecycleRepositoryResult LifecycleFailure(
        VersionStatus? versionStatus,
        string code,
        string message,
        IReadOnlyList<ApiErrorItem> errors) =>
        new(true, true, versionStatus, null, code, message, errors);

    private sealed record BindingSynchronizationResult(
        IReadOnlyList<DeviceModelBinding> AffectedBindings,
        string? ConflictMessage)
    {
        public static BindingSynchronizationResult Success(IReadOnlyList<DeviceModelBinding> affectedBindings) =>
            new(affectedBindings, null);

        public static BindingSynchronizationResult Conflict(string message) =>
            new(Array.Empty<DeviceModelBinding>(), message);
    }

    private sealed record ManifestVersionData(
        long Id,
        int VersionNo,
        VersionStatus VersionStatus);

    public async Task<UploadModelAssetResponse> CreateUploadAsync(
        CreateModelAssetUploadCommand command,
        Func<long, long, CancellationToken, Task<StoredModelAssetFile>> persistSourceFileAsync,
        Func<long, long, CancellationToken, Task> cleanupSourceFileAsync,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        long? assetId = null;
        long? versionId = null;
        var sourceFilePersisted = false;

        try
        {
            var now = DateTime.UtcNow;
            var modelAsset = new ModelAsset
            {
                AssetCode = command.AssetCode,
                AssetName = command.AssetName,
                SourceFileName = command.SourceFileName,
                SourceFileHash = command.SourceFileHash,
                SourceFileType = command.SourceFileType,
                AssetType = command.AssetType,
                ProcessingStatus = ProcessingStatus.pending,
                CreatedTime = now,
                UpdatedTime = now
            };

            _dbContext.ModelAssets.Add(modelAsset);
            await _dbContext.SaveChangesAsync(cancellationToken);
            assetId = modelAsset.Id;

            var assetVersion = new AssetVersion
            {
                ModelAssetId = modelAsset.Id,
                VersionNo = 1,
                VersionStatus = VersionStatus.Draft,
                CreatedTime = now
            };

            _dbContext.AssetVersions.Add(assetVersion);
            await _dbContext.SaveChangesAsync(cancellationToken);
            versionId = assetVersion.Id;

            var sourceFile = await persistSourceFileAsync(modelAsset.Id, assetVersion.Id, cancellationToken);
            sourceFilePersisted = true;

            var sourceVariant = new ModelAssetVariant
            {
                ModelAssetId = modelAsset.Id,
                AssetVersionId = assetVersion.Id,
                VariantLevel = VariantLevel.source,
                FileUrl = sourceFile.FileUrl,
                FileHash = sourceFile.SourceFileHash,
                FileSize = sourceFile.FileSize,
                CreatedTime = now
            };

            _dbContext.ModelAssetVariants.Add(sourceVariant);

            var uploadJob = new ModelConversionJob
            {
                ModelAssetId = modelAsset.Id,
                AssetVersionId = assetVersion.Id,
                JobType = ConversionJobType.upload,
                Status = ConversionJobStatus.pending,
                Progress = 0,
                Message = "GLB upload saved. Waiting for later inspection tasks.",
                InputFile = sourceFile.FileUrl,
                OutputDirectory = sourceFile.FileUrl,
                RetryCount = 0
            };

            _dbContext.ModelConversionJobs.Add(uploadJob);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _dbContext.OperationAudits.Add(new OperationAudit
            {
                OperationType = OperationType.upload,
                TargetType = OperationTargetType.model_asset,
                TargetId = modelAsset.Id,
                BeforeJson = null,
                AfterJson = JsonSerializer.Serialize(
                    new
                    {
                        assetId = modelAsset.Id,
                        versionId = assetVersion.Id,
                        jobId = uploadJob.Id,
                        sourceFileHash = sourceFile.SourceFileHash,
                        sourceFileUrl = sourceFile.FileUrl
                    },
                    AuditJsonOptions),
                CreatedTime = now
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new UploadModelAssetResponse(
                modelAsset.Id,
                assetVersion.Id,
                uploadJob.Id,
                modelAsset.AssetCode,
                assetVersion.VersionStatus.ToString(),
                modelAsset.ProcessingStatus.ToString(),
                sourceFile.SourceFileHash,
                sourceFile.FileUrl);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);

            if (sourceFilePersisted && assetId.HasValue && versionId.HasValue)
            {
                await cleanupSourceFileAsync(assetId.Value, versionId.Value, cancellationToken);
            }

            throw;
        }
    }
}
