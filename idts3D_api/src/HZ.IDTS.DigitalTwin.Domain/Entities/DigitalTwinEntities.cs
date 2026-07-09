using HZ.IDTS.DigitalTwin.Domain.Enums;

namespace HZ.IDTS.DigitalTwin.Domain.Entities;

public sealed class ModelAsset
{
    public long Id { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string SourceFileName { get; set; } = string.Empty;
    public string SourceFileHash { get; set; } = string.Empty;
    public SourceFileType SourceFileType { get; set; } = SourceFileType.glb;
    public AssetType AssetType { get; set; } = AssetType.device_glb;
    public ProcessingStatus ProcessingStatus { get; set; } = ProcessingStatus.pending;
    public long? CurrentVersionId { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime UpdatedTime { get; set; }

    public AssetVersion? CurrentVersion { get; set; }
    public ICollection<AssetVersion> Versions { get; set; } = new List<AssetVersion>();
    public ICollection<ModelAssetVariant> Variants { get; set; } = new List<ModelAssetVariant>();
    public ICollection<ModelConversionJob> ConversionJobs { get; set; } = new List<ModelConversionJob>();
    public ICollection<ModelObjectIndex> ObjectIndexes { get; set; } = new List<ModelObjectIndex>();
    public ICollection<AssetManifest> Manifests { get; set; } = new List<AssetManifest>();
    public ICollection<DeviceModelBinding> DeviceModelBindings { get; set; } = new List<DeviceModelBinding>();
    public ICollection<MovablePartBinding> MovablePartBindings { get; set; } = new List<MovablePartBinding>();
}

public sealed class AssetVersion
{
    public long Id { get; set; }
    public long ModelAssetId { get; set; }
    public int VersionNo { get; set; } = 1;
    public VersionStatus VersionStatus { get; set; } = VersionStatus.Draft;
    public DateTime CreatedTime { get; set; }
    public DateTime? PublishedTime { get; set; }
    public DateTime? ArchivedTime { get; set; }

    public ModelAsset ModelAsset { get; set; } = null!;
    public ICollection<ModelAsset> CurrentForAssets { get; set; } = new List<ModelAsset>();
    public ICollection<ModelAssetVariant> Variants { get; set; } = new List<ModelAssetVariant>();
    public ICollection<ModelConversionJob> ConversionJobs { get; set; } = new List<ModelConversionJob>();
    public ICollection<ModelObjectIndex> ObjectIndexes { get; set; } = new List<ModelObjectIndex>();
    public AssetManifest? Manifest { get; set; }
    public ICollection<DeviceModelBinding> DeviceModelBindings { get; set; } = new List<DeviceModelBinding>();
    public ICollection<MovablePartBinding> MovablePartBindings { get; set; } = new List<MovablePartBinding>();
}

public sealed class ModelAssetVariant
{
    public long Id { get; set; }
    public long ModelAssetId { get; set; }
    public long AssetVersionId { get; set; }
    public VariantLevel VariantLevel { get; set; } = VariantLevel.source;
    public string FileUrl { get; set; } = string.Empty;
    public string FileHash { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public long? TriangleCount { get; set; }
    public long? VertexCount { get; set; }
    public int? MeshCount { get; set; }
    public int? MaterialCount { get; set; }
    public int? TextureCount { get; set; }
    public DateTime CreatedTime { get; set; }

    public ModelAsset ModelAsset { get; set; } = null!;
    public AssetVersion AssetVersion { get; set; } = null!;
}

public sealed class ModelConversionJob
{
    public long Id { get; set; }
    public long ModelAssetId { get; set; }
    public long AssetVersionId { get; set; }
    public ConversionJobType JobType { get; set; } = ConversionJobType.upload;
    public ConversionJobStatus Status { get; set; } = ConversionJobStatus.pending;
    public int Progress { get; set; }
    public string? Message { get; set; }
    public string? InputFile { get; set; }
    public string? OutputDirectory { get; set; }
    public string? StdoutLogUrl { get; set; }
    public string? StderrLogUrl { get; set; }
    public int? ExitCode { get; set; }
    public long? ElapsedMs { get; set; }
    public int RetryCount { get; set; }
    public DateTime? StartedTime { get; set; }
    public DateTime? FinishedTime { get; set; }

    public ModelAsset ModelAsset { get; set; } = null!;
    public AssetVersion AssetVersion { get; set; } = null!;
}

public sealed class ModelObjectIndex
{
    public long Id { get; set; }
    public long ModelAssetId { get; set; }
    public long AssetVersionId { get; set; }
    public string ObjectUuid { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string ObjectPath { get; set; } = string.Empty;
    public string? ParentUuid { get; set; }
    public string? ParentPath { get; set; }
    public string ObjectType { get; set; } = string.Empty;
    public decimal? BoundingBoxMinX { get; set; }
    public decimal? BoundingBoxMinY { get; set; }
    public decimal? BoundingBoxMinZ { get; set; }
    public decimal? BoundingBoxMaxX { get; set; }
    public decimal? BoundingBoxMaxY { get; set; }
    public decimal? BoundingBoxMaxZ { get; set; }
    public string? MeshFingerprint { get; set; }

    public ModelAsset ModelAsset { get; set; } = null!;
    public AssetVersion AssetVersion { get; set; } = null!;
}

public sealed class AssetManifest
{
    public long Id { get; set; }
    public long ModelAssetId { get; set; }
    public long AssetVersionId { get; set; }
    public string ManifestJson { get; set; } = "{}";
    public string? ModelStatsJson { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime UpdatedTime { get; set; }

    public ModelAsset ModelAsset { get; set; } = null!;
    public AssetVersion AssetVersion { get; set; } = null!;
}

public sealed class SceneNode
{
    public long Id { get; set; }
    public long? ParentId { get; set; }
    public string NodeCode { get; set; } = string.Empty;
    public string NodeName { get; set; } = string.Empty;
    public SceneNodeType NodeType { get; set; } = SceneNodeType.scene;
    public int SortNo { get; set; }
    public bool Enabled { get; set; } = true;

    public SceneNode? Parent { get; set; }
    public ICollection<SceneNode> Children { get; set; } = new List<SceneNode>();
    public ICollection<DeviceInstance> DeviceInstances { get; set; } = new List<DeviceInstance>();
}

public sealed class DeviceInstance
{
    public long Id { get; set; }
    public long SceneNodeId { get; set; }
    public string DeviceCode { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public decimal PositionX { get; set; }
    public decimal PositionY { get; set; }
    public decimal PositionZ { get; set; }
    public decimal RotationX { get; set; }
    public decimal RotationY { get; set; }
    public decimal RotationZ { get; set; }
    public decimal ScaleX { get; set; } = 1;
    public decimal ScaleY { get; set; } = 1;
    public decimal ScaleZ { get; set; } = 1;
    public bool Enabled { get; set; } = true;

    public SceneNode SceneNode { get; set; } = null!;
    public ICollection<DeviceModelBinding> DeviceModelBindings { get; set; } = new List<DeviceModelBinding>();
    public ICollection<MovablePartBinding> MovablePartBindings { get; set; } = new List<MovablePartBinding>();
}

public sealed class DeviceModelBinding
{
    public long Id { get; set; }
    public long DeviceInstanceId { get; set; }
    public long ModelAssetId { get; set; }
    public long AssetVersionId { get; set; }
    public BindingStatus BindingStatus { get; set; } = BindingStatus.active;
    public DateTime ActiveFrom { get; set; }
    public DateTime? ActiveTo { get; set; }

    public DeviceInstance DeviceInstance { get; set; } = null!;
    public ModelAsset ModelAsset { get; set; } = null!;
    public AssetVersion AssetVersion { get; set; } = null!;
}

public sealed class MovablePartBinding
{
    public long Id { get; set; }
    public long ModelAssetId { get; set; }
    public long AssetVersionId { get; set; }
    public long? DeviceInstanceId { get; set; }
    public string? ObjectUuid { get; set; }
    public string ObjectName { get; set; } = string.Empty;
    public string? ObjectPath { get; set; }
    public string? ParentUuid { get; set; }
    public string? ParentPath { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string PartCode { get; set; } = string.Empty;
    public MotionType MotionType { get; set; } = MotionType.linear;
    public AxisMode AxisMode { get; set; } = AxisMode.world;
    public Axis Axis { get; set; } = Axis.z;
    public decimal? CustomAxisX { get; set; }
    public decimal? CustomAxisY { get; set; }
    public decimal? CustomAxisZ { get; set; }
    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }
    public decimal HomeValue { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal DefaultSpeed { get; set; } = 1;
    public BindingStatus BindingStatus { get; set; } = BindingStatus.active;
    public bool Enabled { get; set; } = true;

    public ModelAsset ModelAsset { get; set; } = null!;
    public AssetVersion AssetVersion { get; set; } = null!;
    public DeviceInstance? DeviceInstance { get; set; }
    public ICollection<MotionTarget> MotionTargets { get; set; } = new List<MotionTarget>();
}

public sealed class MotionTarget
{
    public long Id { get; set; }
    public long MovablePartId { get; set; }
    public string TargetCode { get; set; } = string.Empty;
    public string TargetName { get; set; } = string.Empty;
    public decimal? TargetValue { get; set; }
    public decimal? TargetX { get; set; }
    public decimal? TargetY { get; set; }
    public decimal? TargetZ { get; set; }
    public int SortNo { get; set; }
    public bool Enabled { get; set; } = true;

    public MovablePartBinding MovablePart { get; set; } = null!;
}

public sealed class OperationAudit
{
    public long Id { get; set; }
    public OperationType OperationType { get; set; }
    public OperationTargetType TargetType { get; set; }
    public long TargetId { get; set; }
    public string? BeforeJson { get; set; }
    public string? AfterJson { get; set; }
    public string? OperatorId { get; set; }
    public string? OperatorName { get; set; }
    public DateTime CreatedTime { get; set; }
}

public sealed class ToolPackage
{
    public long Id { get; set; }
    public string ToolCode { get; set; } = string.Empty;
    public string ToolName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string InstallPath { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public DateTime CreatedTime { get; set; }
    public DateTime UpdatedTime { get; set; }

    public ICollection<ToolHealthCheck> HealthChecks { get; set; } = new List<ToolHealthCheck>();
}

public sealed class ToolHealthCheck
{
    public long Id { get; set; }
    public long ToolPackageId { get; set; }
    public ToolHealthStatus Status { get; set; } = ToolHealthStatus.unknown;
    public string? Message { get; set; }
    public DateTime CheckedTime { get; set; }

    public ToolPackage ToolPackage { get; set; } = null!;
}
