using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

namespace HZ.IDTS.DigitalTwin.Application.ModelAssets;

public interface IModelManifestService
{
    Task<ModelManifestResult> GetManifestAsync(
        GetModelManifestRequest request,
        CancellationToken cancellationToken);
}
