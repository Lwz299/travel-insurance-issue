using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TravelInsurance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CatalogModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "plan_benefits");

            // Legacy embedded plan_coverages cannot map to catalog assignments — clear for re-seed.
            migrationBuilder.Sql("DELETE FROM plan_coverages;");

            migrationBuilder.DropIndex(
                name: "IX_plan_coverages_plan_id",
                table: "plan_coverages");

            migrationBuilder.DropColumn(
                name: "description",
                table: "plan_coverages");

            migrationBuilder.DropColumn(
                name: "max_amount",
                table: "plan_coverages");

            migrationBuilder.DropColumn(
                name: "name",
                table: "plan_coverages");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "plan_coverages",
                newName: "is_enabled");

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                table: "plan_coverages",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3);

            migrationBuilder.AddColumn<Guid>(
                name: "coverage_id",
                table: "plan_coverages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "custom_deductible",
                table: "plan_coverages",
                type: "numeric(14,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "custom_limit",
                table: "plan_coverages",
                type: "numeric(14,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "display_order",
                table: "plan_coverages",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "benefit_types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_benefit_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "coverage_groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coverage_groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "limit_types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_limit_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "risk_types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_risk_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "benefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    benefit_type_id = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_benefits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_benefits_benefit_types_benefit_type_id",
                        column: x => x.benefit_type_id,
                        principalTable: "benefit_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "coverages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    risk_type_id = table.Column<int>(type: "integer", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coverages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_coverages_coverage_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "coverage_groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_coverages_risk_types_risk_type_id",
                        column: x => x.risk_type_id,
                        principalTable: "risk_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "plan_coverage_benefit_overrides",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_coverage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    benefit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    limit_type_id = table.Column<int>(type: "integer", nullable: false),
                    limit_override = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    deductible_override = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_coverage_benefit_overrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_plan_coverage_benefit_overrides_benefits_benefit_id",
                        column: x => x.benefit_id,
                        principalTable: "benefits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_plan_coverage_benefit_overrides_limit_types_limit_type_id",
                        column: x => x.limit_type_id,
                        principalTable: "limit_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_plan_coverage_benefit_overrides_plan_coverages_plan_coverag~",
                        column: x => x.plan_coverage_id,
                        principalTable: "plan_coverages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "coverage_benefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    coverage_id = table.Column<Guid>(type: "uuid", nullable: false),
                    benefit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    limit_type_id = table.Column<int>(type: "integer", nullable: false),
                    limit_value = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    deductible = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    waiting_period_days = table.Column<int>(type: "integer", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    is_optional = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coverage_benefits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_coverage_benefits_benefits_benefit_id",
                        column: x => x.benefit_id,
                        principalTable: "benefits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_coverage_benefits_coverages_coverage_id",
                        column: x => x.coverage_id,
                        principalTable: "coverages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_coverage_benefits_limit_types_limit_type_id",
                        column: x => x.limit_type_id,
                        principalTable: "limit_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "benefit_types",
                columns: new[] { "Id", "code", "is_active", "name" },
                values: new object[,]
                {
                    { 1, "FINANCIAL", true, "Financial" },
                    { 2, "SERVICE", true, "Service" },
                    { 3, "ASSISTANCE", true, "Assistance" },
                    { 4, "REIMBURSEMENT", true, "Reimbursement" }
                });

            migrationBuilder.InsertData(
                table: "limit_types",
                columns: new[] { "Id", "code", "is_active", "name" },
                values: new object[,]
                {
                    { 1, "AMOUNT", true, "Amount" },
                    { 2, "PERCENTAGE", true, "Percentage" },
                    { 3, "UNLIMITED", true, "Unlimited" },
                    { 4, "PER_PERSON", true, "Per Person" },
                    { 5, "PER_TRIP", true, "Per Trip" }
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "Id", "description", "module", "name" },
                values: new object[] { 11, "Manage insurance product catalog", "Catalog", "CATALOG.MANAGE" });

            migrationBuilder.InsertData(
                table: "risk_types",
                columns: new[] { "Id", "code", "is_active", "name" },
                values: new object[,]
                {
                    { 1, "MEDICAL", true, "Medical" },
                    { 2, "TRAVEL", true, "Travel" },
                    { 3, "PROPERTY", true, "Property" },
                    { 4, "LIABILITY", true, "Liability" },
                    { 5, "LIFE", true, "Life" },
                    { 6, "ACCIDENT", true, "Accident" }
                });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "permission_id", "role_id" },
                values: new object[] { 11, 5 });

            migrationBuilder.CreateIndex(
                name: "IX_plan_coverages_coverage_id",
                table: "plan_coverages",
                column: "coverage_id");

            migrationBuilder.CreateIndex(
                name: "IX_plan_coverages_plan_id_coverage_id",
                table: "plan_coverages",
                columns: new[] { "plan_id", "coverage_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_benefit_types_code",
                table: "benefit_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_benefits_benefit_type_id",
                table: "benefits",
                column: "benefit_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_benefits_code",
                table: "benefits",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_coverage_benefits_benefit_id",
                table: "coverage_benefits",
                column: "benefit_id");

            migrationBuilder.CreateIndex(
                name: "IX_coverage_benefits_coverage_id_benefit_id",
                table: "coverage_benefits",
                columns: new[] { "coverage_id", "benefit_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_coverage_benefits_limit_type_id",
                table: "coverage_benefits",
                column: "limit_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_coverage_groups_code",
                table: "coverage_groups",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_coverages_code",
                table: "coverages",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_coverages_group_id",
                table: "coverages",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_coverages_risk_type_id",
                table: "coverages",
                column: "risk_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_limit_types_code",
                table: "limit_types",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plan_coverage_benefit_overrides_benefit_id",
                table: "plan_coverage_benefit_overrides",
                column: "benefit_id");

            migrationBuilder.CreateIndex(
                name: "IX_plan_coverage_benefit_overrides_limit_type_id",
                table: "plan_coverage_benefit_overrides",
                column: "limit_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_plan_coverage_benefit_overrides_plan_coverage_id_benefit_id",
                table: "plan_coverage_benefit_overrides",
                columns: new[] { "plan_coverage_id", "benefit_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_risk_types_code",
                table: "risk_types",
                column: "code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_plan_coverages_coverages_coverage_id",
                table: "plan_coverages",
                column: "coverage_id",
                principalTable: "coverages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_plan_coverages_coverages_coverage_id",
                table: "plan_coverages");

            migrationBuilder.DropTable(
                name: "coverage_benefits");

            migrationBuilder.DropTable(
                name: "plan_coverage_benefit_overrides");

            migrationBuilder.DropTable(
                name: "coverages");

            migrationBuilder.DropTable(
                name: "benefits");

            migrationBuilder.DropTable(
                name: "limit_types");

            migrationBuilder.DropTable(
                name: "coverage_groups");

            migrationBuilder.DropTable(
                name: "risk_types");

            migrationBuilder.DropTable(
                name: "benefit_types");

            migrationBuilder.DropIndex(
                name: "IX_plan_coverages_coverage_id",
                table: "plan_coverages");

            migrationBuilder.DropIndex(
                name: "IX_plan_coverages_plan_id_coverage_id",
                table: "plan_coverages");

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { 11, 5 });

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DropColumn(
                name: "coverage_id",
                table: "plan_coverages");

            migrationBuilder.DropColumn(
                name: "custom_deductible",
                table: "plan_coverages");

            migrationBuilder.DropColumn(
                name: "custom_limit",
                table: "plan_coverages");

            migrationBuilder.DropColumn(
                name: "display_order",
                table: "plan_coverages");

            migrationBuilder.RenameColumn(
                name: "is_enabled",
                table: "plan_coverages",
                newName: "is_active");

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                table: "plan_coverages",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "plan_coverages",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "max_amount",
                table: "plan_coverages",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "plan_coverages",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "plan_benefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    benefit_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    coverage_limit = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    excess_amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    limit_notes = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_benefits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_plan_benefits_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_plan_coverages_plan_id",
                table: "plan_coverages",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_plan_benefits_plan_id",
                table: "plan_benefits",
                column: "plan_id");
        }
    }
}
