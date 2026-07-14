namespace HZ.IDTS.DigitalTwin.Contracts.Common;

public static class ErrorCode
{
    public const string Ok = "OK";
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string FileTypeNotAllowed = "FILE_TYPE_NOT_ALLOWED";
    public const string FileTooLarge = "FILE_TOO_LARGE";
    public const string FileHashExists = "FILE_HASH_EXISTS";
    public const string NotFound = "NOT_FOUND";
    public const string AssetNotFound = "ASSET_NOT_FOUND";
    public const string VersionNotFound = "VERSION_NOT_FOUND";
    public const string VersionStatusInvalid = "VERSION_STATUS_INVALID";
    public const string ManifestRequired = "MANIFEST_REQUIRED";
    public const string ObjectTreeRequired = "OBJECT_TREE_REQUIRED";
    public const string ModelStatsRequired = "MODEL_STATS_REQUIRED";
    public const string Conflict = "CONFLICT";
    public const string DuplicatePartCode = "DUPLICATE_PART_CODE";
    public const string MovablePartNotFound = "MOVABLE_PART_NOT_FOUND";
    public const string DuplicateTargetCode = "DUPLICATE_TARGET_CODE";
    public const string InternalError = "INTERNAL_ERROR";
}
