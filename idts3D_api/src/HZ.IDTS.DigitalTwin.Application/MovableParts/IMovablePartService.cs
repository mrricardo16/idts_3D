using HZ.IDTS.DigitalTwin.Contracts.MovableParts;

namespace HZ.IDTS.DigitalTwin.Application.MovableParts;

public interface IMovablePartService
{
    Task<MovablePartListResult> GetMovablePartsAsync(GetMovablePartsRequest request, CancellationToken cancellationToken);
    Task<MovablePartResult> CreateAsync(long assetId, long versionId, CreateMovablePartRequest request, CancellationToken cancellationToken);
    Task<MovablePartResult> UpdateAsync(long assetId, long versionId, long partId, UpdateMovablePartRequest request, CancellationToken cancellationToken);
    Task<MovablePartDeleteResult> DeleteAsync(long assetId, long versionId, long partId, CancellationToken cancellationToken);
}
