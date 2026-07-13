using HZ.IDTS.DigitalTwin.Application.ModelAssets;
using HZ.IDTS.DigitalTwin.Application.Storage;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;
using HZ.IDTS.DigitalTwin.Domain.Enums;

namespace HZ.IDTS.DigitalTwin.Application.Tests.Fakes;

internal sealed class FakeModelAssetRepository : IModelAssetRepository
{
    public bool AssetCodeExistsResult { get; set; }

    public bool SourceFileHashExistsResult { get; set; }

    public bool AssetExistsResult { get; set; } = true;

    public ModelManifestQueryData? ManifestResult { get; set; }

    public AssetVersionAccessData? AssetVersionResult { get; set; }

    public IReadOnlyList<ModelObjectIndexData> ObjectTreeResult { get; set; } = Array.Empty<ModelObjectIndexData>();

    public string? ModelStatsJsonResult { get; set; }

    public DateTime? ModelStatsUpdatedTimeResult { get; set; }

    public SaveModelStatsData? SaveModelStatsResult { get; set; } = new(new DateTime(2026, 7, 1));

    public AssetVersionLifecycleRepositoryResult LifecycleResult { get; set; } = new(
        true,
        true,
        VersionStatus.Ready,
        new DateTime(2026, 7, 1),
        null,
        null,
        Array.Empty<ApiErrorItem>());

    public UploadModelAssetResponse UploadResponse { get; set; } = new(
        10,
        20,
        30,
        "asset-01",
        "Draft",
        "pending",
        "hash-1",
        "/assets/models/10/20/source.glb");

    public Func<
        CreateModelAssetUploadCommand,
        Func<long, long, CancellationToken, Task<StoredModelAssetFile>>,
        Func<long, long, CancellationToken, Task>,
        CancellationToken,
        Task<UploadModelAssetResponse>>? CreateUploadHandler { get; set; }

    public Exception? ExceptionToThrow { get; set; }

    public int AssetCodeExistsCallCount { get; private set; }

    public int SourceFileHashExistsCallCount { get; private set; }

    public int AssetExistsCallCount { get; private set; }

    public int CreateUploadCallCount { get; private set; }

    public int ReplaceObjectTreeCallCount { get; private set; }

    public int SaveModelStatsCallCount { get; private set; }

    public int LifecycleCallCount { get; private set; }

    public CreateModelAssetUploadCommand? LastCreateUploadCommand { get; private set; }

    public AssetVersionLifecycleCommand? LastLifecycleCommand { get; private set; }

    public bool LastManifestUsedPublishedBaseline { get; private set; }

    public bool LastAssetVersionUsedPublishedBaseline { get; private set; }

    public string? LastAssetCodeExistsArgument { get; private set; }

    public string? LastSourceFileHashExistsArgument { get; private set; }

    public long? LastAssetExistsArgument { get; private set; }

    public (long AssetId, long? VersionId)? LastManifestRequest { get; private set; }

    public (long AssetId, long? VersionId)? LastAssetVersionRequest { get; private set; }

    public (long AssetId, long VersionId)? LastObjectTreeRequest { get; private set; }

    public (long AssetId, long VersionId, int NodeCount)? LastReplaceObjectTreeRequest { get; private set; }

    public (long AssetId, long VersionId)? LastModelStatsRequest { get; private set; }

    public (long AssetId, long VersionId)? LastModelStatsUpdatedTimeRequest { get; private set; }

    public (long AssetId, long VersionId)? LastSaveModelStatsRequest { get; private set; }

    public Task<bool> AssetCodeExistsAsync(string assetCode, CancellationToken cancellationToken)
    {
        AssetCodeExistsCallCount++;
        LastAssetCodeExistsArgument = assetCode;
        ThrowIfConfigured();
        return Task.FromResult(AssetCodeExistsResult);
    }

    public Task<bool> SourceFileHashExistsAsync(string sourceFileHash, CancellationToken cancellationToken)
    {
        SourceFileHashExistsCallCount++;
        LastSourceFileHashExistsArgument = sourceFileHash;
        ThrowIfConfigured();
        return Task.FromResult(SourceFileHashExistsResult);
    }

