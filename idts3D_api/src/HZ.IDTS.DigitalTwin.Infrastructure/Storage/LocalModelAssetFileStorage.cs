using System.Security.Cryptography;
using HZ.IDTS.DigitalTwin.Application.Storage;

namespace HZ.IDTS.DigitalTwin.Infrastructure.Storage;

public sealed class LocalModelAssetFileStorage : IModelAssetFileStorage
{
    private readonly FileStorageOptions _options;

    public LocalModelAssetFileStorage(FileStorageOptions options)
    {
        _options = options;
    }

    public long MaxFileSizeBytes => _options.MaxFileSizeBytes;

    public bool IsAllowedSourceFile(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        return !string.IsNullOrWhiteSpace(extension) &&
            _options.AllowedExtensions.Contains(extension);
    }

    public async Task<StoredTemporaryModelFile> SaveTemporarySourceFileAsync(
        Stream fileContent,
        string originalFileName,
        CancellationToken cancellationToken)
    {
        EnsureRootPathConfigured();

        var tempDirectory = Path.Combine(_options.RootPath, "_tmp");
        Directory.CreateDirectory(tempDirectory);

        var sourceFileName = Path.GetFileName(originalFileName);
        var tempPath = Path.Combine(tempDirectory, $"{Guid.NewGuid():N}.upload");

        await using var output = new FileStream(
            tempPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 81920,
            useAsync: true);

        using var sha256 = SHA256.Create();
        var buffer = new byte[81920];
        long fileSize = 0;

        while (true)
        {
            var read = await fileContent.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            if (read == 0)
            {
                break;
            }

            fileSize += read;
            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            sha256.TransformBlock(buffer, 0, read, null, 0);
        }

        sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        var hash = Convert.ToHexString(sha256.Hash!).ToLowerInvariant();

        return new StoredTemporaryModelFile(
            tempPath,
            sourceFileName,
            hash,
            fileSize);
    }

    public Task<StoredModelAssetFile> MoveTemporaryToSourceAsync(
        StoredTemporaryModelFile temporaryFile,
        long assetId,
        long versionId,
        CancellationToken cancellationToken)
    {
        EnsureRootPathConfigured();
        cancellationToken.ThrowIfCancellationRequested();

        var sourceDirectory = GetSourceDirectory(assetId, versionId);
        Directory.CreateDirectory(sourceDirectory);

        var destinationPath = Path.Combine(sourceDirectory, "source.glb");
        if (File.Exists(destinationPath))
        {
            File.Delete(destinationPath);
        }

        File.Move(temporaryFile.PhysicalPath, destinationPath);

        return Task.FromResult(new StoredModelAssetFile(
            destinationPath,
            BuildSourceFileUrl(assetId, versionId),
            temporaryFile.SourceFileName,
            temporaryFile.SourceFileHash,
            temporaryFile.FileSize));
    }

    public Task DeleteTemporaryFileAsync(
        StoredTemporaryModelFile temporaryFile,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DeleteFileIfExists(temporaryFile.PhysicalPath);

        return Task.CompletedTask;
    }

    public Task DeleteSourceFileAsync(
        long assetId,
        long versionId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sourceDirectory = GetSourceDirectory(assetId, versionId);
        DeleteFileIfExists(Path.Combine(sourceDirectory, "source.glb"));

        if (Directory.Exists(sourceDirectory) && Directory.GetFileSystemEntries(sourceDirectory).Length == 0)
        {
            Directory.Delete(sourceDirectory);
        }

        return Task.CompletedTask;
    }

    private string BuildSourceFileUrl(long assetId, long versionId)
    {
        return $"{_options.PublicBaseUrl}/models/{assetId}/{versionId}/source.glb";
    }

    private string GetSourceDirectory(long assetId, long versionId)
    {
        return Path.Combine(
            _options.RootPath,
            "models",
            assetId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            versionId.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    private void EnsureRootPathConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.RootPath))
        {
            throw new InvalidOperationException("FileStorage:RootPath is required.");
        }
    }

    private static void DeleteFileIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
