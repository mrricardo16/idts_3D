using HZ.IDTS.DigitalTwin.Application.MotionTargets;
using HZ.IDTS.DigitalTwin.Domain.Enums;

namespace HZ.IDTS.DigitalTwin.Application.Tests.Fakes;

internal sealed class FakeMotionTargetRepository : IMotionTargetRepository
{
    public MotionTargetPartLookup PartLookup { get; set; } = new(true, true, VersionStatus.Draft, BindingStatus.active, MotionType.linear, AxisMode.world, Axis.z, 0, 10, true);
    public IReadOnlyList<MotionTargetData> Items { get; set; } = Array.Empty<MotionTargetData>();
    public MotionTargetWriteResult CreateResult { get; set; } = new(MotionTargetWriteFailure.None, Target);
    public MotionTargetWriteResult UpdateResult { get; set; } = new(MotionTargetWriteFailure.None, Target);
    public MotionTargetWriteResult DeleteResult { get; set; } = new(MotionTargetWriteFailure.None, Target);
    public int CreateCallCount { get; private set; }
    public CreateMotionTargetCommand? LastCreateCommand { get; private set; }

    public Task<MotionTargetPartLookup> GetPartAsync(long partId, CancellationToken cancellationToken) => Task.FromResult(PartLookup);
    public Task<IReadOnlyList<MotionTargetData>> GetListAsync(long partId, bool? enabled, CancellationToken cancellationToken) => Task.FromResult(Items);
    public Task<MotionTargetWriteResult> CreateAsync(CreateMotionTargetCommand command, CancellationToken cancellationToken)
    {
        CreateCallCount++;
        LastCreateCommand = command;
        return Task.FromResult(CreateResult);
    }
    public Task<MotionTargetWriteResult> UpdateAsync(UpdateMotionTargetCommand command, CancellationToken cancellationToken) => Task.FromResult(UpdateResult);
    public Task<MotionTargetWriteResult> DeleteAsync(DeleteMotionTargetCommand command, CancellationToken cancellationToken) => Task.FromResult(DeleteResult);

    public static MotionTargetData Target { get; } = new(30, 10, "F1", "Floor 1", 2, null, null, 2, 1, true);
}
