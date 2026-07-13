using System.Net;
using HZ.IDTS.DigitalTwin.Application.ModelAssets;
using HZ.IDTS.DigitalTwin.Application.Tests.Fakes;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;
using HZ.IDTS.DigitalTwin.Domain.Enums;

namespace HZ.IDTS.DigitalTwin.Application.Tests;

public sealed class ObjectTreeModelStatsServiceTests
{
    [Fact]
    public async Task Given_EmptyObjectTree_When_Saving_Then_ReturnsValidationFailureWithoutRepositoryAccess()
    {
        var repository = new FakeModelAssetRepository();
        var service = new ObjectTreeModelStatsService(repository);

        var result = await service.SaveObjectTreeAsync(1, 2, new SaveObjectTreeRequest(Array.Empty<ObjectTreeNodeRequest>()), CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(ErrorCode.ValidationFailed, result.Response.Code);
        Assert.Equal(0, repository.ReplaceObjectTreeCallCount);
    }

    [Fact]
    public async Task Given_PublishedVersion_When_SavingObjectTree_Then_ReturnsVersionStatusConflict()
    {
        var repository = new FakeModelAssetRepository
        {
            AssetVersionResult = new AssetVersionAccessData(1, 2, VersionStatus.Published)
        };
        var service = new ObjectTreeModelStatsService(repository);

        var result = await service.SaveObjectTreeAsync(1, 2, ValidTree(), CancellationToken.None);

        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        Assert.Equal(ErrorCode.VersionStatusInvalid, result.Response.Code);
        Assert.Equal(0, repository.ReplaceObjectTreeCallCount);
    }

    [Fact]
    public async Task Given_DraftVersionAndValidTree_When_SavingObjectTree_Then_CallsRepositoryAndReturnsNodeCount()
    {
        var repository = new FakeModelAssetRepository
        {
            AssetVersionResult = new AssetVersionAccessData(1, 2, VersionStatus.Draft)
        };
        var service = new ObjectTreeModelStatsService(repository);

        var result = await service.SaveObjectTreeAsync(1, 2, ValidTree(), CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(result.Response.Success);
        Assert.Equal(1, repository.ReplaceObjectTreeCallCount);
        Assert.Equal(1, result.Response.Data!.NodeCount);
    }

    [Fact]
    public async Task Given_NegativeModelStats_When_Saving_Then_ReturnsValidationFailure()
    {
        var repository = new FakeModelAssetRepository();
        var service = new ObjectTreeModelStatsService(repository);
        var request = new SaveModelStatsRequest(-1, 0, 0, 0, 0, 0, 0, 0, false, false, false, false, null);

        var result = await service.SaveModelStatsAsync(1, 2, request, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(ErrorCode.ValidationFailed, result.Response.Code);
        Assert.Equal(0, repository.SaveModelStatsCallCount);
    }

    private static SaveObjectTreeRequest ValidTree() => new(
        new[] { new ObjectTreeNodeRequest("uuid", "Object", "/Object", null, null, "Mesh", null, null) });
}
