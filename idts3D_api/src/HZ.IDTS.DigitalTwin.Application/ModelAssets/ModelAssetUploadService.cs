using System.Net;
using HZ.IDTS.DigitalTwin.Application.Storage;
using HZ.IDTS.DigitalTwin.Contracts.Common;
using HZ.IDTS.DigitalTwin.Contracts.ModelAssets;
using HZ.IDTS.DigitalTwin.Domain.Enums;

namespace HZ.IDTS.DigitalTwin.Application.ModelAssets;

public sealed class ModelAssetUploadService : IModelAssetUploadService
{
    private readonly IModelAssetRepository _repository;
    private readonly IModelAssetFileStorage _fileStorage;

    public ModelAssetUploadService(
        IModelAssetRepository repository,
        IModelAssetFileStorage fileStorage)
    {
        _repository = repository;
        _fileStorage = fileStorage;
    }

    public async Task<UploadModelAssetResult> UploadAsync(
        UploadModelAssetRequest request,
        Stream fileContent,
        string fileName,
        long fileLength,
        CancellationToken cancellationToken)
    {
        var validationErrors = ValidateRequest(request, fileName, fileLength);
        if (validationErrors.Count > 0)
        {
            return Failure(
                HttpStatusCode.BadRequest,
                ErrorCode.ValidationFailed,
                "上传参数无效。",
                validationErrors);
        }

        if (!_fileStorage.IsAllowedSourceFile(fileName))
        {
            return Failure(
                HttpStatusCode.BadRequest,
                ErrorCode.FileTypeNotAllowed,
                "MVP 阶段只允许上传 GLB 文件。",
                new[] { new ApiErrorItem("file", "文件扩展名必须是 .glb。") });
        }

        if (fileLength > _fileStorage.MaxFileSizeBytes)
        {
            return Failure(
                HttpStatusCode.BadRequest,
                ErrorCode.FileTooLarge,
                "上传文件超过大小限制。",
                new[] { new ApiErrorItem("file", $"文件大小不能超过 {_fileStorage.MaxFileSizeBytes} bytes。") });
        }

        var assetType = Enum.Parse<AssetType>(request.AssetType, ignoreCase: true);
        var sourceFileType = Enum.Parse<SourceFileType>(request.SourceFileType, ignoreCase: true);
        var assetCode = request.AssetCode.Trim();
        var assetName = request.AssetName.Trim();

        if (await _repository.AssetCodeExistsAsync(assetCode, cancellationToken))
        {
            return Failure(
                HttpStatusCode.Conflict,
                ErrorCode.Conflict,
                "assetCode 已存在。",
                new[] { new ApiErrorItem("assetCode", $"{assetCode} 已存在。") });
        }

        StoredTemporaryModelFile? temporaryFile = null;
        try
        {
            temporaryFile = await _fileStorage.SaveTemporarySourceFileAsync(
                fileContent,
                fileName,
                cancellationToken);

            if (await _repository.SourceFileHashExistsAsync(temporaryFile.SourceFileHash, cancellationToken))
            {
                return Failure(
                    HttpStatusCode.Conflict,
                    ErrorCode.FileHashExists,
                    "sourceFileHash 已存在。",
                    new[] { new ApiErrorItem("file", "相同 SHA256 的 GLB 文件已存在。") });
            }

            var command = new CreateModelAssetUploadCommand(
                assetCode,
                assetName,
                temporaryFile.SourceFileName,
                temporaryFile.SourceFileHash,
                temporaryFile.FileSize,
                assetType,
                sourceFileType);

            var response = await _repository.CreateUploadAsync(
                command,
                (assetId, versionId, token) => _fileStorage.MoveTemporaryToSourceAsync(
                    temporaryFile,
                    assetId,
                    versionId,
                    token),
                (assetId, versionId, token) => _fileStorage.DeleteSourceFileAsync(
                    assetId,
                    versionId,
                    token),
                cancellationToken);

            temporaryFile = null;

            return new UploadModelAssetResult(
                HttpStatusCode.OK,
                ApiResponse<UploadModelAssetResponse>.Ok(response));
        }
        finally
        {
            if (temporaryFile is not null)
            {
                await _fileStorage.DeleteTemporaryFileAsync(temporaryFile, cancellationToken);
            }
        }
    }

    private List<ApiErrorItem> ValidateRequest(
        UploadModelAssetRequest request,
        string fileName,
        long fileLength)
    {
        var errors = new List<ApiErrorItem>();

        if (string.IsNullOrWhiteSpace(fileName) || fileLength <= 0)
        {
            errors.Add(new ApiErrorItem("file", "文件不能为空。"));
        }

        if (string.IsNullOrWhiteSpace(request.AssetCode))
        {
            errors.Add(new ApiErrorItem("assetCode", "assetCode 不能为空。"));
        }

        if (string.IsNullOrWhiteSpace(request.AssetName))
        {
            errors.Add(new ApiErrorItem("assetName", "assetName 不能为空。"));
        }

        if (!Enum.TryParse<AssetType>(request.AssetType, ignoreCase: true, out _))
        {
            errors.Add(new ApiErrorItem("assetType", "assetType 只允许 device_glb 或 static_glb。"));
        }

        if (!Enum.TryParse<SourceFileType>(request.SourceFileType, ignoreCase: true, out var sourceFileType) ||
            sourceFileType != SourceFileType.glb)
        {
            errors.Add(new ApiErrorItem("sourceFileType", "sourceFileType 只允许 glb。"));
        }

        return errors;
    }

    private static UploadModelAssetResult Failure(
        HttpStatusCode statusCode,
        string code,
        string message,
        IReadOnlyList<ApiErrorItem> errors)
    {
        return new UploadModelAssetResult(
            statusCode,
            ApiResponse<UploadModelAssetResponse>.Failure(code, message, errors));
    }
}
