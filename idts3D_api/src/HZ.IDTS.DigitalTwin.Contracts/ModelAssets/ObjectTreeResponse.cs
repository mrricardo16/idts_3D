using System.Text.Json.Serialization;

namespace HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

public sealed record ObjectTreeResponse(
    long AssetId,
    long VersionId,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] int? NodeCount,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<ObjectTreeNodeResponse>? Nodes,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DateTime? SavedTime);

public sealed record ObjectTreeNodeResponse(
    string ObjectUuid,
    string ObjectName,
    string ObjectPath,
    string? ParentUuid,
    string? ParentPath,
    string ObjectType,
    ObjectTreeBoundingBoxResponse? BoundingBox,
    string? MeshFingerprint);

public sealed record ObjectTreeBoundingBoxResponse(
    ObjectTreeVector3Response Min,
    ObjectTreeVector3Response Max);

public sealed record ObjectTreeVector3Response(
    decimal X,
    decimal Y,
    decimal Z);
