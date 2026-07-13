using HZ.IDTS.DigitalTwin.Application.ModelAssets;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

namespace HZ.IDTS.DigitalTwin.Api.IntegrationTests.Fakes;

public sealed class FakeAssetVersionLifecycleService : IAssetVersionLifecycleService
{
    public int CallCount { get; private set; }

    public long LastAssetId { get; private set; }

    public long LastVersionId { get; private set; }

    public ChangeVersionStatusRequest? LastRequest { get; private set; }

    public string? LastOperation { get; private set; }

    public AssetVersionStatusChangeResult Result { get; set; } = new(
        HttpStatusCode.OK,
        ApiResponse<AssetVersionResponse>.Ok(new AssetVersionResponse(101, 202, "Published", DateTime.UtcNow)));

    public Task<AssetVersionStatusChangeResult> MarkReadyAsync(long assetId, long versionId, ChangeVersionStatusRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(RecordCall(assetId, versionId, request, "mark-ready"));

    public Task<AssetVersionStatusChangeResult> PublishAsync(long assetId, long versionId, ChangeVersionStatusRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(RecordCall(assetId, versionId, request, "publish"));

    public Task<AssetVersionStatusChangeResult> ArchiveAsync(long assetId, long versionId, ChangeVersionStatusRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(RecordCall(assetId, versionId, request, "archive"));

    public Task<AssetVersionStatusChangeResult> RollbackAsync(long assetId, long versionId, ChangeVersionStatusRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(RecordCall(assetId, versionId, request, "rollback"));

    private AssetVersionStatusChangeResult RecordCall(long assetId, long versionId, ChangeVersionStatusRequest request, string operation)
    {
        CallCount++;
        LastAssetId = assetId;
        LastVersionId = versionId;
        LastRequest = request;
        LastOperation = operation;
        return Result;
    }
}
