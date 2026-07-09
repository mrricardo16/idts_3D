using HZ.IDTS.DigitalTwin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HZ.IDTS.DigitalTwin.Infrastructure.Persistence;

public sealed class DigitalTwinDbContext : DbContext
{
    public DigitalTwinDbContext(DbContextOptions<DigitalTwinDbContext> options)
        : base(options)
    {
    }

    public DbSet<ModelAsset> ModelAssets => Set<ModelAsset>();
    public DbSet<AssetVersion> AssetVersions => Set<AssetVersion>();
    public DbSet<ModelAssetVariant> ModelAssetVariants => Set<ModelAssetVariant>();
    public DbSet<ModelConversionJob> ModelConversionJobs => Set<ModelConversionJob>();
    public DbSet<ModelObjectIndex> ModelObjectIndexes => Set<ModelObjectIndex>();
    public DbSet<AssetManifest> AssetManifests => Set<AssetManifest>();
    public DbSet<SceneNode> SceneNodes => Set<SceneNode>();
    public DbSet<DeviceInstance> DeviceInstances => Set<DeviceInstance>();
    public DbSet<DeviceModelBinding> DeviceModelBindings => Set<DeviceModelBinding>();
    public DbSet<MovablePartBinding> MovablePartBindings => Set<MovablePartBinding>();
    public DbSet<MotionTarget> MotionTargets => Set<MotionTarget>();
    public DbSet<OperationAudit> OperationAudits => Set<OperationAudit>();
    public DbSet<ToolPackage> ToolPackages => Set<ToolPackage>();
    public DbSet<ToolHealthCheck> ToolHealthChecks => Set<ToolHealthCheck>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DigitalTwinDbContext).Assembly);
    }
}
