using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelInsurance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReinsuranceFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "destination_country_id",
                table: "policies",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "reinsurance_agreements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reinsurer_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agreement_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    agreement_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    agreement_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: false),
                    reporting_mode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    reporting_frequency = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    requires_pre_approval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    ceded_share_percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    max_trip_duration_days = table.Column<int>(type: "integer", nullable: true),
                    max_coverage_limit = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    min_age = table.Column<int>(type: "integer", nullable: true),
                    max_age = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reinsurance_agreements", x => x.Id);
                    table.CheckConstraint("CK_reinsurance_agreements_ceded_share", "ceded_share_percentage > 0 AND ceded_share_percentage <= 100");
                    table.CheckConstraint("CK_reinsurance_agreements_dates", "effective_to >= effective_from");
                    table.ForeignKey(
                        name: "FK_reinsurance_agreements_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reinsurance_agreements_companies_reinsurer_company_id",
                        column: x => x.reinsurer_company_id,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "plan_reinsurance_agreements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reinsurance_agreement_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan_reinsurance_agreements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_plan_reinsurance_agreements_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_plan_reinsurance_agreements_reinsurance_agreements_reinsura~",
                        column: x => x.reinsurance_agreement_id,
                        principalTable: "reinsurance_agreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reinsurance_agreement_coverage_zones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    reinsurance_agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    coverage_zone_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reinsurance_agreement_coverage_zones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reinsurance_agreement_coverage_zones_coverage_zones_coverag~",
                        column: x => x.coverage_zone_id,
                        principalTable: "coverage_zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reinsurance_agreement_coverage_zones_reinsurance_agreements~",
                        column: x => x.reinsurance_agreement_id,
                        principalTable: "reinsurance_agreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reinsurance_bordereau_batches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    reinsurance_agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    batch_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    period_from = table.Column<DateOnly>(type: "date", nullable: false),
                    period_to = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    generated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    finalized_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reinsurance_bordereau_batches", x => x.Id);
                    table.CheckConstraint("CK_reinsurance_bordereau_batches_dates", "period_to >= period_from");
                    table.ForeignKey(
                        name: "FK_reinsurance_bordereau_batches_reinsurance_agreements_reinsu~",
                        column: x => x.reinsurance_agreement_id,
                        principalTable: "reinsurance_agreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reinsurance_cessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reinsurance_agreement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reinsurer_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bordereau_batch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    agreement_number_snapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    gross_premium = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    ceded_premium = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    gross_sum_insured = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    ceded_sum_insured = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    ceded_share_percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    company_retention_percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reinsurance_cessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reinsurance_cessions_companies_reinsurer_company_id",
                        column: x => x.reinsurer_company_id,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reinsurance_cessions_policies_policy_id",
                        column: x => x.policy_id,
                        principalTable: "policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_reinsurance_cessions_reinsurance_agreements_reinsurance_agr~",
                        column: x => x.reinsurance_agreement_id,
                        principalTable: "reinsurance_agreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reinsurance_cessions_reinsurance_bordereau_batches_borderea~",
                        column: x => x.bordereau_batch_id,
                        principalTable: "reinsurance_bordereau_batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_policies_destination_country_id",
                table: "policies",
                column: "destination_country_id");

            migrationBuilder.CreateIndex(
                name: "IX_plan_reinsurance_agreements_plan_id_reinsurance_agreement_id",
                table: "plan_reinsurance_agreements",
                columns: new[] { "plan_id", "reinsurance_agreement_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_plan_reinsurance_agreements_reinsurance_agreement_id",
                table: "plan_reinsurance_agreements",
                column: "reinsurance_agreement_id");

            migrationBuilder.CreateIndex(
                name: "IX_reinsurance_agreement_coverage_zones_coverage_zone_id",
                table: "reinsurance_agreement_coverage_zones",
                column: "coverage_zone_id");

            migrationBuilder.CreateIndex(
                name: "IX_reinsurance_agreement_coverage_zones_reinsurance_agreement_~",
                table: "reinsurance_agreement_coverage_zones",
                columns: new[] { "reinsurance_agreement_id", "coverage_zone_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reinsurance_agreements_company_id_agreement_number",
                table: "reinsurance_agreements",
                columns: new[] { "company_id", "agreement_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reinsurance_agreements_company_id_is_active",
                table: "reinsurance_agreements",
                columns: new[] { "company_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_reinsurance_agreements_reinsurer_company_id",
                table: "reinsurance_agreements",
                column: "reinsurer_company_id");

            migrationBuilder.CreateIndex(
                name: "IX_reinsurance_bordereau_batches_batch_number",
                table: "reinsurance_bordereau_batches",
                column: "batch_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reinsurance_bordereau_batches_reinsurance_agreement_id_peri~",
                table: "reinsurance_bordereau_batches",
                columns: new[] { "reinsurance_agreement_id", "period_from", "period_to" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reinsurance_cessions_bordereau_batch_id",
                table: "reinsurance_cessions",
                column: "bordereau_batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_reinsurance_cessions_policy_id",
                table: "reinsurance_cessions",
                column: "policy_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reinsurance_cessions_reinsurance_agreement_id_status",
                table: "reinsurance_cessions",
                columns: new[] { "reinsurance_agreement_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_reinsurance_cessions_reinsurer_company_id",
                table: "reinsurance_cessions",
                column: "reinsurer_company_id");

            migrationBuilder.AddForeignKey(
                name: "FK_policies_countries_destination_country_id",
                table: "policies",
                column: "destination_country_id",
                principalTable: "countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_policies_countries_destination_country_id",
                table: "policies");

            migrationBuilder.DropTable(
                name: "plan_reinsurance_agreements");

            migrationBuilder.DropTable(
                name: "reinsurance_agreement_coverage_zones");

            migrationBuilder.DropTable(
                name: "reinsurance_cessions");

            migrationBuilder.DropTable(
                name: "reinsurance_bordereau_batches");

            migrationBuilder.DropTable(
                name: "reinsurance_agreements");

            migrationBuilder.DropIndex(
                name: "IX_policies_destination_country_id",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "destination_country_id",
                table: "policies");
        }
    }
}
