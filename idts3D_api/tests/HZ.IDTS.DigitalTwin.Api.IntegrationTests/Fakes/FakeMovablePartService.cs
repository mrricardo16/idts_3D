using HZ.IDTS.DigitalTwin.Application.MovableParts;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.MovableParts;

namespace HZ.IDTS.DigitalTwin.Api.IntegrationTests.Fakes;

public sealed class FakeMovablePartService : IMovablePartService
{
    public GetMovablePartsRequest? LastGetRequest { get; private set; }
    public CreateMovablePartRequest? LastCreateRequest { get; private set; }
    public UpdateMovablePartRequest? LastUpdateRequest { get; private set; }
    public long LastPartId { get; private set; }
    public Exception? ExceptionToThrow { get; set; }
    public MovablePartListResult ListResult { get; set; } = new(HttpStatusCode.OK, ApiResponse<MovablePartListResponse>.Ok(new MovablePartListResponse(new[] { Response } )));
    public MovablePartResult WriteResult { get; set; } = new(HttpStatusCode.OK, ApiResponse<MovablePartResponse>.Ok(Response));
    public MovablePartDeleteResult DeleteResult { get; set; } = new(HttpStatusCode.OK, ApiResponse<DeleteMovablePartResponse>.Ok(new DeleteMovablePartResponse(303, true)));

    public Task<MovablePartListResult> GetMovablePartsAsync(GetMovablePartsRequest request, CancellationToken cancellationToken)
    {
        ThrowIfConfigured(); LastGetRequest = request; return Task.FromResult(ListResult);
    }
    public Task<MovablePartResult> CreateAsync(long assetId, long versionId, CreateMovablePartRequest request, CancellationToken cancellationToken)
    {
        ThrowIfConfigured(); LastCreateRequest = request; return Task.FromResult(WriteResult);
    }
    public Task<MovablePartResult> UpdateAsync(long assetId, long versionId, long partId, UpdateMovablePartRequest request, CancellationToken cancellationToken)
    {
        ThrowIfConfigured(); LastPartId = partId; LastUpdateRequest = request; return Task.FromResult(WriteResult);
    }
    public Task<MovablePartDeleteResult> DeleteAsync(long assetId, long versionId, long partId, CancellationToken cancellationToken)
    {
        ThrowIfConfigured(); LastPartId = partId; return Task.FromResult(DeleteResult);
    }
    private void ThrowIfConfigured() { if (ExceptionToThrow is not null) throw ExceptionToThrow; }
    private static MovablePartResponse Response => new(303, 101, 202, "uuid-1", "platform", "/root/platform", "root", "/root", "Platform", "PLATFORM", "linear", "world", "z", null, null, null, 0, 10, 0, 0, 1, "active", true);
}
