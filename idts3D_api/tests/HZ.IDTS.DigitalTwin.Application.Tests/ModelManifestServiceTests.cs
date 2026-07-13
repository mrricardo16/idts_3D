using System.Net;
using HZ.IDTS.DigitalTwin.Application.ModelAssets;
using HZ.IDTS.DigitalTwin.Application.Tests.Fakes;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;
using HZ.IDTS.DigitalTwin.Domain.Enums;

namespace HZ.IDTS.DigitalTwin.Application.Tests;

public sealed class ModelManifestServiceTests
{
    [Fact]
    public async Task Given_InvalidMode_When_GettingManifest_Then_ReturnsValidationFailureWithoutRepositoryAccess()
    {
        var repository = new FakeModelAssetRepository();
        var service = new ModelManifestService(repository);

        var result = await service.GetManifestAsync(new GetModelManifestRequest(1, 2, "preview"), CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(ErrorCode.ValidationFailed, result.Response.Code);
        Assert.Equal(0, repository.AssetExistsCallCount);
    }

    [Fact]
    public async Task Given_MissingAsset_When_GettingManifest_Then_ReturnsAssetNotFound()
    {
        var repository = new FakeModelAssetRepository { AssetExistsResult = false };
        var service = new ModelManifestService(repository);

        var result = await service.GetManifestAsync(new GetModelManifestRequest(7, null, "monitor"), CancellationToken.None);

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.False(result.Response.Success);
        Assert.Equal(ErrorCode.AssetNotFound, result.Response.Code);
        Assert.Equal(1, repository.AssetExistsCallCount);
    }

    [Fact]
    public async Task Given_DraftVersionInMonitorMode_When_GettingManifest_Then_ReturnsVersionStatusConflict()
    {
        var repository = new FakeModelAssetRepository { ManifestResult = Manifest(VersionStatus.Draft) };
        var service = new ModelManifestService(repository);

        var result = await service.GetManifestAsync(new GetModelManifestRequest(1, 2, "monitor"), CancellationToken.None);

        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        Assert.Equal(ErrorCode.VersionStatusInvalid, result.Response.Code);
        Assert.True(repository.LastManifestUsedPublishedBaseline);
    }

    [Fact]
    public async Task Given_DraftVersionInEditMode_When_GettingManifest_Then_ReturnsManifest()
    {
        var repository = new FakeModelAssetRepository { ManifestResult = Manifest(VersionStatus.Draft) };
        var service = new ModelManifestService(repository);

        var result = await service.GetManifestAsync(new GetModelManifestRequest(1, 2, "edit"), CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(result.Response.Success);
        Assert.Equal("asset-01", result.Response.Data!.AssetCode);
        Assert.Equal("/assets/source.glb", result.Response.Data.Levels.Source);
        Assert.False(repository.LastManifestUsedPublishedBaseline);
    }

    private static ModelManifestQueryData Manifest(VersionStatus versionStatus) => new(
        1,
        2,
        "asset-01",
        "Asset",
        versionStatus,
        new[] { new ModelManifestVariantData(VariantLevel.source, "/assets/source.glb") },
        null,
        Array.Empty<ModelManifestMovablePartData>());
}