    public Task<bool> AssetExistsByIdAsync(long assetId, CancellationToken cancellationToken)
    {
        AssetExistsCallCount++;
        LastAssetExistsArgument = assetId;
        ThrowIfConfigured();
        return Task.FromResult(AssetExistsResult);
    }

    public Task<ModelManifestQueryData?> GetModelManifestAsync(
        long assetId,
        long? versionId,
        bool usePublishedBaseline,
        CancellationToken cancellationToken)
    {
        LastManifestUsedPublishedBaseline = usePublishedBaseline;
        LastManifestRequest = (assetId, versionId);
        ThrowIfConfigured();
        return Task.FromResult(ManifestResult);
    }

    public Task<AssetVersionAccessData?> GetAssetVersionAsync(
        long assetId,
        long? versionId,
        bool usePublishedBaseline,
        CancellationToken cancellationToken)
    {
        LastAssetVersionUsedPublishedBaseline = usePublishedBaseline;
        LastAssetVersionRequest = (assetId, versionId);
        ThrowIfConfigured();
        return Task.FromResult(AssetVersionResult);
    }

    public Task<IReadOnlyList<ModelObjectIndexData>> GetObjectTreeAsync(long assetId, long versionId, CancellationToken cancellationToken)
    {
        LastObjectTreeRequest = (assetId, versionId);
        ThrowIfConfigured();
        return Task.FromResult(ObjectTreeResult);
    }

    public Task<DateTime> ReplaceObjectTreeAsync(
        long assetId,
        long versionId,
        IReadOnlyList<ObjectTreeNodeRequest> nodes,
        CancellationToken cancellationToken)
    {
        ReplaceObjectTreeCallCount++;
        LastReplaceObjectTreeRequest = (assetId, versionId, nodes.Count);
        ThrowIfConfigured();
        return Task.FromResult(new DateTime(2026, 7, 1));
    }

    public Task<string?> GetModelStatsJsonAsync(long assetId, long versionId, CancellationToken cancellationToken)
    {
        LastModelStatsRequest = (assetId, versionId);
        ThrowIfConfigured();
        return Task.FromResult(ModelStatsJsonResult);
    }

    public Task<DateTime?> GetModelStatsUpdatedTimeAsync(long assetId, long versionId, CancellationToken cancellationToken)
    {
        LastModelStatsUpdatedTimeRequest = (assetId, versionId);
        ThrowIfConfigured();
        return Task.FromResult(ModelStatsUpdatedTimeResult);
    }

    public Task<SaveModelStatsData?> SaveModelStatsAsync(
        long assetId,
        long versionId,
        SaveModelStatsRequest request,
        CancellationToken cancellationToken)
    {
        SaveModelStatsCallCount++;
        LastSaveModelStatsRequest = (assetId, versionId);
        ThrowIfConfigured();
        return Task.FromResult(SaveModelStatsResult);
    }

    public Task<AssetVersionLifecycleRepositoryResult> ExecuteVersionLifecycleAsync(
        AssetVersionLifecycleCommand command,
        CancellationToken cancellationToken)
    {
        LifecycleCallCount++;
        LastLifecycleCommand = command;
        ThrowIfConfigured();
        return Task.FromResult(LifecycleResult);
    }

    public async Task<UploadModelAssetResponse> CreateUploadAsync(
        CreateModelAssetUploadCommand command,
        Func<long, long, CancellationToken, Task<StoredModelAssetFile>> persistSourceFileAsync,
        Func<long, long, CancellationToken, Task> cleanupSourceFileAsync,
        CancellationToken cancellationToken)
    {
        CreateUploadCallCount++;
        LastCreateUploadCommand = command;

        ThrowIfConfigured();

        if (CreateUploadHandler is not null)
        {
            return await CreateUploadHandler(command, persistSourceFileAsync, cleanupSourceFileAsync, cancellationToken);
        }

        await persistSourceFileAsync(UploadResponse.AssetId, UploadResponse.VersionId, cancellationToken);
        return UploadResponse;
    }

    private void ThrowIfConfigured()
    {
        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }
    }
}
