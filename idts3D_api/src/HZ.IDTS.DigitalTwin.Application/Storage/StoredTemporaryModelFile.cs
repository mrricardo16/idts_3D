namespace HZ.IDTS.DigitalTwin.Application.Storage;

public sealed record StoredTemporaryModelFile(
    string PhysicalPath,
    string SourceFileName,
    string SourceFileHash,
    long FileSize);
