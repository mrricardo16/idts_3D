using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HZ.IDTS.DigitalTwin.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitDigitalTwinSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "operation_audit",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    operation_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    target_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    target_id = table.Column<long>(type: "bigint", nullable: false),
                    before_json = table.Column<string>(type: "jsonb", nullable: true),
                    after_json = table.Column<string>(type: "jsonb", nullable: true),
                    operator_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    operator_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    created_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operation_audit", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "scene_node",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parent_id = table.Column<long>(type: "bigint", nullable: true),
                    node_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    node_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    node_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "scene"),
                    sort_no = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scene_node", x => x.id);
                    table.ForeignKey(
                        name: "FK_scene_node_scene_node_parent_id",
                        column: x => x.parent_id,
                        principalTable: "scene_node",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tool_package",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tool_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tool_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    version = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    install_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tool_package", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "device_instance",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    scene_node_id = table.Column<long>(type: "bigint", nullable: false),
                    device_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    device_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    position_x = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 0m),
                    position_y = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 0m),
                    position_z = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 0m),
                    rotation_x = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 0m),
                    rotation_y = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 0m),
                    rotation_z = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 0m),
                    scale_x = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 1m),
                    scale_y = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 1m),
                    scale_z = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 1m),
                    enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_instance", x => x.id);
                    table.ForeignKey(
                        name: "FK_device_instance_scene_node_scene_node_id",
                        column: x => x.scene_node_id,
                        principalTable: "scene_node",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tool_health_check",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tool_package_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "unknown"),
                    message = table.Column<string>(type: "text", nullable: true),
                    checked_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tool_health_check", x => x.id);
                    table.ForeignKey(
                        name: "FK_tool_health_check_tool_package_tool_package_id",
                        column: x => x.tool_package_id,
                        principalTable: "tool_package",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asset_manifest",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    model_asset_id = table.Column<long>(type: "bigint", nullable: false),
                    asset_version_id = table.Column<long>(type: "bigint", nullable: false),
                    manifest_json = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    model_stats_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_manifest", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "asset_version",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    model_asset_id = table.Column<long>(type: "bigint", nullable: false),
                    version_no = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    version_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Draft"),
                    created_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    published_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_version", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "model_asset",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    asset_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    asset_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    source_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    source_file_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    source_file_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "glb"),
                    asset_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "device_glb"),
                    processing_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    current_version_id = table.Column<long>(type: "bigint", nullable: true),
                    created_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_model_asset", x => x.id);
                    table.ForeignKey(
                        name: "FK_model_asset_asset_version_current_version_id",
                        column: x => x.current_version_id,
                        principalTable: "asset_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "device_model_binding",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    device_instance_id = table.Column<long>(type: "bigint", nullable: false),
                    model_asset_id = table.Column<long>(type: "bigint", nullable: false),
                    asset_version_id = table.Column<long>(type: "bigint", nullable: false),
                    binding_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "active"),
                    active_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    active_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_model_binding", x => x.id);
                    table.ForeignKey(
                        name: "FK_device_model_binding_asset_version_asset_version_id",
                        column: x => x.asset_version_id,
                        principalTable: "asset_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_device_model_binding_device_instance_device_instance_id",
                        column: x => x.device_instance_id,
                        principalTable: "device_instance",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_device_model_binding_model_asset_model_asset_id",
                        column: x => x.model_asset_id,
                        principalTable: "model_asset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "model_asset_variant",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    model_asset_id = table.Column<long>(type: "bigint", nullable: false),
                    asset_version_id = table.Column<long>(type: "bigint", nullable: false),
                    variant_level = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "source"),
                    file_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    file_hash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    triangle_count = table.Column<long>(type: "bigint", nullable: true),
                    vertex_count = table.Column<long>(type: "bigint", nullable: true),
                    mesh_count = table.Column<int>(type: "integer", nullable: true),
                    material_count = table.Column<int>(type: "integer", nullable: true),
                    texture_count = table.Column<int>(type: "integer", nullable: true),
                    created_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_model_asset_variant", x => x.id);
                    table.ForeignKey(
                        name: "FK_model_asset_variant_asset_version_asset_version_id",
                        column: x => x.asset_version_id,
                        principalTable: "asset_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_model_asset_variant_model_asset_model_asset_id",
                        column: x => x.model_asset_id,
                        principalTable: "model_asset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "model_conversion_job",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    model_asset_id = table.Column<long>(type: "bigint", nullable: false),
                    asset_version_id = table.Column<long>(type: "bigint", nullable: false),
                    job_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "upload"),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "pending"),
                    progress = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    message = table.Column<string>(type: "text", nullable: true),
                    input_file = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    output_directory = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    stdout_log_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    stderr_log_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    exit_code = table.Column<int>(type: "integer", nullable: true),
                    elapsed_ms = table.Column<long>(type: "bigint", nullable: true),
                    retry_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    started_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    finished_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_model_conversion_job", x => x.id);
                    table.ForeignKey(
                        name: "FK_model_conversion_job_asset_version_asset_version_id",
                        column: x => x.asset_version_id,
                        principalTable: "asset_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_model_conversion_job_model_asset_model_asset_id",
                        column: x => x.model_asset_id,
                        principalTable: "model_asset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "model_object_index",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    model_asset_id = table.Column<long>(type: "bigint", nullable: false),
                    asset_version_id = table.Column<long>(type: "bigint", nullable: false),
                    object_uuid = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    object_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    object_path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    parent_uuid = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    parent_path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    object_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    bounding_box_min_x = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    bounding_box_min_y = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    bounding_box_min_z = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    bounding_box_max_x = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    bounding_box_max_y = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    bounding_box_max_z = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    mesh_fingerprint = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_model_object_index", x => x.id);
                    table.ForeignKey(
                        name: "FK_model_object_index_asset_version_asset_version_id",
                        column: x => x.asset_version_id,
                        principalTable: "asset_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_model_object_index_model_asset_model_asset_id",
                        column: x => x.model_asset_id,
                        principalTable: "model_asset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "movable_part_binding",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    model_asset_id = table.Column<long>(type: "bigint", nullable: false),
                    asset_version_id = table.Column<long>(type: "bigint", nullable: false),
                    device_instance_id = table.Column<long>(type: "bigint", nullable: true),
                    object_uuid = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    object_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    object_path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    parent_uuid = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    parent_path = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    business_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    part_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    motion_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "linear"),
                    axis_mode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "world"),
                    axis = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "z"),
                    custom_axis_x = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    custom_axis_y = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    custom_axis_z = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    min_value = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 0m),
                    max_value = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 0m),
                    home_value = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 0m),
                    current_value = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 0m),
                    default_speed = table.Column<decimal>(type: "numeric(18,6)", nullable: false, defaultValue: 1m),
                    binding_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "active"),
                    enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movable_part_binding", x => x.id);
                    table.ForeignKey(
                        name: "FK_movable_part_binding_asset_version_asset_version_id",
                        column: x => x.asset_version_id,
                        principalTable: "asset_version",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movable_part_binding_device_instance_device_instance_id",
                        column: x => x.device_instance_id,
                        principalTable: "device_instance",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movable_part_binding_model_asset_model_asset_id",
                        column: x => x.model_asset_id,
                        principalTable: "model_asset",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "motion_target",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    movable_part_id = table.Column<long>(type: "bigint", nullable: false),
                    target_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    target_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    target_value = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    target_x = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    target_y = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    target_z = table.Column<decimal>(type: "numeric(18,6)", nullable: true),
                    sort_no = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_motion_target", x => x.id);
                    table.ForeignKey(
                        name: "FK_motion_target_movable_part_binding_movable_part_id",
                        column: x => x.movable_part_id,
                        principalTable: "movable_part_binding",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_asset_manifest_asset_version_id",
                table: "asset_manifest",
                column: "asset_version_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_asset_manifest_model_asset_id",
                table: "asset_manifest",
                column: "model_asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_asset_version_model_asset_id_version_no",
                table: "asset_version",
                columns: new[] { "model_asset_id", "version_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_asset_version_version_status",
                table: "asset_version",
                column: "version_status");

            migrationBuilder.CreateIndex(
                name: "IX_device_instance_device_code",
                table: "device_instance",
                column: "device_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_device_instance_device_name",
                table: "device_instance",
                column: "device_name");

            migrationBuilder.CreateIndex(
                name: "IX_device_instance_enabled",
                table: "device_instance",
                column: "enabled");

            migrationBuilder.CreateIndex(
                name: "IX_device_instance_scene_node_id",
                table: "device_instance",
                column: "scene_node_id");

            migrationBuilder.CreateIndex(
                name: "IX_device_model_binding_asset_version_id",
                table: "device_model_binding",
                column: "asset_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_device_model_binding_binding_status",
                table: "device_model_binding",
                column: "binding_status");

            migrationBuilder.CreateIndex(
                name: "IX_device_model_binding_device_instance_id",
                table: "device_model_binding",
                column: "device_instance_id",
                unique: true,
                filter: "binding_status = 'active'");

            migrationBuilder.CreateIndex(
                name: "IX_device_model_binding_device_instance_id_asset_version_id",
                table: "device_model_binding",
                columns: new[] { "device_instance_id", "asset_version_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_device_model_binding_model_asset_id",
                table: "device_model_binding",
                column: "model_asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_model_asset_asset_code",
                table: "model_asset",
                column: "asset_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_model_asset_asset_name",
                table: "model_asset",
                column: "asset_name");

            migrationBuilder.CreateIndex(
                name: "IX_model_asset_asset_type",
                table: "model_asset",
                column: "asset_type");

            migrationBuilder.CreateIndex(
                name: "IX_model_asset_current_version_id",
                table: "model_asset",
                column: "current_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_model_asset_processing_status",
                table: "model_asset",
                column: "processing_status");

            migrationBuilder.CreateIndex(
                name: "IX_model_asset_source_file_hash",
                table: "model_asset",
                column: "source_file_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_model_asset_variant_asset_version_id_variant_level",
                table: "model_asset_variant",
                columns: new[] { "asset_version_id", "variant_level" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_model_asset_variant_file_hash",
                table: "model_asset_variant",
                column: "file_hash");

            migrationBuilder.CreateIndex(
                name: "IX_model_asset_variant_model_asset_id",
                table: "model_asset_variant",
                column: "model_asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_model_asset_variant_variant_level",
                table: "model_asset_variant",
                column: "variant_level");

            migrationBuilder.CreateIndex(
                name: "IX_model_conversion_job_asset_version_id",
                table: "model_conversion_job",
                column: "asset_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_model_conversion_job_job_type",
                table: "model_conversion_job",
                column: "job_type");

            migrationBuilder.CreateIndex(
                name: "IX_model_conversion_job_model_asset_id",
                table: "model_conversion_job",
                column: "model_asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_model_conversion_job_status",
                table: "model_conversion_job",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_model_object_index_asset_version_id_object_path",
                table: "model_object_index",
                columns: new[] { "asset_version_id", "object_path" });

            migrationBuilder.CreateIndex(
                name: "IX_model_object_index_asset_version_id_object_uuid",
                table: "model_object_index",
                columns: new[] { "asset_version_id", "object_uuid" });

            migrationBuilder.CreateIndex(
                name: "IX_model_object_index_mesh_fingerprint",
                table: "model_object_index",
                column: "mesh_fingerprint");

            migrationBuilder.CreateIndex(
                name: "IX_model_object_index_model_asset_id",
                table: "model_object_index",
                column: "model_asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_model_object_index_object_name",
                table: "model_object_index",
                column: "object_name");

            migrationBuilder.CreateIndex(
                name: "IX_model_object_index_object_path",
                table: "model_object_index",
                column: "object_path");

            migrationBuilder.CreateIndex(
                name: "IX_model_object_index_object_uuid",
                table: "model_object_index",
                column: "object_uuid");

            migrationBuilder.CreateIndex(
                name: "IX_motion_target_enabled",
                table: "motion_target",
                column: "enabled");

            migrationBuilder.CreateIndex(
                name: "IX_motion_target_movable_part_id_target_code",
                table: "motion_target",
                columns: new[] { "movable_part_id", "target_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_motion_target_target_value",
                table: "motion_target",
                column: "target_value");

            migrationBuilder.CreateIndex(
                name: "IX_movable_part_binding_asset_version_id_part_code",
                table: "movable_part_binding",
                columns: new[] { "asset_version_id", "part_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_movable_part_binding_binding_status",
                table: "movable_part_binding",
                column: "binding_status");

            migrationBuilder.CreateIndex(
                name: "IX_movable_part_binding_business_name",
                table: "movable_part_binding",
                column: "business_name");

            migrationBuilder.CreateIndex(
                name: "IX_movable_part_binding_device_instance_id",
                table: "movable_part_binding",
                column: "device_instance_id");

            migrationBuilder.CreateIndex(
                name: "IX_movable_part_binding_enabled",
                table: "movable_part_binding",
                column: "enabled");

            migrationBuilder.CreateIndex(
                name: "IX_movable_part_binding_model_asset_id",
                table: "movable_part_binding",
                column: "model_asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_movable_part_binding_object_path",
                table: "movable_part_binding",
                column: "object_path");

            migrationBuilder.CreateIndex(
                name: "IX_movable_part_binding_object_uuid",
                table: "movable_part_binding",
                column: "object_uuid");

            migrationBuilder.CreateIndex(
                name: "IX_operation_audit_created_time",
                table: "operation_audit",
                column: "created_time");

            migrationBuilder.CreateIndex(
                name: "IX_operation_audit_operation_type",
                table: "operation_audit",
                column: "operation_type");

            migrationBuilder.CreateIndex(
                name: "IX_operation_audit_target_id",
                table: "operation_audit",
                column: "target_id");

            migrationBuilder.CreateIndex(
                name: "IX_operation_audit_target_type",
                table: "operation_audit",
                column: "target_type");

            migrationBuilder.CreateIndex(
                name: "IX_scene_node_enabled",
                table: "scene_node",
                column: "enabled");

            migrationBuilder.CreateIndex(
                name: "IX_scene_node_node_code",
                table: "scene_node",
                column: "node_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_scene_node_node_name",
                table: "scene_node",
                column: "node_name");

            migrationBuilder.CreateIndex(
                name: "IX_scene_node_node_type",
                table: "scene_node",
                column: "node_type");

            migrationBuilder.CreateIndex(
                name: "IX_scene_node_parent_id",
                table: "scene_node",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_tool_health_check_checked_time",
                table: "tool_health_check",
                column: "checked_time");

            migrationBuilder.CreateIndex(
                name: "IX_tool_health_check_status",
                table: "tool_health_check",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_tool_health_check_tool_package_id_checked_time",
                table: "tool_health_check",
                columns: new[] { "tool_package_id", "checked_time" });

            migrationBuilder.CreateIndex(
                name: "IX_tool_package_enabled",
                table: "tool_package",
                column: "enabled");

            migrationBuilder.CreateIndex(
                name: "IX_tool_package_tool_code",
                table: "tool_package",
                column: "tool_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tool_package_version",
                table: "tool_package",
                column: "version");

            migrationBuilder.AddForeignKey(
                name: "FK_asset_manifest_asset_version_asset_version_id",
                table: "asset_manifest",
                column: "asset_version_id",
                principalTable: "asset_version",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_asset_manifest_model_asset_model_asset_id",
                table: "asset_manifest",
                column: "model_asset_id",
                principalTable: "model_asset",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_asset_version_model_asset_model_asset_id",
                table: "asset_version",
                column: "model_asset_id",
                principalTable: "model_asset",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_model_asset_asset_version_current_version_id",
                table: "model_asset");

            migrationBuilder.DropTable(
                name: "asset_manifest");

            migrationBuilder.DropTable(
                name: "device_model_binding");

            migrationBuilder.DropTable(
                name: "model_asset_variant");

            migrationBuilder.DropTable(
                name: "model_conversion_job");

            migrationBuilder.DropTable(
                name: "model_object_index");

            migrationBuilder.DropTable(
                name: "motion_target");

            migrationBuilder.DropTable(
                name: "operation_audit");

            migrationBuilder.DropTable(
                name: "tool_health_check");

            migrationBuilder.DropTable(
                name: "movable_part_binding");

            migrationBuilder.DropTable(
                name: "tool_package");

            migrationBuilder.DropTable(
                name: "device_instance");

            migrationBuilder.DropTable(
                name: "scene_node");

            migrationBuilder.DropTable(
                name: "asset_version");

            migrationBuilder.DropTable(
                name: "model_asset");
        }
    }
}
