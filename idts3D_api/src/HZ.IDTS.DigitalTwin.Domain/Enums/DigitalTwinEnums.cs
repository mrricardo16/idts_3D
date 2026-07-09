namespace HZ.IDTS.DigitalTwin.Domain.Enums;

public enum AssetType
{
    device_glb,
    static_glb
}

public enum SourceFileType
{
    glb
}

public enum ProcessingStatus
{
    pending,
    ready,
    failed
}

public enum VariantLevel
{
    source,
    high,
    medium,
    low,
    proxy
}

public enum VersionStatus
{
    Draft,
    Ready,
    Published,
    Archived,
    Failed,
    Invalid
}

public enum ConversionJobType
{
    upload,
    inspect,
    object_tree,
    model_stats,
    convert
}

public enum ConversionJobStatus
{
    pending,
    running,
    completed,
    failed,
    canceled
}

public enum SceneNodeType
{
    scene,
    area,
    floor,
    line
}

public enum BindingStatus
{
    active,
    archived,
    invalid
}

public enum MotionType
{
    linear,
    rotate,
    path,
    joint
}

public enum AxisMode
{
    world,
    local,
    custom
}

public enum Axis
{
    z,
    x,
    y
}

public enum OperationType
{
    upload,
    edit,
    publish,
    rollback,
    delete
}

public enum OperationTargetType
{
    model_asset,
    asset_version,
    movable_part,
    motion_target,
    scene
}

public enum ToolHealthStatus
{
    unknown,
    healthy,
    warning,
    error
}
