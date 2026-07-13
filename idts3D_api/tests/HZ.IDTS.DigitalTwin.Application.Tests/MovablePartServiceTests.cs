using System.Net;
using HZ.IDTS.DigitalTwin.Application.MovableParts;
using HZ.IDTS.DigitalTwin.Application.Tests.Fakes;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.MovableParts;
using HZ.IDTS.DigitalTwin.Domain.Enums;

namespace HZ.IDTS.DigitalTwin.Application.Tests;

public sealed class MovablePartServiceTests
{
    [Theory]
    [InlineData(0, 20)]
    [InlineData(10, 0)]
    public async Task Get_WhenRouteIdIsInvalid_ReturnsValidationFailure(long assetId, long versionId)
    {
        var result = await new MovablePartService(new FakeMovablePartRepository()).GetMovablePartsAsync(new GetMovablePartsRequest(assetId, versionId, null, null), CancellationToken.None);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(ErrorCode.ValidationFailed, result.Response.Code);
    }

    [Fact]
    public async Task Get_WhenModeIsInvalid_ReturnsValidationFailure()
    {
        var result = await new MovablePartService(new FakeMovablePartRepository()).GetMovablePartsAsync(new GetMovablePartsRequest(10, 20, null, "preview"), CancellationToken.None);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(ErrorCode.ValidationFailed, result.Response.Code);
    }

    [Fact]
    public async Task Get_WhenMonitorReadsDraft_ReturnsVersionStatusConflict()
    {
        var repository = new FakeMovablePartRepository { VersionLookup = new(true, true, VersionStatus.Draft) };
        var result = await new MovablePartService(repository).GetMovablePartsAsync(new GetMovablePartsRequest(10, 20, false, null), CancellationToken.None);
        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        Assert.Equal(ErrorCode.VersionStatusInvalid, result.Response.Code);
    }

