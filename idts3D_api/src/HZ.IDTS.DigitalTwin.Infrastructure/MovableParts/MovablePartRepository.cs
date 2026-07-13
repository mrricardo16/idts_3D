using System.Text.Json;
using HZ.IDTS.DigitalTwin.Application.MovableParts;
using HZ.IDTS.DigitalTwin.Domain.Entities;
using HZ.IDTS.DigitalTwin.Domain.Enums;
using HZ.IDTS.DigitalTwin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HZ.IDTS.DigitalTwin.Infrastructure.MovableParts;

public sealed class MovablePartRepository : IMovablePartRepository
{
    private const string PartCodeUniqueConstraint = "IX_movable_part_binding_asset_version_id_part_code";
    private static readonly JsonSerializerOptions AuditJsonOptions = new(JsonSerializerDefaults.Web);
    private readonly DigitalTwinDbContext _dbContext;

    public MovablePartRepository(DigitalTwinDbContext dbContext) => _dbContext = dbContext;

    public async Task<MovablePartVersionLookup> GetVersionAsync(long assetId, long versionId, CancellationToken cancellationToken)
    {
        var assetExists = await _dbContext.ModelAssets.AsNoTracking().AnyAsync(x => x.Id == assetId, cancellationToken);
        if (!assetExists) return new MovablePartVersionLookup(false, false, null);
        var status = await _dbContext.AssetVersions.AsNoTracking().Where(x => x.Id == versionId && x.ModelAssetId == assetId).Select(x => (VersionStatus?)x.VersionStatus).SingleOrDefaultAsync(cancellationToken);
        return new MovablePartVersionLookup(true, status.HasValue, status);
    }

    public async Task<MovablePartObjectResolution> ResolveCanonicalObjectAsync(long assetId, long versionId, string? objectUuid, string? objectPath, CancellationToken cancellationToken)
    {
        var candidates = await _dbContext.ModelObjectIndexes.AsNoTracking()
            .Where(x => x.ModelAssetId == assetId && x.AssetVersionId == versionId && ((objectUuid != null && x.ObjectUuid == objectUuid) || (objectPath != null && x.ObjectPath == objectPath)))
            .OrderBy(x => x.Id).ToListAsync(cancellationToken);
        var uuidMatch = objectUuid is null ? null : candidates.FirstOrDefault(x => x.ObjectUuid == objectUuid);
        var pathMatch = objectPath is null ? null : candidates.FirstOrDefault(x => x.ObjectPath == objectPath);
        if (objectUuid is not null && objectPath is not null && (uuidMatch is null || pathMatch is null || uuidMatch.Id != pathMatch.Id)) return new(MovablePartObjectResolutionKind.Conflict, null);
        var match = uuidMatch ?? pathMatch;
        return match is null ? new(MovablePartObjectResolutionKind.NotFound, null) : new(MovablePartObjectResolutionKind.Found, ToCanonicalObject(match));
    }

    public async Task<IReadOnlyList<MovablePartData>> GetListAsync(long assetId, long versionId, bool? enabled, CancellationToken cancellationToken) =>
        await _dbContext.MovablePartBindings.AsNoTracking().Where(x => x.ModelAssetId == assetId && x.AssetVersionId == versionId && (!enabled.HasValue || x.Enabled == enabled.Value)).OrderBy(x => x.PartCode).ThenBy(x => x.Id).Select(x => ToData(x)).ToListAsync(cancellationToken);

