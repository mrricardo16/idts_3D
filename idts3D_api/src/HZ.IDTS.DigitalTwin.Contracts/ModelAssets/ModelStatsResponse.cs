namespace HZ.IDTS.DigitalTwin.Contracts.ModelAssets;

public sealed record ModelStatsResponse(
    long AssetId,
    long VersionId,
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
    IReadOnlyList<string> BudgetMessages,
    DateTime SavedTime);
