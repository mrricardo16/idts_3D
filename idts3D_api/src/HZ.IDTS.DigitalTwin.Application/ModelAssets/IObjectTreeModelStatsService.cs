using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

namespace HZ.IDTS.DigitalTwin.Application.ModelAssets;

public interface IObjectTreeModelStatsService
{
    Task<ObjectTreeResult> SaveObjectTreeAsync(
        long assetId,
        long versionId,
        SaveObjectTreeRequest request,
        CancellationToken cancellationToken);

    Task<ObjectTreeResult> GetObjectTreeAsync(
        GetObjectTreeRequest request,
        CancellationToken cancellationToken);

    Task<ModelStatsResult> SaveModelStatsAsync(
        long assetId,
        long versionId,
        SaveModelStatsRequest request,
        CancellationToken cancellationToken);

    Task<ModelStatsResult> GetModelStatsAsync(
        GetModelStatsRequest request,
        CancellationToken cancellationToken);
}

public sealed record ObjectTreeResult(
    System.Net.HttpStatusCode StatusCode,
    ApiResponse<ObjectTreeResponse> Response);

public sealed record ModelStatsResult(
    System.Net.HttpStatusCode StatusCode,
    ApiResponse<ModelStatsResponse> Response);
