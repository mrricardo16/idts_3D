namespace HZ.IDTS.DigitalTwin.Application.Storage;

public interface IModelAssetFileStorage
{
    long MaxFileSizeBytes { get; }

    bool IsAllowedSourceFile(string fileName);

    Task<StoredTemporaryModelFile> SaveTemporarySourceFileAsync(
        Stream fileContent,
        string originalFileName,
        CancellationToken cancellationToken);

    Task<StoredModelAssetFile> MoveTemporaryToSourceAsync(
        StoredTemporaryModelFile temporaryFile,
        long assetId,
        long versionId,
        CancellationToken cancellationToken);

    Task DeleteTemporaryFileAsync(
        StoredTemporaryModelFile temporaryFile,
        CancellationToken cancellationToken);

    Task DeleteSourceFileAsync(
        long assetId,
        long versionId,
        CancellationToken cancellationToken);
}
