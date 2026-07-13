using System.Net;
using HZ.IDTS.DigitalTwin.Application.ModelAssets;
using HZ.IDTS.DigitalTwin.Application.Tests.Fakes;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;
using HZ.IDTS.DigitalTwin.Domain.Enums;

namespace HZ.IDTS.DigitalTwin.Application.Tests;

public sealed class AssetVersionLifecycleServiceTests
{
    [Fact]
    public async Task Given_TooLongRemark_When_MarkingReady_Then_ReturnsValidationFailureWithoutRepositoryCall()
    {
        var repository = new FakeModelAssetRepository();
        var service = new AssetVersionLifecycleService(repository);

        var result = await service.MarkReadyAsync(1, 2, new ChangeVersionStatusRequest(new string('a', 501)), CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(ErrorCode.ValidationFailed, result.Response.Code);
        Assert.Equal(0, repository.LifecycleCallCount);
    }

    [Fact]
    public async Task Given_MissingAsset_When_Publishing_Then_ReturnsAssetNotFound()
    {
        var repository = new FakeModelAssetRepository
        {
            LifecycleResult = new AssetVersionLifecycleRepositoryResult(
                false,
                false,
                null,
                null,
                null,
                null,
                Array.Empty<ApiErrorItem>())
        };
        var service = new AssetVersionLifecycleService(repository);

        var result = await service.PublishAsync(1, 2, new ChangeVersionStatusRequest(null), CancellationToken.None);

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(ErrorCode.AssetNotFound, result.Response.Code);
        Assert.Equal(AssetVersionLifecycleOperation.Publish, repository.LastLifecycleCommand!.Operation);
    }

    [Fact]
    public async Task Given_MissingManifestGate_When_MarkingReady_Then_ReturnsBusinessFailure()
    {
        var repository = new FakeModelAssetRepository
        {
            LifecycleResult = new AssetVersionLifecycleRepositoryResult(
                true,
                true,
                VersionStatus.Draft,
                null,
                ErrorCode.ManifestRequired,
                "manifest required",
                Array.Empty<ApiErrorItem>())
        };
        var service = new AssetVersionLifecycleService(repository);

        var result = await service.MarkReadyAsync(1, 2, new ChangeVersionStatusRequest(null), CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(ErrorCode.ManifestRequired, result.Response.Code);
        Assert.Equal(AssetVersionLifecycleOperation.MarkReady, repository.LastLifecycleCommand!.Operation);
    }

    [Fact]
    public async Task Given_InvalidVersionTransition_When_Archiving_Then_ReturnsVersionStatusConflict()
    {
        var repository = new FakeModelAssetRepository
        {
            LifecycleResult = new AssetVersionLifecycleRepositoryResult(
                true,
                true,
                VersionStatus.Draft,
                null,
                ErrorCode.VersionStatusInvalid,
                "invalid transition",
                Array.Empty<ApiErrorItem>())
        };
        var service = new AssetVersionLifecycleService(repository);

        var result = await service.ArchiveAsync(1, 2, new ChangeVersionStatusRequest(null), CancellationToken.None);

        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        Assert.Equal(ErrorCode.VersionStatusInvalid, result.Response.Code);
        Assert.Equal(AssetVersionLifecycleOperation.Archive, repository.LastLifecycleCommand!.Operation);
    }
}
