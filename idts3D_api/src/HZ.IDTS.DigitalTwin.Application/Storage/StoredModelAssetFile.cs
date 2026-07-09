namespace HZ.IDTS.DigitalTwin.Application.Storage;

public sealed record StoredModelAssetFile(
    string PhysicalPath,
    string FileUrl,
    string SourceFileName,
    string SourceFileHash,
    long FileSize);
