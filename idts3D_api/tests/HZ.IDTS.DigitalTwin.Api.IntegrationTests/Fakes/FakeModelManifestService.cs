using HZ.IDTS.DigitalTwin.Application.ModelAssets;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

namespace HZ.IDTS.DigitalTwin.Api.IntegrationTests.Fakes;

public sealed class FakeModelManifestService : IModelManifestService
{
    public int CallCount { get; private set; }

    public GetModelManifestRequest? LastRequest { get; private set; }

    public Exception? ExceptionToThrow { get; set; }

    public ModelManifestResult Result { get; set; } = new(
        HttpStatusCode.OK,
        ApiResponse<ModelManifestResponse>.Ok(CreateResponse()));

    public Task<ModelManifestResult> GetManifestAsync(
        GetModelManifestRequest request,
        CancellationToken cancellationToken)
    {
        CallCount++;
        LastRequest = request;

        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        return Task.FromResult(Result);
    }

    public static ModelManifestResponse CreateResponse() => new(
        101,
        202,
        "TEST-GLB",
        "Test GLB",
        "Draft",
        new ModelManifestLevelsResponse("/assets/models/101/202/source.glb", null, null, null, null),
        new ModelTransformResponse(
            new ManifestVector3Response(0, 0, 0),
            new ManifestVector3Response(0, 0, 0),
            new ManifestVector3Response(1, 1, 1)),
        Array.Empty<ModelManifestMovablePartResponse>());
}
