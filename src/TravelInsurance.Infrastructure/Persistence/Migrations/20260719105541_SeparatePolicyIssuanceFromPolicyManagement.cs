using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelInsurance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeparatePolicyIssuanceFromPolicyManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_payments_single_target",
                table: "payments");

            migrationBuilder.AddColumn<Guid>(
                name: "issuance_request_id",
                table: "policies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "issuance_request_id",
                table: "payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "policy_issuance_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    plan_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quote_id = table.Column<Guid>(type: "uuid", nullable: true),
                    issuance_source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    requested_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agency_company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    gross_premium = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                    effective_to = table.Column<DateOnly>(type: "date", nullable: false),
                    policy_draft_json = table.Column<string>(type: "text", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policy_issuance_requests", x => x.Id);
                    table.CheckConstraint("CK_issuance_request_source_quote", "(issuance_source = 'Quote' AND quote_id IS NOT NULL) OR (issuance_source = 'Direct' AND quote_id IS NULL)");
                    table.ForeignKey(
                        name: "FK_policy_issuance_requests_plans_plan_id",
                        column: x => x.plan_id,
                        principalTable: "plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_policy_issuance_requests_quotes_quote_id",
                        column: x => x.quote_id,
                        principalTable: "quotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_policies_issuance_request_id",
                table: "policies",
                column: "issuance_request_id",
                unique: true,
                filter: "issuance_request_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_payments_issuance_request_id",
                table: "payments",
                column: "issuance_request_id");

            migrationBuilder.AddCheckConstraint(
                name: "CK_payments_single_target",
                table: "payments",
                sql: "(quote_id IS NOT NULL AND policy_id IS NULL AND issuance_request_id IS NULL) OR (quote_id IS NULL AND policy_id IS NOT NULL AND issuance_request_id IS NULL) OR (quote_id IS NULL AND policy_id IS NULL AND issuance_request_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_policy_issuance_requests_idempotency_key",
                table: "policy_issuance_requests",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_policy_issuance_requests_plan_id",
                table: "policy_issuance_requests",
                column: "plan_id");

            migrationBuilder.CreateIndex(
                name: "IX_policy_issuance_requests_quote_id",
                table: "policy_issuance_requests",
                column: "quote_id",
                unique: true,
                filter: "quote_id IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_payments_policy_issuance_requests_issuance_request_id",
                table: "payments",
                column: "issuance_request_id",
                principalTable: "policy_issuance_requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_policies_policy_issuance_requests_issuance_request_id",
                table: "policies",
                column: "issuance_request_id",
                principalTable: "policy_issuance_requests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_payments_policy_issuance_requests_issuance_request_id",
                table: "payments");

            migrationBuilder.DropForeignKey(
                name: "FK_policies_policy_issuance_requests_issuance_request_id",
                table: "policies");

            migrationBuilder.DropTable(
                name: "policy_issuance_requests");

            migrationBuilder.DropIndex(
                name: "IX_policies_issuance_request_id",
                table: "policies");

            migrationBuilder.DropIndex(
                name: "IX_payments_issuance_request_id",
                table: "payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_payments_single_target",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "issuance_request_id",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "issuance_request_id",
                table: "payments");

            migrationBuilder.AddCheckConstraint(
                name: "CK_payments_single_target",
                table: "payments",
                sql: "(quote_id IS NOT NULL AND policy_id IS NULL) OR (quote_id IS NULL AND policy_id IS NOT NULL)");
        }
    }
}
