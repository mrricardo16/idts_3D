using HZ.IDTS.DigitalTwin.Application.Storage;

namespace HZ.IDTS.DigitalTwin.Application.Tests.Fakes;

internal sealed class FakeModelAssetFileStorage : IModelAssetFileStorage
{
    public long MaxFileSizeBytes { get; set; } = 1024 * 1024;

    public bool IsAllowedSourceFileResult { get; set; } = true;

    public StoredTemporaryModelFile TemporaryFile { get; set; } = new(
        "temporary.glb",
        "model.glb",
        "hash-1",
        3);

    public Exception? IsAllowedSourceFileException { get; set; }

    public Exception? SaveTemporaryException { get; set; }

    public Exception? MoveTemporaryException { get; set; }

    public Exception? DeleteTemporaryException { get; set; }

    public Exception? DeleteSourceException { get; set; }

    public int IsAllowedSourceFileCallCount { get; private set; }

    public int SaveTemporaryCallCount { get; private set; }

    public int MoveTemporaryCallCount { get; private set; }

    public int DeleteTemporaryCallCount { get; private set; }

    public int DeleteSourceCallCount { get; private set; }

    public StoredTemporaryModelFile? DeletedTemporaryFile { get; private set; }

    public (long AssetId, long VersionId)? DeletedSourceFile { get; private set; }

    public string? LastAllowedSourceFileName { get; private set; }

    public (Stream FileContent, string OriginalFileName)? LastTemporarySaveRequest { get; private set; }

    public (StoredTemporaryModelFile TemporaryFile, long AssetId, long VersionId)? LastMoveRequest { get; private set; }

    public bool IsAllowedSourceFile(string fileName)
    {
        IsAllowedSourceFileCallCount++;
        LastAllowedSourceFileName = fileName;
        if (IsAllowedSourceFileException is not null) throw IsAllowedSourceFileException;
        return IsAllowedSourceFileResult;
    }

    public Task<StoredTemporaryModelFile> SaveTemporarySourceFileAsync(
        Stream fileContent,
        string originalFileName,
        CancellationToken cancellationToken)
    {
        SaveTemporaryCallCount++;
        LastTemporarySaveRequest = (fileContent, originalFileName);
        if (SaveTemporaryException is not null) throw SaveTemporaryException;
        return Task.FromResult(TemporaryFile);
    }

    public Task<StoredModelAssetFile> MoveTemporaryToSourceAsync(
        StoredTemporaryModelFile temporaryFile,
        long assetId,
        long versionId,
        CancellationToken cancellationToken)
    {
        MoveTemporaryCallCount++;
        LastMoveRequest = (temporaryFile, assetId, versionId);
        if (MoveTemporaryException is not null) throw MoveTemporaryException;
        return Task.FromResult(new StoredModelAssetFile(
            "source.glb",
            $"/assets/models/{assetId}/{versionId}/source.glb",
            temporaryFile.SourceFileName,
            temporaryFile.SourceFileHash,
            temporaryFile.FileSize));
    }

    public Task DeleteTemporaryFileAsync(
        StoredTemporaryModelFile temporaryFile,
        CancellationToken cancellationToken)
    {
        DeleteTemporaryCallCount++;
        DeletedTemporaryFile = temporaryFile;
        if (DeleteTemporaryException is not null) throw DeleteTemporaryException;
        return Task.CompletedTask;
    }

    public Task DeleteSourceFileAsync(long assetId, long versionId, CancellationToken cancellationToken)
    {
        DeleteSourceCallCount++;
        DeletedSourceFile = (assetId, versionId);
        if (DeleteSourceException is not null) throw DeleteSourceException;
        return Task.CompletedTask;
    }
}