    public async Task<MovablePartWriteResult> CreateAsync(CreateMovablePartCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var versionResult = await LockWritableVersionAsync(command.AssetId, command.VersionId, cancellationToken);
            if (versionResult.Failure != MovablePartWriteFailure.None) return versionResult;
            var resolution = await ResolveCanonicalObjectAsync(command.AssetId, command.VersionId, command.ObjectUuid, command.ObjectPath, cancellationToken);
            if (resolution.Kind != MovablePartObjectResolutionKind.Found) return new(resolution.Kind == MovablePartObjectResolutionKind.Conflict ? MovablePartWriteFailure.ObjectConflict : MovablePartWriteFailure.ObjectNotFound, null);
            if (await PartCodeExistsAsync(command.VersionId, command.PartCode, null, cancellationToken)) return new(MovablePartWriteFailure.DuplicatePartCode, null);
            var part = ToEntity(command, resolution.Object!);
            _dbContext.MovablePartBindings.Add(part);
            await _dbContext.SaveChangesAsync(cancellationToken);
            AddAudit(OperationType.edit, part.Id, null, ToAuditSnapshot(part));
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new(MovablePartWriteFailure.None, ToData(part));
        }
        catch (DbUpdateException exception) when (IsPartCodeUniqueViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return new(MovablePartWriteFailure.DuplicatePartCode, null);
        }
    }

    public async Task<MovablePartWriteResult> UpdateAsync(UpdateMovablePartCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var versionResult = await LockWritableVersionAsync(command.AssetId, command.VersionId, cancellationToken);
            if (versionResult.Failure != MovablePartWriteFailure.None) return versionResult;
            var part = await _dbContext.MovablePartBindings.FromSqlInterpolated($"SELECT * FROM movable_part_binding WHERE id = {command.PartId} AND model_asset_id = {command.AssetId} AND asset_version_id = {command.VersionId} FOR UPDATE").SingleOrDefaultAsync(cancellationToken);
            if (part is null) return new(MovablePartWriteFailure.PartNotFound, null);
            var resolution = await ResolveCanonicalObjectAsync(command.AssetId, command.VersionId, command.ObjectUuid, command.ObjectPath, cancellationToken);
            if (resolution.Kind != MovablePartObjectResolutionKind.Found) return new(resolution.Kind == MovablePartObjectResolutionKind.Conflict ? MovablePartWriteFailure.ObjectConflict : MovablePartWriteFailure.ObjectNotFound, null);
            if (await PartCodeExistsAsync(command.VersionId, command.PartCode, command.PartId, cancellationToken)) return new(MovablePartWriteFailure.DuplicatePartCode, null);
            var before = ToAuditSnapshot(part);
            Apply(part, command, resolution.Object!);
            AddAudit(OperationType.edit, part.Id, before, ToAuditSnapshot(part));
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new(MovablePartWriteFailure.None, ToData(part));
        }
        catch (DbUpdateException exception) when (IsPartCodeUniqueViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return new(MovablePartWriteFailure.DuplicatePartCode, null);
        }
    }

    public async Task<MovablePartWriteResult> DeleteAsync(DeleteMovablePartCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var versionResult = await LockWritableVersionAsync(command.AssetId, command.VersionId, cancellationToken);
        if (versionResult.Failure != MovablePartWriteFailure.None) return versionResult;
        var part = await _dbContext.MovablePartBindings.FromSqlInterpolated($"SELECT * FROM movable_part_binding WHERE id = {command.PartId} AND model_asset_id = {command.AssetId} AND asset_version_id = {command.VersionId} FOR UPDATE").SingleOrDefaultAsync(cancellationToken);
        if (part is null) return new(MovablePartWriteFailure.PartNotFound, null);
        var before = ToAuditSnapshot(part);
        await _dbContext.MotionTargets.Where(x => x.MovablePartId == command.PartId).ExecuteDeleteAsync(cancellationToken);
        _dbContext.MovablePartBindings.Remove(part);
        AddAudit(OperationType.delete, part.Id, before, null);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(MovablePartWriteFailure.None, ToData(part));
    }

    private async Task<MovablePartWriteResult> LockWritableVersionAsync(long assetId, long versionId, CancellationToken cancellationToken)
    {
        var version = await _dbContext.AssetVersions.FromSqlInterpolated($"SELECT * FROM asset_version WHERE id = {versionId} AND model_asset_id = {assetId} FOR UPDATE").SingleOrDefaultAsync(cancellationToken);
        if (version is null)
        {
            var assetExists = await _dbContext.ModelAssets.AsNoTracking().AnyAsync(x => x.Id == assetId, cancellationToken);
            return new(assetExists ? MovablePartWriteFailure.VersionNotFound : MovablePartWriteFailure.AssetNotFound, null);
        }
        return version.VersionStatus is VersionStatus.Draft or VersionStatus.Ready ? new(MovablePartWriteFailure.None, null) : new(MovablePartWriteFailure.VersionNotWritable, null);
    }

    private Task<bool> PartCodeExistsAsync(long versionId, string partCode, long? excludingPartId, CancellationToken cancellationToken) => _dbContext.MovablePartBindings.AnyAsync(x => x.AssetVersionId == versionId && x.PartCode == partCode && (!excludingPartId.HasValue || x.Id != excludingPartId.Value), cancellationToken);

    private static MovablePartBinding ToEntity(CreateMovablePartCommand command, MovablePartCanonicalObject canonical) => new()
    {
        ModelAssetId = command.AssetId, AssetVersionId = command.VersionId, ObjectUuid = canonical.ObjectUuid, ObjectName = canonical.ObjectName, ObjectPath = canonical.ObjectPath, ParentUuid = canonical.ParentUuid, ParentPath = canonical.ParentPath, BusinessName = command.BusinessName, PartCode = command.PartCode, MotionType = command.MotionType, AxisMode = command.AxisMode, Axis = command.Axis, CustomAxisX = command.CustomAxisX, CustomAxisY = command.CustomAxisY, CustomAxisZ = command.CustomAxisZ, MinValue = command.MinValue, MaxValue = command.MaxValue, HomeValue = command.HomeValue, CurrentValue = command.CurrentValue, DefaultSpeed = command.DefaultSpeed, BindingStatus = command.BindingStatus, Enabled = command.Enabled
    };

    private static void Apply(MovablePartBinding part, UpdateMovablePartCommand command, MovablePartCanonicalObject canonical)
    {
        part.ObjectUuid = canonical.ObjectUuid; part.ObjectName = canonical.ObjectName; part.ObjectPath = canonical.ObjectPath; part.ParentUuid = canonical.ParentUuid; part.ParentPath = canonical.ParentPath; part.BusinessName = command.BusinessName; part.PartCode = command.PartCode; part.MotionType = command.MotionType; part.AxisMode = command.AxisMode; part.Axis = command.Axis; part.CustomAxisX = command.CustomAxisX; part.CustomAxisY = command.CustomAxisY; part.CustomAxisZ = command.CustomAxisZ; part.MinValue = command.MinValue; part.MaxValue = command.MaxValue; part.HomeValue = command.HomeValue; part.CurrentValue = command.CurrentValue; part.DefaultSpeed = command.DefaultSpeed; part.BindingStatus = command.BindingStatus; part.Enabled = command.Enabled;
    }

    private void AddAudit(OperationType operationType, long partId, object? before, object? after) => _dbContext.OperationAudits.Add(new OperationAudit { OperationType = operationType, TargetType = OperationTargetType.movable_part, TargetId = partId, BeforeJson = before is null ? null : JsonSerializer.Serialize(before, AuditJsonOptions), AfterJson = after is null ? null : JsonSerializer.Serialize(after, AuditJsonOptions), OperatorId = null, OperatorName = null, CreatedTime = DateTime.UtcNow });
    private static object ToAuditSnapshot(MovablePartBinding part) => new { part.Id, part.ModelAssetId, part.AssetVersionId, part.ObjectUuid, part.ObjectName, part.ObjectPath, part.ParentUuid, part.ParentPath, part.BusinessName, part.PartCode, motionType = part.MotionType.ToString(), axisMode = part.AxisMode.ToString(), axis = part.Axis.ToString(), part.MinValue, part.MaxValue, part.HomeValue, part.CurrentValue, part.DefaultSpeed, bindingStatus = part.BindingStatus.ToString(), part.Enabled };
    private static MovablePartCanonicalObject ToCanonicalObject(ModelObjectIndex item) => new(item.ObjectUuid, item.ObjectName, item.ObjectPath, item.ParentUuid, item.ParentPath);
    private static MovablePartData ToData(MovablePartBinding item) => new(item.Id, item.ModelAssetId, item.AssetVersionId, item.ObjectUuid, item.ObjectName, item.ObjectPath, item.ParentUuid, item.ParentPath, item.BusinessName, item.PartCode, item.MotionType, item.AxisMode, item.Axis, item.CustomAxisX, item.CustomAxisY, item.CustomAxisZ, item.MinValue, item.MaxValue, item.HomeValue, item.CurrentValue, item.DefaultSpeed, item.BindingStatus, item.Enabled);
    private static bool IsPartCodeUniqueViolation(DbUpdateException exception) => exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: PartCodeUniqueConstraint };
}
