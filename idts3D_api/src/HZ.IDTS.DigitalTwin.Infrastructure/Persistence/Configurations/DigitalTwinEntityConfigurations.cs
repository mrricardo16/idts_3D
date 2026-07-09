using HZ.IDTS.DigitalTwin.Domain.Entities;
using HZ.IDTS.DigitalTwin.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HZ.IDTS.DigitalTwin.Infrastructure.Persistence.Configurations;

internal sealed class ModelAssetConfiguration : IEntityTypeConfiguration<ModelAsset>
{
    public void Configure(EntityTypeBuilder<ModelAsset> builder)
    {
        builder.ToTable("model_asset");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.AssetCode).HasColumnName("asset_code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.AssetName).HasColumnName("asset_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.SourceFileName).HasColumnName("source_file_name").HasMaxLength(255).IsRequired();
        builder.Property(x => x.SourceFileHash).HasColumnName("source_file_hash").HasMaxLength(128).IsRequired();
        builder.Property(x => x.SourceFileType).HasColumnName("source_file_type").HasMaxLength(50).HasConversion<string>().HasDefaultValue(SourceFileType.glb).IsRequired();
        builder.Property(x => x.AssetType).HasColumnName("asset_type").HasMaxLength(50).HasConversion<string>().HasDefaultValue(AssetType.device_glb).IsRequired();
        builder.Property(x => x.ProcessingStatus).HasColumnName("processing_status").HasMaxLength(50).HasConversion<string>().HasDefaultValue(ProcessingStatus.pending).IsRequired();
        builder.Property(x => x.CurrentVersionId).HasColumnName("current_version_id");
        builder.Property(x => x.CreatedTime).HasColumnName("created_time").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.UpdatedTime).HasColumnName("updated_time").HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(x => x.AssetCode).IsUnique();
        builder.HasIndex(x => x.AssetName);
        builder.HasIndex(x => x.SourceFileHash).IsUnique();
        builder.HasIndex(x => x.AssetType);
        builder.HasIndex(x => x.ProcessingStatus);

        builder.HasOne(x => x.CurrentVersion)
            .WithMany(x => x.CurrentForAssets)
            .HasForeignKey(x => x.CurrentVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class AssetVersionConfiguration : IEntityTypeConfiguration<AssetVersion>
{
    public void Configure(EntityTypeBuilder<AssetVersion> builder)
    {
        builder.ToTable("asset_version");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ModelAssetId).HasColumnName("model_asset_id").IsRequired();
        builder.Property(x => x.VersionNo).HasColumnName("version_no").HasDefaultValue(1).IsRequired();
        builder.Property(x => x.VersionStatus).HasColumnName("version_status").HasMaxLength(50).HasConversion<string>().HasDefaultValue(VersionStatus.Draft).IsRequired();
        builder.Property(x => x.CreatedTime).HasColumnName("created_time").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.PublishedTime).HasColumnName("published_time");
        builder.Property(x => x.ArchivedTime).HasColumnName("archived_time");

        builder.HasIndex(x => new { x.ModelAssetId, x.VersionNo }).IsUnique();
        builder.HasIndex(x => x.VersionStatus);

        builder.HasOne(x => x.ModelAsset)
            .WithMany(x => x.Versions)
            .HasForeignKey(x => x.ModelAssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ModelAssetVariantConfiguration : IEntityTypeConfiguration<ModelAssetVariant>
{
    public void Configure(EntityTypeBuilder<ModelAssetVariant> builder)
    {
        builder.ToTable("model_asset_variant");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ModelAssetId).HasColumnName("model_asset_id").IsRequired();
        builder.Property(x => x.AssetVersionId).HasColumnName("asset_version_id").IsRequired();
        builder.Property(x => x.VariantLevel).HasColumnName("variant_level").HasMaxLength(50).HasConversion<string>().HasDefaultValue(VariantLevel.source).IsRequired();
        builder.Property(x => x.FileUrl).HasColumnName("file_url").HasMaxLength(500).IsRequired();
        builder.Property(x => x.FileHash).HasColumnName("file_hash").HasMaxLength(128).IsRequired();
        builder.Property(x => x.FileSize).HasColumnName("file_size").HasDefaultValue(0L).IsRequired();
        builder.Property(x => x.TriangleCount).HasColumnName("triangle_count");
        builder.Property(x => x.VertexCount).HasColumnName("vertex_count");
        builder.Property(x => x.MeshCount).HasColumnName("mesh_count");
        builder.Property(x => x.MaterialCount).HasColumnName("material_count");
        builder.Property(x => x.TextureCount).HasColumnName("texture_count");
        builder.Property(x => x.CreatedTime).HasColumnName("created_time").HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(x => new { x.AssetVersionId, x.VariantLevel }).IsUnique();
        builder.HasIndex(x => x.VariantLevel);
        builder.HasIndex(x => x.FileHash);

        builder.HasOne(x => x.ModelAsset).WithMany(x => x.Variants).HasForeignKey(x => x.ModelAssetId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AssetVersion).WithMany(x => x.Variants).HasForeignKey(x => x.AssetVersionId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ModelConversionJobConfiguration : IEntityTypeConfiguration<ModelConversionJob>
{
    public void Configure(EntityTypeBuilder<ModelConversionJob> builder)
    {
        builder.ToTable("model_conversion_job");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ModelAssetId).HasColumnName("model_asset_id").IsRequired();
        builder.Property(x => x.AssetVersionId).HasColumnName("asset_version_id").IsRequired();
        builder.Property(x => x.JobType).HasColumnName("job_type").HasMaxLength(50).HasConversion<string>().HasDefaultValue(ConversionJobType.upload).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).HasConversion<string>().HasDefaultValue(ConversionJobStatus.pending).IsRequired();
        builder.Property(x => x.Progress).HasColumnName("progress").HasDefaultValue(0).IsRequired();
        builder.Property(x => x.Message).HasColumnName("message").HasColumnType("text");
        builder.Property(x => x.InputFile).HasColumnName("input_file").HasMaxLength(500);
        builder.Property(x => x.OutputDirectory).HasColumnName("output_directory").HasMaxLength(500);
        builder.Property(x => x.StdoutLogUrl).HasColumnName("stdout_log_url").HasMaxLength(500);
        builder.Property(x => x.StderrLogUrl).HasColumnName("stderr_log_url").HasMaxLength(500);
        builder.Property(x => x.ExitCode).HasColumnName("exit_code");
        builder.Property(x => x.ElapsedMs).HasColumnName("elapsed_ms");
        builder.Property(x => x.RetryCount).HasColumnName("retry_count").HasDefaultValue(0).IsRequired();
        builder.Property(x => x.StartedTime).HasColumnName("started_time");
        builder.Property(x => x.FinishedTime).HasColumnName("finished_time");

        builder.HasIndex(x => x.JobType);
        builder.HasIndex(x => x.Status);

        builder.HasOne(x => x.ModelAsset).WithMany(x => x.ConversionJobs).HasForeignKey(x => x.ModelAssetId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AssetVersion).WithMany(x => x.ConversionJobs).HasForeignKey(x => x.AssetVersionId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ModelObjectIndexConfiguration : IEntityTypeConfiguration<ModelObjectIndex>
{
    public void Configure(EntityTypeBuilder<ModelObjectIndex> builder)
    {
        builder.ToTable("model_object_index");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ModelAssetId).HasColumnName("model_asset_id").IsRequired();
        builder.Property(x => x.AssetVersionId).HasColumnName("asset_version_id").IsRequired();
        builder.Property(x => x.ObjectUuid).HasColumnName("object_uuid").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ObjectName).HasColumnName("object_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.ObjectPath).HasColumnName("object_path").HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ParentUuid).HasColumnName("parent_uuid").HasMaxLength(100);
        builder.Property(x => x.ParentPath).HasColumnName("parent_path").HasMaxLength(1000);
        builder.Property(x => x.ObjectType).HasColumnName("object_type").HasMaxLength(50).IsRequired();
        DecimalColumn(builder.Property(x => x.BoundingBoxMinX).HasColumnName("bounding_box_min_x"));
        DecimalColumn(builder.Property(x => x.BoundingBoxMinY).HasColumnName("bounding_box_min_y"));
        DecimalColumn(builder.Property(x => x.BoundingBoxMinZ).HasColumnName("bounding_box_min_z"));
        DecimalColumn(builder.Property(x => x.BoundingBoxMaxX).HasColumnName("bounding_box_max_x"));
        DecimalColumn(builder.Property(x => x.BoundingBoxMaxY).HasColumnName("bounding_box_max_y"));
        DecimalColumn(builder.Property(x => x.BoundingBoxMaxZ).HasColumnName("bounding_box_max_z"));
        builder.Property(x => x.MeshFingerprint).HasColumnName("mesh_fingerprint").HasMaxLength(200);

        builder.HasIndex(x => new { x.AssetVersionId, x.ObjectUuid });
        builder.HasIndex(x => new { x.AssetVersionId, x.ObjectPath });
        builder.HasIndex(x => x.ObjectUuid);
        builder.HasIndex(x => x.ObjectName);
        builder.HasIndex(x => x.ObjectPath);
        builder.HasIndex(x => x.MeshFingerprint);

        builder.HasOne(x => x.ModelAsset).WithMany(x => x.ObjectIndexes).HasForeignKey(x => x.ModelAssetId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AssetVersion).WithMany(x => x.ObjectIndexes).HasForeignKey(x => x.AssetVersionId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void DecimalColumn(PropertyBuilder<decimal?> builder)
    {
        builder.HasColumnType("numeric(18,6)");
    }
}

internal sealed class AssetManifestConfiguration : IEntityTypeConfiguration<AssetManifest>
{
    public void Configure(EntityTypeBuilder<AssetManifest> builder)
    {
        builder.ToTable("asset_manifest");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ModelAssetId).HasColumnName("model_asset_id").IsRequired();
        builder.Property(x => x.AssetVersionId).HasColumnName("asset_version_id").IsRequired();
        builder.Property(x => x.ManifestJson).HasColumnName("manifest_json").HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb").IsRequired();
        builder.Property(x => x.ModelStatsJson).HasColumnName("model_stats_json").HasColumnType("jsonb");
        builder.Property(x => x.CreatedTime).HasColumnName("created_time").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.UpdatedTime).HasColumnName("updated_time").HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(x => x.AssetVersionId).IsUnique();

        builder.HasOne(x => x.ModelAsset).WithMany(x => x.Manifests).HasForeignKey(x => x.ModelAssetId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AssetVersion).WithOne(x => x.Manifest).HasForeignKey<AssetManifest>(x => x.AssetVersionId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SceneNodeConfiguration : IEntityTypeConfiguration<SceneNode>
{
    public void Configure(EntityTypeBuilder<SceneNode> builder)
    {
        builder.ToTable("scene_node");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ParentId).HasColumnName("parent_id");
        builder.Property(x => x.NodeCode).HasColumnName("node_code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.NodeName).HasColumnName("node_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.NodeType).HasColumnName("node_type").HasMaxLength(50).HasConversion<string>().HasDefaultValue(SceneNodeType.scene).IsRequired();
        builder.Property(x => x.SortNo).HasColumnName("sort_no").HasDefaultValue(0).IsRequired();
        builder.Property(x => x.Enabled).HasColumnName("enabled").HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => x.NodeCode).IsUnique();
        builder.HasIndex(x => x.NodeName);
        builder.HasIndex(x => x.NodeType);
        builder.HasIndex(x => x.Enabled);

        builder.HasOne(x => x.Parent).WithMany(x => x.Children).HasForeignKey(x => x.ParentId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class DeviceInstanceConfiguration : IEntityTypeConfiguration<DeviceInstance>
{
    public void Configure(EntityTypeBuilder<DeviceInstance> builder)
    {
        builder.ToTable("device_instance");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.SceneNodeId).HasColumnName("scene_node_id").IsRequired();
        builder.Property(x => x.DeviceCode).HasColumnName("device_code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.DeviceName).HasColumnName("device_name").HasMaxLength(200).IsRequired();
        DecimalColumn(builder.Property(x => x.PositionX).HasColumnName("position_x").HasDefaultValue(0m));
        DecimalColumn(builder.Property(x => x.PositionY).HasColumnName("position_y").HasDefaultValue(0m));
        DecimalColumn(builder.Property(x => x.PositionZ).HasColumnName("position_z").HasDefaultValue(0m));
        DecimalColumn(builder.Property(x => x.RotationX).HasColumnName("rotation_x").HasDefaultValue(0m));
        DecimalColumn(builder.Property(x => x.RotationY).HasColumnName("rotation_y").HasDefaultValue(0m));
        DecimalColumn(builder.Property(x => x.RotationZ).HasColumnName("rotation_z").HasDefaultValue(0m));
        DecimalColumn(builder.Property(x => x.ScaleX).HasColumnName("scale_x").HasDefaultValue(1m));
        DecimalColumn(builder.Property(x => x.ScaleY).HasColumnName("scale_y").HasDefaultValue(1m));
        DecimalColumn(builder.Property(x => x.ScaleZ).HasColumnName("scale_z").HasDefaultValue(1m));
        builder.Property(x => x.Enabled).HasColumnName("enabled").HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => x.DeviceCode).IsUnique();
        builder.HasIndex(x => x.DeviceName);
        builder.HasIndex(x => x.Enabled);

        builder.HasOne(x => x.SceneNode).WithMany(x => x.DeviceInstances).HasForeignKey(x => x.SceneNodeId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void DecimalColumn(PropertyBuilder<decimal> builder)
    {
        builder.HasColumnType("numeric(18,6)").IsRequired();
    }
}

internal sealed class DeviceModelBindingConfiguration : IEntityTypeConfiguration<DeviceModelBinding>
{
    public void Configure(EntityTypeBuilder<DeviceModelBinding> builder)
    {
        builder.ToTable("device_model_binding");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.DeviceInstanceId).HasColumnName("device_instance_id").IsRequired();
        builder.Property(x => x.ModelAssetId).HasColumnName("model_asset_id").IsRequired();
        builder.Property(x => x.AssetVersionId).HasColumnName("asset_version_id").IsRequired();
        builder.Property(x => x.BindingStatus).HasColumnName("binding_status").HasMaxLength(50).HasConversion<string>().HasDefaultValue(BindingStatus.active).IsRequired();
        builder.Property(x => x.ActiveFrom).HasColumnName("active_from").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.ActiveTo).HasColumnName("active_to");

        builder.HasIndex(x => new { x.DeviceInstanceId, x.AssetVersionId }).IsUnique();
        builder.HasIndex(x => x.BindingStatus);
        builder.HasIndex(x => x.DeviceInstanceId).IsUnique().HasFilter("binding_status = 'active'");

        builder.HasOne(x => x.DeviceInstance).WithMany(x => x.DeviceModelBindings).HasForeignKey(x => x.DeviceInstanceId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ModelAsset).WithMany(x => x.DeviceModelBindings).HasForeignKey(x => x.ModelAssetId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AssetVersion).WithMany(x => x.DeviceModelBindings).HasForeignKey(x => x.AssetVersionId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class MovablePartBindingConfiguration : IEntityTypeConfiguration<MovablePartBinding>
{
    public void Configure(EntityTypeBuilder<MovablePartBinding> builder)
    {
        builder.ToTable("movable_part_binding");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ModelAssetId).HasColumnName("model_asset_id").IsRequired();
        builder.Property(x => x.AssetVersionId).HasColumnName("asset_version_id").IsRequired();
        builder.Property(x => x.DeviceInstanceId).HasColumnName("device_instance_id");
        builder.Property(x => x.ObjectUuid).HasColumnName("object_uuid").HasMaxLength(100);
        builder.Property(x => x.ObjectName).HasColumnName("object_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.ObjectPath).HasColumnName("object_path").HasMaxLength(1000);
        builder.Property(x => x.ParentUuid).HasColumnName("parent_uuid").HasMaxLength(100);
        builder.Property(x => x.ParentPath).HasColumnName("parent_path").HasMaxLength(1000);
        builder.Property(x => x.BusinessName).HasColumnName("business_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.PartCode).HasColumnName("part_code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.MotionType).HasColumnName("motion_type").HasMaxLength(50).HasConversion<string>().HasDefaultValue(MotionType.linear).IsRequired();
        builder.Property(x => x.AxisMode).HasColumnName("axis_mode").HasMaxLength(50).HasConversion<string>().HasDefaultValue(AxisMode.world).IsRequired();
        builder.Property(x => x.Axis).HasColumnName("axis").HasMaxLength(10).HasConversion<string>().HasDefaultValue(Axis.z).IsRequired();
        DecimalColumn(builder.Property(x => x.CustomAxisX).HasColumnName("custom_axis_x"));
        DecimalColumn(builder.Property(x => x.CustomAxisY).HasColumnName("custom_axis_y"));
        DecimalColumn(builder.Property(x => x.CustomAxisZ).HasColumnName("custom_axis_z"));
        DecimalColumn(builder.Property(x => x.MinValue).HasColumnName("min_value").HasDefaultValue(0m));
        DecimalColumn(builder.Property(x => x.MaxValue).HasColumnName("max_value").HasDefaultValue(0m));
        DecimalColumn(builder.Property(x => x.HomeValue).HasColumnName("home_value").HasDefaultValue(0m));
        DecimalColumn(builder.Property(x => x.CurrentValue).HasColumnName("current_value").HasDefaultValue(0m));
        DecimalColumn(builder.Property(x => x.DefaultSpeed).HasColumnName("default_speed").HasDefaultValue(1m));
        builder.Property(x => x.BindingStatus).HasColumnName("binding_status").HasMaxLength(50).HasConversion<string>().HasDefaultValue(BindingStatus.active).IsRequired();
        builder.Property(x => x.Enabled).HasColumnName("enabled").HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => new { x.AssetVersionId, x.PartCode }).IsUnique();
        builder.HasIndex(x => x.ObjectUuid);
        builder.HasIndex(x => x.ObjectPath);
        builder.HasIndex(x => x.BusinessName);
        builder.HasIndex(x => x.BindingStatus);
        builder.HasIndex(x => x.Enabled);

        builder.HasOne(x => x.ModelAsset).WithMany(x => x.MovablePartBindings).HasForeignKey(x => x.ModelAssetId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.AssetVersion).WithMany(x => x.MovablePartBindings).HasForeignKey(x => x.AssetVersionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.DeviceInstance).WithMany(x => x.MovablePartBindings).HasForeignKey(x => x.DeviceInstanceId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void DecimalColumn(PropertyBuilder<decimal> builder)
    {
        builder.HasColumnType("numeric(18,6)").IsRequired();
    }

    private static void DecimalColumn(PropertyBuilder<decimal?> builder)
    {
        builder.HasColumnType("numeric(18,6)");
    }
}

internal sealed class MotionTargetConfiguration : IEntityTypeConfiguration<MotionTarget>
{
    public void Configure(EntityTypeBuilder<MotionTarget> builder)
    {
        builder.ToTable("motion_target");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.MovablePartId).HasColumnName("movable_part_id").IsRequired();
        builder.Property(x => x.TargetCode).HasColumnName("target_code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.TargetName).HasColumnName("target_name").HasMaxLength(200).IsRequired();
        DecimalColumn(builder.Property(x => x.TargetValue).HasColumnName("target_value"));
        DecimalColumn(builder.Property(x => x.TargetX).HasColumnName("target_x"));
        DecimalColumn(builder.Property(x => x.TargetY).HasColumnName("target_y"));
        DecimalColumn(builder.Property(x => x.TargetZ).HasColumnName("target_z"));
        builder.Property(x => x.SortNo).HasColumnName("sort_no").HasDefaultValue(0).IsRequired();
        builder.Property(x => x.Enabled).HasColumnName("enabled").HasDefaultValue(true).IsRequired();

        builder.HasIndex(x => new { x.MovablePartId, x.TargetCode }).IsUnique();
        builder.HasIndex(x => x.TargetValue);
        builder.HasIndex(x => x.Enabled);

        builder.HasOne(x => x.MovablePart).WithMany(x => x.MotionTargets).HasForeignKey(x => x.MovablePartId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void DecimalColumn(PropertyBuilder<decimal?> builder)
    {
        builder.HasColumnType("numeric(18,6)");
    }
}

internal sealed class OperationAuditConfiguration : IEntityTypeConfiguration<OperationAudit>
{
    public void Configure(EntityTypeBuilder<OperationAudit> builder)
    {
        builder.ToTable("operation_audit");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.OperationType).HasColumnName("operation_type").HasMaxLength(50).HasConversion<string>().IsRequired();
        builder.Property(x => x.TargetType).HasColumnName("target_type").HasMaxLength(50).HasConversion<string>().IsRequired();
        builder.Property(x => x.TargetId).HasColumnName("target_id").IsRequired();
        builder.Property(x => x.BeforeJson).HasColumnName("before_json").HasColumnType("jsonb");
        builder.Property(x => x.AfterJson).HasColumnName("after_json").HasColumnType("jsonb");
        builder.Property(x => x.OperatorId).HasColumnName("operator_id").HasMaxLength(100);
        builder.Property(x => x.OperatorName).HasColumnName("operator_name").HasMaxLength(100);
        builder.Property(x => x.CreatedTime).HasColumnName("created_time").HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(x => x.OperationType);
        builder.HasIndex(x => x.TargetType);
        builder.HasIndex(x => x.TargetId);
        builder.HasIndex(x => x.CreatedTime);
    }
}

internal sealed class ToolPackageConfiguration : IEntityTypeConfiguration<ToolPackage>
{
    public void Configure(EntityTypeBuilder<ToolPackage> builder)
    {
        builder.ToTable("tool_package");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ToolCode).HasColumnName("tool_code").HasMaxLength(100).IsRequired();
        builder.Property(x => x.ToolName).HasColumnName("tool_name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Version).HasColumnName("version").HasMaxLength(100).IsRequired();
        builder.Property(x => x.InstallPath).HasColumnName("install_path").HasMaxLength(500).IsRequired();
        builder.Property(x => x.Enabled).HasColumnName("enabled").HasDefaultValue(true).IsRequired();
        builder.Property(x => x.CreatedTime).HasColumnName("created_time").HasDefaultValueSql("now()").IsRequired();
        builder.Property(x => x.UpdatedTime).HasColumnName("updated_time").HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(x => x.ToolCode).IsUnique();
        builder.HasIndex(x => x.Version);
        builder.HasIndex(x => x.Enabled);
    }
}

internal sealed class ToolHealthCheckConfiguration : IEntityTypeConfiguration<ToolHealthCheck>
{
    public void Configure(EntityTypeBuilder<ToolHealthCheck> builder)
    {
        builder.ToTable("tool_health_check");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ToolPackageId).HasColumnName("tool_package_id").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(50).HasConversion<string>().HasDefaultValue(ToolHealthStatus.unknown).IsRequired();
        builder.Property(x => x.Message).HasColumnName("message").HasColumnType("text");
        builder.Property(x => x.CheckedTime).HasColumnName("checked_time").HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(x => new { x.ToolPackageId, x.CheckedTime });
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CheckedTime);

        builder.HasOne(x => x.ToolPackage).WithMany(x => x.HealthChecks).HasForeignKey(x => x.ToolPackageId).OnDelete(DeleteBehavior.Restrict);
    }
}
