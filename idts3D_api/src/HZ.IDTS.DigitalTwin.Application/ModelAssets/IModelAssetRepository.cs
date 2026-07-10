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

    Task<UploadModelAssetResponse> CreateUploadAsync(
        CreateModelAssetUploadCommand command,
        Func<long, long, CancellationToken, Task<StoredModelAssetFile>> persistSourceFileAsync,
        Func<long, long, CancellationToken, Task> cleanupSourceFileAsync,
        CancellationToken cancellationToken);
}
