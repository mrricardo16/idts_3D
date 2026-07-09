using HZ.IDTS.DigitalTwin.Domain.Enums;

namespace HZ.IDTS.DigitalTwin.Application.ModelAssets;

public sealed record CreateModelAssetUploadCommand(
    string AssetCode,
    string AssetName,
    string SourceFileName,
    string SourceFileHash,
    long FileSize,
    AssetType AssetType,
    SourceFileType SourceFileType);
