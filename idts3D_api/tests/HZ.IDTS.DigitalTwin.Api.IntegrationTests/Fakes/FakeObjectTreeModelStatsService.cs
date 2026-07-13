using HZ.IDTS.DigitalTwin.Application.ModelAssets;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

namespace HZ.IDTS.DigitalTwin.Api.IntegrationTests.Fakes;

public sealed class FakeObjectTreeModelStatsService : IObjectTreeModelStatsService
{
    private static readonly ObjectTreeResult ObjectTreeResult = new(
        HttpStatusCode.OK,
        ApiResponse<ObjectTreeResponse>.Ok(new ObjectTreeResponse(101, 202, 0, Array.Empty<ObjectTreeNodeResponse>(), DateTime.UtcNow)));

    private static readonly ModelStatsResult ModelStatsResult = new(
        HttpStatusCode.OK,
        ApiResponse<ModelStatsResponse>.Ok(new ModelStatsResponse(101, 202, 0, 0, 0, 0, 0, 0, 0, 0, false, false, false, false, Array.Empty<string>(), DateTime.UtcNow)));

    public Task<ObjectTreeResult> SaveObjectTreeAsync(long assetId, long versionId, SaveObjectTreeRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(ObjectTreeResult);

    public Task<ObjectTreeResult> GetObjectTreeAsync(GetObjectTreeRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(ObjectTreeResult);

    public Task<ModelStatsResult> SaveModelStatsAsync(long assetId, long versionId, SaveModelStatsRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(ModelStatsResult);

    public Task<ModelStatsResult> GetModelStatsAsync(GetModelStatsRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(ModelStatsResult);
}
