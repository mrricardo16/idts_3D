using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

namespace HZ.IDTS.DigitalTwin.Application.ModelAssets;

public interface IModelAssetUploadService
{
    Task<UploadModelAssetResult> UploadAsync(
        UploadModelAssetRequest request,
        Stream fileContent,
        string fileName,
        long fileLength,
        CancellationToken cancellationToken);
}
