using HZ.IDTS.DigitalTwin.Application.ModelAssets;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

namespace HZ.IDTS.DigitalTwin.Api.IntegrationTests.Fakes;

public sealed class FakeModelAssetUploadService : IModelAssetUploadService
{
    public int CallCount { get; private set; }

    public UploadModelAssetRequest? LastRequest { get; private set; }

    public string? LastFileName { get; private set; }

    public long LastFileLength { get; private set; }

    public byte[] LastFileContent { get; private set; } = Array.Empty<byte>();

    public Exception? ExceptionToThrow { get; set; }

    public UploadModelAssetResult Result { get; set; } = new(
        HttpStatusCode.OK,
        ApiResponse<UploadModelAssetResponse>.Ok(
            new UploadModelAssetResponse(101, 202, 303, "TEST-GLB", "Draft", "pending", "test-hash", "/assets/models/101/202/source.glb")));

    public async Task<UploadModelAssetResult> UploadAsync(
        UploadModelAssetRequest request,
        Stream fileContent,
        string fileName,
        long fileLength,
        CancellationToken cancellationToken)
    {
        CallCount++;
        LastRequest = request;
        LastFileName = fileName;
        LastFileLength = fileLength;

        await using var buffer = new MemoryStream();
        await fileContent.CopyToAsync(buffer, cancellationToken);
        LastFileContent = buffer.ToArray();

        if (ExceptionToThrow is not null)
        {
            throw ExceptionToThrow;
        }

        return Result;
    }
}
