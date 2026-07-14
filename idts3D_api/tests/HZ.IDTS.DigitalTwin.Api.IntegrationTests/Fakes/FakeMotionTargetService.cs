using HZ.IDTS.DigitalTwin.Application.MotionTargets;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.MotionTargets;

namespace HZ.IDTS.DigitalTwin.Api.IntegrationTests.Fakes;

public sealed class FakeMotionTargetService : IMotionTargetService
{
    public GetMotionTargetsRequest? LastGetRequest { get; private set; }
    public CreateMotionTargetRequest? LastCreateRequest { get; private set; }
    public UpdateMotionTargetRequest? LastUpdateRequest { get; private set; }
    public long LastPartId { get; private set; }
    public long LastTargetId { get; private set; }
    public Exception? ExceptionToThrow { get; set; }
    public MotionTargetListResult ListResult { get; set; } = new(HttpStatusCode.OK, ApiResponse<MotionTargetListResponse>.Ok(new MotionTargetListResponse(101, new[] { Response })));
    public MotionTargetResult WriteResult { get; set; } = new(HttpStatusCode.OK, ApiResponse<MotionTargetResponse>.Ok(Response));
    public MotionTargetDeleteResult DeleteResult { get; set; } = new(HttpStatusCode.OK, ApiResponse<DeleteMotionTargetResponse>.Ok(new DeleteMotionTargetResponse(303, true)));

    public Task<MotionTargetListResult> GetMotionTargetsAsync(GetMotionTargetsRequest request, CancellationToken cancellationToken)
    {
        ThrowIfConfigured(); LastGetRequest = request; return Task.FromResult(ListResult);
    }

    public Task<MotionTargetResult> CreateAsync(long partId, CreateMotionTargetRequest request, CancellationToken cancellationToken)
    {
        ThrowIfConfigured(); LastPartId = partId; LastCreateRequest = request; return Task.FromResult(WriteResult);
    }

    public Task<MotionTargetResult> UpdateAsync(long partId, long targetId, UpdateMotionTargetRequest request, CancellationToken cancellationToken)
    {
        ThrowIfConfigured(); LastPartId = partId; LastTargetId = targetId; LastUpdateRequest = request; return Task.FromResult(WriteResult);
    }

    public Task<MotionTargetDeleteResult> DeleteAsync(long partId, long targetId, CancellationToken cancellationToken)
    {
        ThrowIfConfigured(); LastPartId = partId; LastTargetId = targetId; return Task.FromResult(DeleteResult);
    }

    private void ThrowIfConfigured()
    {
        if (ExceptionToThrow is not null) throw ExceptionToThrow;
    }

    private static MotionTargetResponse Response => new(303, 101, "F1", "Floor 1", 2, null, null, 2, 1, true);
}
