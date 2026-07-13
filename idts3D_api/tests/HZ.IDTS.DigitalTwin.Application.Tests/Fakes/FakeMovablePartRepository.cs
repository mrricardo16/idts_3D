using HZ.IDTS.DigitalTwin.Application.MovableParts;
using HZ.IDTS.DigitalTwin.Domain.Enums;

namespace HZ.IDTS.DigitalTwin.Application.Tests.Fakes;

internal sealed class FakeMovablePartRepository : IMovablePartRepository
{
    public MovablePartVersionLookup VersionLookup { get; set; } = new(true, true, VersionStatus.Draft);
    public MovablePartObjectResolution ObjectResolution { get; set; } = new(MovablePartObjectResolutionKind.Found, TestData.CanonicalObject);
    public IReadOnlyList<MovablePartData> Items { get; set; } = new[] { TestData.MovablePart };
    public MovablePartWriteResult? CreateResult { get; set; }
    public MovablePartWriteResult UpdateResult { get; set; } = new(MovablePartWriteFailure.None, TestData.MovablePart);
    public MovablePartWriteResult DeleteResult { get; set; } = new(MovablePartWriteFailure.None, TestData.MovablePart);
    public int CreateCallCount { get; private set; }
    public int UpdateCallCount { get; private set; }
    public int DeleteCallCount { get; private set; }
    public CreateMovablePartCommand? LastCreateCommand { get; private set; }
    public UpdateMovablePartCommand? LastUpdateCommand { get; private set; }

    public Task<MovablePartVersionLookup> GetVersionAsync(long assetId, long versionId, CancellationToken cancellationToken) => Task.FromResult(VersionLookup);
    public Task<MovablePartObjectResolution> ResolveCanonicalObjectAsync(long assetId, long versionId, string? objectUuid, string? objectPath, CancellationToken cancellationToken) => Task.FromResult(ObjectResolution);
    public Task<IReadOnlyList<MovablePartData>> GetListAsync(long assetId, long versionId, bool? enabled, CancellationToken cancellationToken) => Task.FromResult(Items);
    public Task<MovablePartWriteResult> CreateAsync(CreateMovablePartCommand command, CancellationToken cancellationToken)
    {
        CreateCallCount++;
        LastCreateCommand = command;
        return Task.FromResult(CreateResult ?? new MovablePartWriteResult(MovablePartWriteFailure.None, TestData.MovablePart with { PartCode = command.PartCode, BindingStatus = command.BindingStatus, Enabled = command.Enabled }));
    }

    public Task<MovablePartWriteResult> UpdateAsync(UpdateMovablePartCommand command, CancellationToken cancellationToken)
    {
        UpdateCallCount++;
        LastUpdateCommand = command;
        return Task.FromResult(UpdateResult);
    }

    public Task<MovablePartWriteResult> DeleteAsync(DeleteMovablePartCommand command, CancellationToken cancellationToken)
    {
        DeleteCallCount++;
        return Task.FromResult(DeleteResult);
    }
}

internal static class TestData
{
    public static readonly MovablePartCanonicalObject CanonicalObject = new("uuid-1", "platform", "/root/platform", "root", "/root");
    public static readonly MovablePartData MovablePart = new(30, 10, 20, "uuid-1", "platform", "/root/platform", "root", "/root", "Platform", "PLATFORM", MotionType.linear, AxisMode.world, Axis.z, null, null, null, 0, 10, 0, 0, 1, BindingStatus.active, true);
}
