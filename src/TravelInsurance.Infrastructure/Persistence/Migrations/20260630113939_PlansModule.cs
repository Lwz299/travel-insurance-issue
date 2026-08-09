using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TravelInsurance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PlansModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    coverage_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    default_duration_days = table.Column<int>(type: "integer", nullable: true),
                    min_age = table.Column<int>(type: "integer", nullable: true),
                    max_age = table.Column<int>(type: "integer", nullable: true),
                    max_trip_duration_days = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_plans_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "plan_benefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    benefit_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    coverage_limit = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    limit_notes = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    excess_amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false, defaultValue: 0m),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "plan_countries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    country_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_excluded = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_countries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_plan_countries_countries_country_id",
                        column: x => x.country_id,
                        principalTable: "countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_plan_countries_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "plan_coverages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    max_amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_coverages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_plan_coverages_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pricing_rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    min_age = table.Column<int>(type: "integer", nullable: false),
                    max_age = table.Column<int>(type: "integer", nullable: false),
                    duration_from = table.Column<int>(type: "integer", nullable: false),
                    duration_to = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pricing_rules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pricing_rules_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "countries",
                columns: new[] { "Id", "code", "name" },
                values: new object[,]
                {
                    { new Guid("c0000000-0000-0000-0000-000000000001"), "IQ", "Iraq" },
                    { new Guid("c0000000-0000-0000-0000-000000000002"), "US", "United States" },
                    { new Guid("c0000000-0000-0000-0000-000000000003"), "GB", "United Kingdom" },
                    { new Guid("c0000000-0000-0000-0000-000000000004"), "AE", "United Arab Emirates" },
                    { new Guid("c0000000-0000-0000-0000-000000000005"), "SA", "Saudi Arabia" },
                    { new Guid("c0000000-0000-0000-0000-000000000006"), "TR", "Turkey" },
                    { new Guid("c0000000-0000-0000-0000-000000000007"), "EG", "Egypt" },
                    { new Guid("c0000000-0000-0000-0000-000000000008"), "FR", "France" },
                    { new Guid("c0000000-0000-0000-0000-000000000009"), "DE", "Germany" },
                    { new Guid("c0000000-0000-0000-0000-000000000010"), "IT", "Italy" },
                    { new Guid("c0000000-0000-0000-0000-000000000011"), "ES", "Spain" },
                    { new Guid("c0000000-0000-0000-0000-000000000012"), "NL", "Netherlands" },
                    { new Guid("c0000000-0000-0000-0000-000000000013"), "CH", "Switzerland" },
                    { new Guid("c0000000-0000-0000-0000-000000000014"), "SE", "Sweden" },
                    { new Guid("c0000000-0000-0000-0000-000000000015"), "NO", "Norway" },
                    { new Guid("c0000000-0000-0000-0000-000000000016"), "GR", "Greece" },
                    { new Guid("c0000000-0000-0000-0000-000000000017"), "AT", "Austria" },
                    { new Guid("c0000000-0000-0000-0000-000000000018"), "BE", "Belgium" },
                    { new Guid("c0000000-0000-0000-0000-000000000019"), "PT", "Portugal" },
                    { new Guid("c0000000-0000-0000-0000-000000000020"), "IE", "Ireland" },
                    { new Guid("c0000000-0000-0000-0000-000000000021"), "CA", "Canada" },
                    { new Guid("c0000000-0000-0000-0000-000000000022"), "AU", "Australia" },
                    { new Guid("c0000000-0000-0000-0000-000000000023"), "NZ", "New Zealand" },
                    { new Guid("c0000000-0000-0000-0000-000000000024"), "JP", "Japan" },
                    { new Guid("c0000000-0000-0000-0000-000000000025"), "CN", "China" },
                    { new Guid("c0000000-0000-0000-0000-000000000026"), "IN", "India" },
                    { new Guid("c0000000-0000-0000-0000-000000000027"), "RU", "Russia" },
                    { new Guid("c0000000-0000-0000-0000-000000000028"), "BR", "Brazil" },
                    { new Guid("c0000000-0000-0000-0000-000000000029"), "ZA", "South Africa" },
                    { new Guid("c0000000-0000-0000-0000-000000000030"), "MY", "Malaysia" },
                    { new Guid("c0000000-0000-0000-0000-000000000031"), "TH", "Thailand" },
                    { new Guid("c0000000-0000-0000-0000-000000000032"), "SG", "Singapore" },
                    { new Guid("c0000000-0000-0000-0000-000000000033"), "ID", "Indonesia" },
                    { new Guid("c0000000-0000-0000-0000-000000000034"), "PH", "Philippines" },
                    { new Guid("c0000000-0000-0000-0000-000000000035"), "KR", "South Korea" },
                    { new Guid("c0000000-0000-0000-0000-000000000036"), "QA", "Qatar" },
                    { new Guid("c0000000-0000-0000-0000-000000000037"), "KW", "Kuwait" },
                    { new Guid("c0000000-0000-0000-0000-000000000038"), "BH", "Bahrain" },
                    { new Guid("c0000000-0000-0000-0000-000000000039"), "OM", "Oman" },
                    { new Guid("c0000000-0000-0000-0000-000000000040"), "JO", "Jordan" },
                    { new Guid("c0000000-0000-0000-0000-000000000041"), "LB", "Lebanon" },
                    { new Guid("c0000000-0000-0000-0000-000000000042"), "MA", "Morocco" },
                    { new Guid("c0000000-0000-0000-0000-000000000043"), "TN", "Tunisia" },
                    { new Guid("c0000000-0000-0000-0000-000000000044"), "PK", "Pakistan" },
                    { new Guid("c0000000-0000-0000-0000-000000000045"), "DK", "Denmark" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_countries_code",
                table: "countries",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_countries_name",
                table: "countries",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plan_benefits_plan_id",
                table: "plan_benefits",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_plan_countries_country_id",
                table: "plan_countries",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "IX_plan_countries_plan_id_country_id",
                table: "plan_countries",
                columns: new[] { "plan_id", "country_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plan_coverages_plan_id",
                table: "plan_coverages",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_plans_company_id",
                table: "plans",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "IX_plans_is_active",
                table: "plans",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_pricing_rules_plan_id",
                table: "pricing_rules",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_pricing_rules_plan_id_min_age_max_age_duration_from_duratio~",
                table: "pricing_rules",
                columns: new[] { "plan_id", "min_age", "max_age", "duration_from", "duration_to" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "plan_benefits");

            migrationBuilder.DropTable(
                name: "plan_countries");

            migrationBuilder.DropTable(
                name: "plan_coverages");

            migrationBuilder.DropTable(
                name: "pricing_rules");

            migrationBuilder.DropTable(
                name: "countries");

            migrationBuilder.DropTable(
                name: "plans");
        }
    }
}
