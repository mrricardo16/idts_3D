using System.Text.Json;
using HZ.IDTS.DigitalTwin.Application.ModelAssets;
using HZ.IDTS.DigitalTwin.Application.Storage;
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
            ? await versionQuery
                .Where(x => x.Id == versionId.Value)
                .Select(x => new
                {
                    x.Id,
                    x.VersionNo,
                    x.VersionStatus
                })
                .SingleOrDefaultAsync(cancellationToken)
            : await versionQuery
                .Where(x => !asset.CurrentVersionId.HasValue || x.Id == asset.CurrentVersionId.Value)
                .OrderByDescending(x => x.VersionNo)
                .Select(x => new
                {
                    x.Id,
                    x.VersionNo,
                    x.VersionStatus
                })
                .FirstOrDefaultAsync(cancellationToken);

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

            modelAsset.CurrentVersionId = assetVersion.Id;
            modelAsset.UpdatedTime = now;

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
