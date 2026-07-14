using HZ.IDTS.DigitalTwin.Contracts.MotionTargets;

namespace HZ.IDTS.DigitalTwin.Application.MotionTargets;

public interface IMotionTargetService
{
    Task<MotionTargetListResult> GetMotionTargetsAsync(GetMotionTargetsRequest request, CancellationToken cancellationToken);
    Task<MotionTargetResult> CreateAsync(long partId, CreateMotionTargetRequest request, CancellationToken cancellationToken);
    Task<MotionTargetResult> UpdateAsync(long partId, long targetId, UpdateMotionTargetRequest request, CancellationToken cancellationToken);
    Task<MotionTargetDeleteResult> DeleteAsync(long partId, long targetId, CancellationToken cancellationToken);
}
