namespace HZ.IDTS.DigitalTwin.Application.MotionTargets;

public interface IMotionTargetRepository
{
    Task<MotionTargetPartLookup> GetPartAsync(long partId, CancellationToken cancellationToken);
    Task<IReadOnlyList<MotionTargetData>> GetListAsync(long partId, bool? enabled, CancellationToken cancellationToken);
    Task<MotionTargetWriteResult> CreateAsync(CreateMotionTargetCommand command, CancellationToken cancellationToken);
    Task<MotionTargetWriteResult> UpdateAsync(UpdateMotionTargetCommand command, CancellationToken cancellationToken);
    Task<MotionTargetWriteResult> DeleteAsync(DeleteMotionTargetCommand command, CancellationToken cancellationToken);
}
