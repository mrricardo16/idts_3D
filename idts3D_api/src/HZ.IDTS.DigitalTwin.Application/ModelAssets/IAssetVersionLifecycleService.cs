using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

namespace HZ.IDTS.DigitalTwin.Application.ModelAssets;

public interface IAssetVersionLifecycleService
{
    Task<AssetVersionStatusChangeResult> MarkReadyAsync(
        long assetId,
        long versionId,
        ChangeVersionStatusRequest request,
        CancellationToken cancellationToken);

    Task<AssetVersionStatusChangeResult> PublishAsync(
        long assetId,
        long versionId,
        ChangeVersionStatusRequest request,
        CancellationToken cancellationToken);

    Task<AssetVersionStatusChangeResult> ArchiveAsync(
        long assetId,
        long versionId,
        ChangeVersionStatusRequest request,
        CancellationToken cancellationToken);

    Task<AssetVersionStatusChangeResult> RollbackAsync(
        long assetId,
        long versionId,
        ChangeVersionStatusRequest request,
        CancellationToken cancellationToken);
}
