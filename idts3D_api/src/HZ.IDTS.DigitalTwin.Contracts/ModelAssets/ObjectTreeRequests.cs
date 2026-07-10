namespace HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

public sealed record SaveObjectTreeRequest(
    IReadOnlyList<ObjectTreeNodeRequest>? Nodes);

public sealed record ObjectTreeNodeRequest(
    string ObjectUuid,
    string ObjectName,
    string ObjectPath,
    string? ParentUuid,
    string? ParentPath,
    string ObjectType,
    ObjectTreeBoundingBoxRequest? BoundingBox,
    string? MeshFingerprint);

public sealed record ObjectTreeBoundingBoxRequest(
    ObjectTreeVector3Request Min,
    ObjectTreeVector3Request Max);

public sealed record ObjectTreeVector3Request(
    decimal X,
    decimal Y,
    decimal Z);

public sealed record GetObjectTreeRequest(
    long AssetId,
    long? VersionId,
    string? Mode);
