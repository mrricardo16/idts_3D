namespace HZ.IDTS.DigitalTwin.Application.MovableParts;

public interface IMovablePartRepository
{
    Task<MovablePartVersionLookup> GetVersionAsync(long assetId, long versionId, CancellationToken cancellationToken);
    Task<MovablePartObjectResolution> ResolveCanonicalObjectAsync(long assetId, long versionId, string? objectUuid, string? objectPath, CancellationToken cancellationToken);
    Task<IReadOnlyList<MovablePartData>> GetListAsync(long assetId, long versionId, bool? enabled, CancellationToken cancellationToken);
    Task<MovablePartWriteResult> CreateAsync(CreateMovablePartCommand command, CancellationToken cancellationToken);
    Task<MovablePartWriteResult> UpdateAsync(UpdateMovablePartCommand command, CancellationToken cancellationToken);
    Task<MovablePartWriteResult> DeleteAsync(DeleteMovablePartCommand command, CancellationToken cancellationToken);
}
