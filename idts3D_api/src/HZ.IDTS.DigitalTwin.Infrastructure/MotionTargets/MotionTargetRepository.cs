using System.Text.Json;
using HZ.IDTS.DigitalTwin.Application.MotionTargets;
using HZ.IDTS.DigitalTwin.Domain.Entities;
using HZ.IDTS.DigitalTwin.Domain.Enums;
using HZ.IDTS.DigitalTwin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HZ.IDTS.DigitalTwin.Infrastructure.MotionTargets;

public sealed class MotionTargetRepository : IMotionTargetRepository
{
    private const string TargetCodeUniqueConstraint = "IX_motion_target_movable_part_id_target_code";
    private static readonly JsonSerializerOptions AuditJsonOptions = new(JsonSerializerDefaults.Web);
    private readonly DigitalTwinDbContext _dbContext;

    public MotionTargetRepository(DigitalTwinDbContext dbContext) => _dbContext = dbContext;

    public async Task<MotionTargetPartLookup> GetPartAsync(long partId, CancellationToken cancellationToken)
    {
        var part = await _dbContext.MovablePartBindings.AsNoTracking()
            .Where(x => x.Id == partId)
            .Select(x => new { x.AssetVersionId, x.BindingStatus, x.MotionType, x.AxisMode, x.Axis, x.MinValue, x.MaxValue, x.Enabled })
            .SingleOrDefaultAsync(cancellationToken);
        if (part is null) return new(false, false, null, null, null, null, null, null, null, null);

        var versionStatus = await _dbContext.AssetVersions.AsNoTracking()
            .Where(x => x.Id == part.AssetVersionId)
            .Select(x => (VersionStatus?)x.VersionStatus)
            .SingleOrDefaultAsync(cancellationToken);
        return new(true, versionStatus.HasValue, versionStatus, part.BindingStatus, part.MotionType, part.AxisMode, part.Axis, part.MinValue, part.MaxValue, part.Enabled);
    }

    public async Task<IReadOnlyList<MotionTargetData>> GetListAsync(long partId, bool? enabled, CancellationToken cancellationToken)
    {
        return await _dbContext.MotionTargets.AsNoTracking()
            .Where(x => x.MovablePartId == partId && (!enabled.HasValue || x.Enabled == enabled.Value) && x.TargetValue != null)
            .OrderBy(x => x.SortNo).ThenBy(x => x.TargetCode).ThenBy(x => x.Id)
            .Select(x => new MotionTargetData(x.Id, x.MovablePartId, x.TargetCode, x.TargetName, x.TargetValue!.Value, x.TargetX, x.TargetY, x.TargetZ ?? x.TargetValue.Value, x.SortNo, x.Enabled))
            .ToListAsync(cancellationToken);
    }

