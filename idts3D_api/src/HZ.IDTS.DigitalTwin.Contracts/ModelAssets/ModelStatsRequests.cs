namespace HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

public sealed record SaveModelStatsRequest(
    decimal FileSizeMb,
    int MeshCount,
    int MaterialCount,
    int TextureCount,
    long VertexCount,
    long TriangleCount,
    int DrawCallEstimate,
    int MaxTextureSize,
    bool HasMovableCandidates,
    bool HasDuplicatedNames,
    bool HasInvalidMaterials,
    bool IsOverBudget,
    IReadOnlyList<string>? BudgetMessages);

public sealed record GetModelStatsRequest(
    long AssetId,
    long VersionId);
