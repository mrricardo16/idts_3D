using HZ.IDTS.DigitalTwin.Application.Storage;
using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

namespace HZ.IDTS.DigitalTwin.Application.ModelAssets;

public interface IModelAssetRepository
{
    Task<bool> AssetCodeExistsAsync(
        string assetCode,
        CancellationToken cancellationToken);

    Task<bool> SourceFileHashExistsAsync(
        string sourceFileHash,
        CancellationToken cancellationToken);

    Task<bool> AssetExistsByIdAsync(
        long assetId,
        CancellationToken cancellationToken);

    Task<ModelManifestQueryData?> GetModelManifestAsync(
        long assetId,
        long? versionId,
        CancellationToken cancellationToken);

    Task<AssetVersionAccessData?> GetAssetVersionAsync(
        long assetId,
        long? versionId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ModelObjectIndexData>> GetObjectTreeAsync(
        long assetId,
        long versionId,
        CancellationToken cancellationToken);

    Task<DateTime> ReplaceObjectTreeAsync(
        long assetId,
        long versionId,
        IReadOnlyList<ObjectTreeNodeRequest> nodes,
        CancellationToken cancellationToken);

    Task<string?> GetModelStatsJsonAsync(
        long assetId,
        long versionId,
        CancellationToken cancellationToken);

    Task<DateTime?> GetModelStatsUpdatedTimeAsync(
        long assetId,
        long versionId,
        CancellationToken cancellationToken);

    Task<SaveModelStatsData?> SaveModelStatsAsync(
        long assetId,
        long versionId,
        SaveModelStatsRequest request,
        CancellationToken cancellationToken);

    Task<UploadModelAssetResponse> CreateUploadAsync(
        CreateModelAssetUploadCommand command,
        Func<long, long, CancellationToken, Task<StoredModelAssetFile>> persistSourceFileAsync,
        Func<long, long, CancellationToken, Task> cleanupSourceFileAsync,
        CancellationToken cancellationToken);
}