    public async Task<MotionTargetWriteResult> CreateAsync(CreateMotionTargetCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var lockedPart = await LockWritablePartAsync(command.PartId, cancellationToken);
            if (lockedPart.Failure != MotionTargetWriteFailure.None) return new(lockedPart.Failure, null);
            if (await TargetCodeExistsAsync(command.PartId, command.TargetCode, null, cancellationToken)) return new(MotionTargetWriteFailure.DuplicateTargetCode, null);

            var target = new MotionTarget { MovablePartId = command.PartId, TargetCode = command.TargetCode, TargetName = command.TargetName, TargetValue = command.TargetValue, TargetX = null, TargetY = null, TargetZ = command.TargetZ, SortNo = command.SortNo, Enabled = command.Enabled };
            _dbContext.MotionTargets.Add(target);
            await _dbContext.SaveChangesAsync(cancellationToken);
            AddAudit(OperationType.edit, target, null, "create");
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new(MotionTargetWriteFailure.None, ToData(target));
        }
        catch (DbUpdateException exception) when (IsTargetCodeUniqueViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return new(MotionTargetWriteFailure.DuplicateTargetCode, null);
        }
    }

    public async Task<MotionTargetWriteResult> UpdateAsync(UpdateMotionTargetCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var lockedPart = await LockWritablePartAsync(command.PartId, cancellationToken);
            if (lockedPart.Failure != MotionTargetWriteFailure.None) return new(lockedPart.Failure, null);
            var target = await _dbContext.MotionTargets.FromSqlInterpolated($"SELECT * FROM motion_target WHERE id = {command.TargetId} AND movable_part_id = {command.PartId} FOR UPDATE").SingleOrDefaultAsync(cancellationToken);
            if (target is null) return new(MotionTargetWriteFailure.TargetNotFound, null);
            if (await TargetCodeExistsAsync(command.PartId, command.TargetCode, command.TargetId, cancellationToken)) return new(MotionTargetWriteFailure.DuplicateTargetCode, null);

            var before = ToAuditSnapshot(target, "update");
            target.TargetCode = command.TargetCode;
            target.TargetName = command.TargetName;
            target.TargetValue = command.TargetValue;
            target.TargetX = null;
            target.TargetY = null;
            target.TargetZ = command.TargetZ;
            target.SortNo = command.SortNo;
            target.Enabled = command.Enabled;
            AddAudit(OperationType.edit, target, before, "update");
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new(MotionTargetWriteFailure.None, ToData(target));
        }
        catch (DbUpdateException exception) when (IsTargetCodeUniqueViolation(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return new(MotionTargetWriteFailure.DuplicateTargetCode, null);
        }
    }

    public async Task<MotionTargetWriteResult> DeleteAsync(DeleteMotionTargetCommand command, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        var lockedPart = await LockWritablePartAsync(command.PartId, cancellationToken);
        if (lockedPart.Failure != MotionTargetWriteFailure.None) return new(lockedPart.Failure, null);
        var target = await _dbContext.MotionTargets.FromSqlInterpolated($"SELECT * FROM motion_target WHERE id = {command.TargetId} AND movable_part_id = {command.PartId} FOR UPDATE").SingleOrDefaultAsync(cancellationToken);
        if (target is null) return new(MotionTargetWriteFailure.TargetNotFound, null);

        var before = ToAuditSnapshot(target, "delete");
        var data = ToData(target);
        _dbContext.MotionTargets.Remove(target);
        AddAudit(OperationType.delete, target, before, null);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(MotionTargetWriteFailure.None, data);
    }

    private async Task<(MotionTargetWriteFailure Failure, MovablePartBinding? Part)> LockWritablePartAsync(long partId, CancellationToken cancellationToken)
    {
        var versionId = await _dbContext.MovablePartBindings.AsNoTracking().Where(x => x.Id == partId).Select(x => (long?)x.AssetVersionId).SingleOrDefaultAsync(cancellationToken);
        if (!versionId.HasValue) return new(MotionTargetWriteFailure.PartNotFound, null);
        var version = await _dbContext.AssetVersions.FromSqlInterpolated($"SELECT * FROM asset_version WHERE id = {versionId.Value} FOR UPDATE").SingleOrDefaultAsync(cancellationToken);
        if (version is null) return new(MotionTargetWriteFailure.VersionNotFound, null);
        if (version.VersionStatus is not (VersionStatus.Draft or VersionStatus.Ready)) return new(MotionTargetWriteFailure.VersionNotWritable, null);

        var part = await _dbContext.MovablePartBindings.FromSqlInterpolated($"SELECT * FROM movable_part_binding WHERE id = {partId} AND asset_version_id = {versionId.Value} FOR UPDATE").SingleOrDefaultAsync(cancellationToken);
        if (part is null) return new(MotionTargetWriteFailure.PartNotFound, null);
        return part.BindingStatus == BindingStatus.active && part.MotionType == MotionType.linear && part.AxisMode == AxisMode.world && part.Axis == Axis.z
            ? new(MotionTargetWriteFailure.None, part)
            : new(MotionTargetWriteFailure.PartNotConfigurable, null);
    }

    private Task<bool> TargetCodeExistsAsync(long partId, string targetCode, long? excludingTargetId, CancellationToken cancellationToken) =>
        _dbContext.MotionTargets.AnyAsync(x => x.MovablePartId == partId && x.TargetCode == targetCode && (!excludingTargetId.HasValue || x.Id != excludingTargetId.Value), cancellationToken);

    private void AddAudit(OperationType operationType, MotionTarget target, object? before, string? action)
    {
        var operationTime = DateTime.UtcNow;
        _dbContext.OperationAudits.Add(new OperationAudit { OperationType = operationType, TargetType = OperationTargetType.motion_target, TargetId = target.Id, BeforeJson = before is null ? null : JsonSerializer.Serialize(before, AuditJsonOptions), AfterJson = action is null ? null : JsonSerializer.Serialize(ToAuditSnapshot(target, action, operationTime), AuditJsonOptions), OperatorId = null, OperatorName = null, CreatedTime = operationTime });
    }

    private static object ToAuditSnapshot(MotionTarget target, string action, DateTime? operationTime = null) => new { targetId = target.Id, partId = target.MovablePartId, target.TargetCode, target.TargetName, target.TargetValue, target.TargetX, target.TargetY, target.TargetZ, target.SortNo, target.Enabled, action, operationTime = operationTime ?? DateTime.UtcNow };
    private static MotionTargetData ToData(MotionTarget target) => new(target.Id, target.MovablePartId, target.TargetCode, target.TargetName, target.TargetValue!.Value, target.TargetX, target.TargetY, target.TargetZ ?? target.TargetValue.Value, target.SortNo, target.Enabled);
    private static bool IsTargetCodeUniqueViolation(DbUpdateException exception) => exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation, ConstraintName: TargetCodeUniqueConstraint };
}
