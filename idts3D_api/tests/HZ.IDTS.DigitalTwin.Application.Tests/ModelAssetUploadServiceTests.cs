using System.Net;
using HZ.IDTS.DigitalTwin.Application.ModelAssets;
using HZ.IDTS.DigitalTwin.Application.Tests.Fakes;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

namespace HZ.IDTS.DigitalTwin.Application.Tests;

public sealed class ModelAssetUploadServiceTests
{
    [Fact]
    public async Task Given_EmptyAssetCode_When_Uploading_Then_ReturnsValidationFailureWithoutStorageAccess()
    {
        var repository = new FakeModelAssetRepository();
        var storage = new FakeModelAssetFileStorage();
        var service = new ModelAssetUploadService(repository, storage);

        var result = await service.UploadAsync(Request(""), Content(), "model.glb", 3, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.False(result.Response.Success);
        Assert.Equal(ErrorCode.ValidationFailed, result.Response.Code);
        Assert.Contains(result.Response.Errors, error => error.Field == "assetCode");
        Assert.Equal(0, storage.IsAllowedSourceFileCallCount);
        Assert.Equal(0, repository.AssetCodeExistsCallCount);
    }

    [Fact]
    public async Task Given_NonGlbFile_When_Uploading_Then_ReturnsFileTypeFailure()
    {
        var repository = new FakeModelAssetRepository();
        var storage = new FakeModelAssetFileStorage { IsAllowedSourceFileResult = false };
        var service = new ModelAssetUploadService(repository, storage);

        var result = await service.UploadAsync(Request(), Content(), "model.obj", 3, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(ErrorCode.FileTypeNotAllowed, result.Response.Code);
        Assert.Equal(1, storage.IsAllowedSourceFileCallCount);
        Assert.Equal(0, storage.SaveTemporaryCallCount);
    }

    [Fact]
    public async Task Given_ExistingAssetCode_When_Uploading_Then_ReturnsConflictWithoutSavingTemporaryFile()
    {
        var repository = new FakeModelAssetRepository { AssetCodeExistsResult = true };
        var storage = new FakeModelAssetFileStorage();
        var service = new ModelAssetUploadService(repository, storage);

        var result = await service.UploadAsync(Request(), Content(), "model.glb", 3, CancellationToken.None);

        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        Assert.Equal(ErrorCode.Conflict, result.Response.Code);
        Assert.Equal(1, repository.AssetCodeExistsCallCount);
        Assert.Equal(0, storage.SaveTemporaryCallCount);
    }

    [Fact]
    public async Task Given_DuplicateSourceHash_When_Uploading_Then_DeletesTemporaryFileAndReturnsConflict()
    {
        var repository = new FakeModelAssetRepository { SourceFileHashExistsResult = true };
        var storage = new FakeModelAssetFileStorage();
        var service = new ModelAssetUploadService(repository, storage);

        var result = await service.UploadAsync(Request(), Content(), "model.glb", 3, CancellationToken.None);

        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        Assert.Equal(ErrorCode.FileHashExists, result.Response.Code);
        Assert.Equal(1, storage.SaveTemporaryCallCount);
        Assert.Equal(1, repository.SourceFileHashExistsCallCount);
        Assert.Equal(1, storage.DeleteTemporaryCallCount);
        Assert.Equal(storage.TemporaryFile, storage.DeletedTemporaryFile);
    }

    [Fact]
    public async Task Given_ValidUpload_When_RepositoryCreatesAsset_Then_PersistsSourceAndReturnsCreatedAsset()
    {
        var repository = new FakeModelAssetRepository();
        var storage = new FakeModelAssetFileStorage();
        var service = new ModelAssetUploadService(repository, storage);

        var result = await service.UploadAsync(Request(" asset-01 "), Content(), "model.glb", 3, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(result.Response.Success);
        Assert.Equal(ErrorCode.Ok, result.Response.Code);
        Assert.Equal(1, repository.CreateUploadCallCount);
        Assert.Equal("asset-01", repository.LastCreateUploadCommand!.AssetCode);
        Assert.Equal(1, storage.MoveTemporaryCallCount);
        Assert.Equal((10L, 20L), (storage.LastMoveRequest!.Value.AssetId, storage.LastMoveRequest.Value.VersionId));
        Assert.Equal(0, storage.DeleteTemporaryCallCount);
    }

    [Fact]
    public async Task Given_RepositoryCreationThrowsAfterTemporarySave_When_Uploading_Then_DeletesTemporaryFile()
    {
        var repository = new FakeModelAssetRepository
        {
            CreateUploadHandler = (_, _, _, _) => throw new InvalidOperationException("repository failed")
        };
        var storage = new FakeModelAssetFileStorage();
        var service = new ModelAssetUploadService(repository, storage);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadAsync(Request(), Content(), "model.glb", 3, CancellationToken.None));

        Assert.Equal("repository failed", exception.Message);
        Assert.Equal(1, storage.SaveTemporaryCallCount);
        Assert.Equal(1, repository.CreateUploadCallCount);
        Assert.Equal(1, storage.DeleteTemporaryCallCount);
    }

    private static UploadModelAssetRequest Request(string assetCode = "asset-01") =>
        new(assetCode, "Asset", "device_glb", "glb");

    private static MemoryStream Content() => new(new byte[] { 1, 2, 3 });
}
