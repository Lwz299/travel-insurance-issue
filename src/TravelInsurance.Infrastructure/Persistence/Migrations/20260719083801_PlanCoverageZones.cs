using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelInsurance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PlanCoverageZones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "coverage_zones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    is_worldwide = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coverage_zones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "coverage_zone_countries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    coverage_zone_id = table.Column<Guid>(type: "uuid", nullable: false),
                    country_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_excluded = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coverage_zone_countries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_coverage_zone_countries_countries_country_id",
                        column: x => x.country_id,
                        principalTable: "countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_coverage_zone_countries_coverage_zones_coverage_zone_id",
                        column: x => x.coverage_zone_id,
                        principalTable: "coverage_zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plan_coverage_zones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    coverage_zone_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_coverage_zones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_plan_coverage_zones_coverage_zones_coverage_zone_id",
                        column: x => x.coverage_zone_id,
                        principalTable: "coverage_zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_plan_coverage_zones_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_coverage_zone_countries_country_id",
                table: "coverage_zone_countries",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "IX_coverage_zone_countries_coverage_zone_id_country_id",
                table: "coverage_zone_countries",
                columns: new[] { "coverage_zone_id", "country_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_coverage_zones_code",
                table: "coverage_zones",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plan_coverage_zones_coverage_zone_id",
                table: "plan_coverage_zones",
                column: "coverage_zone_id");

            migrationBuilder.CreateIndex(
                name: "IX_plan_coverage_zones_plan_id_coverage_zone_id",
                table: "plan_coverage_zones",
                columns: new[] { "plan_id", "coverage_zone_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "coverage_zone_countries");

            migrationBuilder.DropTable(
                name: "plan_coverage_zones");

            migrationBuilder.DropTable(
                name: "coverage_zones");
        }
    }
}
