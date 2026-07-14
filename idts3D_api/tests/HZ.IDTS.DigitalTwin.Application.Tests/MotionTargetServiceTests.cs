using HZ.IDTS.DigitalTwin.Application.MotionTargets;
using HZ.IDTS.DigitalTwin.Application.Tests.Fakes;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.MotionTargets;
using HZ.IDTS.DigitalTwin.Domain.Enums;
using System.Net;

namespace HZ.IDTS.DigitalTwin.Application.Tests;

public sealed class MotionTargetServiceTests
{
    [Fact]
    public async Task Given_EmptyModeAndPublishedPart_When_GettingTargets_Then_DefaultsToMonitorAndReturnsItems()
    {
        var repository = new FakeMotionTargetRepository { PartLookup = new(true, true, VersionStatus.Published, BindingStatus.active, MotionType.linear, AxisMode.world, Axis.z, 0, 10, true), Items = new[] { FakeMotionTargetRepository.Target } };

        var result = await new MotionTargetService(repository).GetMotionTargetsAsync(new GetMotionTargetsRequest(10, true, " "), CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Single(result.Response.Data!.Items);
    }

    [Theory]
    [InlineData(null, "targetCode")]
    [InlineData(" ", "targetCode")]
    public async Task Given_MissingTargetCode_When_CreatingTarget_Then_ReturnsValidationFailure(string? targetCode, string expectedField)
    {
        var result = await new MotionTargetService(new FakeMotionTargetRepository()).CreateAsync(10, ValidRequest() with { TargetCode = targetCode }, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal(ErrorCode.ValidationFailed, result.Response.Code);
        Assert.Contains(result.Response.Errors, x => x.Field == expectedField);
    }

    [Fact]
    public async Task Given_ValidTargetWithoutTargetZ_When_CreatingTarget_Then_NormalizesCodeAndPersistsTargetZFromTargetValue()
    {
        var repository = new FakeMotionTargetRepository();

        var result = await new MotionTargetService(repository).CreateAsync(10, ValidRequest() with { TargetCode = " f1 ", TargetZ = null }, CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.Equal(1, repository.CreateCallCount);
        Assert.Equal("F1", repository.LastCreateCommand!.TargetCode);
        Assert.Equal(repository.LastCreateCommand.TargetValue, repository.LastCreateCommand.TargetZ);
    }

    [Fact]
    public async Task Given_TargetValueOutsidePartRange_When_CreatingTarget_Then_ReturnsValidationFailure()
    {
        var result = await new MotionTargetService(new FakeMotionTargetRepository()).CreateAsync(10, ValidRequest() with { TargetValue = 11, TargetZ = 11 }, CancellationToken.None);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Contains(result.Response.Errors, x => x.Field == "targetValue");
    }

    [Theory]
    [InlineData(VersionStatus.Published)]
    [InlineData(VersionStatus.Archived)]
    [InlineData(VersionStatus.Failed)]
    [InlineData(VersionStatus.Invalid)]
    public async Task Given_NonWritableVersion_When_CreatingTarget_Then_ReturnsVersionStatusInvalid(VersionStatus status)
    {
        var repository = new FakeMotionTargetRepository { PartLookup = new(true, true, status, BindingStatus.active, MotionType.linear, AxisMode.world, Axis.z, 0, 10, true) };

        var result = await new MotionTargetService(repository).CreateAsync(10, ValidRequest(), CancellationToken.None);

        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        Assert.Equal(ErrorCode.VersionStatusInvalid, result.Response.Code);
    }

    [Fact]
    public async Task Given_DuplicateTargetCode_When_CreatingTarget_Then_ReturnsConflict()
    {
        var repository = new FakeMotionTargetRepository { CreateResult = new(MotionTargetWriteFailure.DuplicateTargetCode, null) };

        var result = await new MotionTargetService(repository).CreateAsync(10, ValidRequest(), CancellationToken.None);

        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
        Assert.Equal(ErrorCode.DuplicateTargetCode, result.Response.Code);
    }

    private static CreateMotionTargetRequest ValidRequest() => new("F1", "Floor 1", 2, null, null, 2, 1, true);
}