    [Fact]
    public async Task Get_WhenEditReadsDraft_AppliesEnabledFilter()
    {
        var repository = new FakeMovablePartRepository { VersionLookup = new(true, true, VersionStatus.Draft) };
        var result = await new MovablePartService(repository).GetMovablePartsAsync(new GetMovablePartsRequest(10, 20, false, "edit"), CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Single(result.Response.Data!.Items);
    }

    [Fact]
    public async Task Create_WhenObjectIdentifiersConflict_ReturnsValidationFailureWithoutWrite()
    {
        var repository = new FakeMovablePartRepository { ObjectResolution = new(MovablePartObjectResolutionKind.Conflict, null) };
        var result = await new MovablePartService(repository).CreateAsync(10, 20, ValidRequest(), CancellationToken.None);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(ErrorCode.ValidationFailed, result.Response.Code);
        Assert.Equal(0, repository.CreateCallCount);
    }

    [Fact]
    public async Task Create_WhenObjectIdentifiersAreMissing_ReturnsValidationFailureWithoutWrite()
    {
        var repository = new FakeMovablePartRepository();
        var request = ValidRequest() with { ObjectUuid = null, ObjectPath = null };
        var result = await new MovablePartService(repository).CreateAsync(10, 20, request, CancellationToken.None);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(0, repository.CreateCallCount);
    }

    [Fact]
    public async Task Create_WhenEnabledIsMissingOrCustomAxisIsProvided_ReturnsValidationFailure()
    {
        var withoutEnabled = await new MovablePartService(new FakeMovablePartRepository()).CreateAsync(10, 20, ValidRequest(null), CancellationToken.None);
        var customAxis = await new MovablePartService(new FakeMovablePartRepository()).CreateAsync(10, 20, ValidRequest() with { CustomAxisX = 1 }, CancellationToken.None);
        Assert.Equal(ErrorCode.ValidationFailed, withoutEnabled.Response.Code);
        Assert.Equal(ErrorCode.ValidationFailed, customAxis.Response.Code);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    public async Task Create_WhenAssetOrVersionIsUnavailable_ReturnsNotFound(bool assetExists, bool versionExists)
    {
        var repository = new FakeMovablePartRepository { VersionLookup = new(assetExists, versionExists, null) };
        var result = await new MovablePartService(repository).CreateAsync(10, 20, ValidRequest(), CancellationToken.None);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(0, repository.CreateCallCount);
    }

    [Fact]
    public async Task Create_WhenVersionIsPublished_ReturnsStatusConflictWithoutWrite()
    {
        var repository = new FakeMovablePartRepository { VersionLookup = new(true, true, VersionStatus.Published) };
        var result = await new MovablePartService(repository).CreateAsync(10, 20, ValidRequest(), CancellationToken.None);
        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        Assert.Equal(ErrorCode.VersionStatusInvalid, result.Response.Code);
        Assert.Equal(0, repository.CreateCallCount);
    }

    [Fact]
    public async Task Create_WhenObjectDoesNotExist_ReturnsNotFoundWithoutInvalidBinding()
    {
        var repository = new FakeMovablePartRepository { ObjectResolution = new(MovablePartObjectResolutionKind.NotFound, null) };
        var result = await new MovablePartService(repository).CreateAsync(10, 20, ValidRequest(), CancellationToken.None);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(ErrorCode.NotFound, result.Response.Code);
        Assert.Equal(0, repository.CreateCallCount);
    }

    [Theory]
    [InlineData(null, "CODE", "linear", "world", "z", 0, 10, 0, 0, 1, true)]
    [InlineData("Name", null, "linear", "world", "z", 0, 10, 0, 0, 1, true)]
    [InlineData("Name", "CODE", "rotate", "world", "z", 0, 10, 0, 0, 1, true)]
    [InlineData("Name", "CODE", "linear", "local", "z", 0, 10, 0, 0, 1, true)]
    [InlineData("Name", "CODE", "linear", "world", "x", 0, 10, 0, 0, 1, true)]
    [InlineData("Name", "CODE", "linear", "world", "z", 10, 0, 5, 5, 1, true)]
    [InlineData("Name", "CODE", "linear", "world", "z", 0, 10, 5, 11, 1, true)]
    [InlineData("Name", "CODE", "linear", "world", "z", 0, 10, 5, 5, 0, true)]
    public async Task Create_WhenBusinessFieldsAreInvalid_ReturnsValidationFailure(string? businessName, string? partCode, string motionType, string axisMode, string axis, decimal min, decimal max, decimal home, decimal current, decimal speed, bool enabled)
    {
        var request = new CreateMovablePartRequest("uuid-1", "/root/platform", businessName, partCode, motionType, axisMode, axis, null, null, null, min, max, home, current, speed, enabled);
        var result = await new MovablePartService(new FakeMovablePartRepository()).CreateAsync(10, 20, request, CancellationToken.None);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(ErrorCode.ValidationFailed, result.Response.Code);
    }

    [Fact]
    public async Task Create_WhenCanonicalObjectIsValid_NormalizesCodeAndWritesActiveBinding()
    {
        var repository = new FakeMovablePartRepository();
        var result = await new MovablePartService(repository).CreateAsync(10, 20, ValidRequest(enabled: false), CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("active", result.Response.Data!.BindingStatus);
        Assert.False(result.Response.Data.Enabled);
        Assert.Equal(BindingStatus.active, repository.LastCreateCommand!.BindingStatus);
        Assert.Equal("PLATFORM", repository.LastCreateCommand.PartCode);
        Assert.False(repository.LastCreateCommand.Enabled);
    }

    [Fact]
    public async Task Update_WhenCanonicalObjectIsValid_RestoresActiveBinding()
    {
        var repository = new FakeMovablePartRepository { UpdateResult = new(MovablePartWriteFailure.None, TestData.MovablePart with { BindingStatus = BindingStatus.active }) };
        var request = ValidRequest();
        var result = await new MovablePartService(repository).UpdateAsync(10, 20, 30, new UpdateMovablePartRequest(request.ObjectUuid, request.ObjectPath, request.BusinessName, request.PartCode, request.MotionType, request.AxisMode, request.Axis, request.CustomAxisX, request.CustomAxisY, request.CustomAxisZ, request.MinValue, request.MaxValue, request.HomeValue, request.CurrentValue, request.DefaultSpeed, request.Enabled), CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal("active", result.Response.Data!.BindingStatus);
        Assert.Equal(BindingStatus.active, repository.LastUpdateCommand!.BindingStatus);
    }

    [Fact]
    public async Task Create_WhenPartCodeIsDuplicate_ReturnsConflict()
    {
        var repository = new FakeMovablePartRepository { CreateResult = new(MovablePartWriteFailure.DuplicatePartCode, null) };
        var result = await new MovablePartService(repository).CreateAsync(10, 20, ValidRequest(), CancellationToken.None);
        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        Assert.Equal(ErrorCode.DuplicatePartCode, result.Response.Code);
    }

    [Fact]
    public async Task Delete_WhenPartIsMissing_ReturnsNotFound()
    {
        var repository = new FakeMovablePartRepository { DeleteResult = new(MovablePartWriteFailure.PartNotFound, null) };
        var result = await new MovablePartService(repository).DeleteAsync(10, 20, 30, CancellationToken.None);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(ErrorCode.NotFound, result.Response.Code);
    }

    [Fact]
    public async Task Delete_WhenWritablePartExists_DeletesIt()
    {
        var repository = new FakeMovablePartRepository();
        var result = await new MovablePartService(repository).DeleteAsync(10, 20, 30, CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(result.Response.Data!.Deleted);
        Assert.Equal(1, repository.DeleteCallCount);
    }

    private static CreateMovablePartRequest ValidRequest(bool? enabled = true) =>
        new(" uuid-1 ", " /root/platform ", " Platform ", " platform ", "linear", "world", "z", null, null, null, 0, 10, 0, 0, 1, enabled);
}
