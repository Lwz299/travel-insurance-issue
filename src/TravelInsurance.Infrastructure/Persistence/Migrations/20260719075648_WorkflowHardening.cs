using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TravelInsurance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cancellation_reason",
                table: "quotes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "cancelled_at",
                table: "quotes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "cancelled_by_user_id",
                table: "quotes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "owner_company_id",
                table: "quotes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "sent_at",
                table: "quotes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cancellation_reason",
                table: "policies",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "cancelled_at",
                table: "policies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "cancelled_by_user_id",
                table: "policies",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "idempotency_key",
                table: "policies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "confirmed_at",
                table: "payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "confirmed_by_user_id",
                table: "payments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "idempotency_key",
                table: "payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "policy_benefit_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_coverage_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    benefit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    benefit_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    benefit_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    limit_type_id = table.Column<int>(type: "integer", nullable: false),
                    limit_value = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    deductible = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    waiting_period_days = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policy_benefit_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_policy_benefit_snapshots_policy_coverage_snapshots_policy_c~",
                        column: x => x.policy_coverage_snapshot_id,
                        principalTable: "policy_coverage_snapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quote_benefit_snapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    quote_coverage_snapshot_id = table.Column<Guid>(type: "uuid", nullable: false),
                    benefit_id = table.Column<Guid>(type: "uuid", nullable: false),
                    benefit_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    benefit_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    limit_type_id = table.Column<int>(type: "integer", nullable: false),
                    limit_value = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    deductible = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    waiting_period_days = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quote_benefit_snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_quote_benefit_snapshots_quote_coverage_snapshots_quote_cove~",
                        column: x => x.quote_coverage_snapshot_id,
                        principalTable: "quote_coverage_snapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "Id", "description", "module", "name" },
                values: new object[,]
                {
                    { 17, "Send quotations", "Quote", "QUOTE.SEND" },
                    { 18, "Cancel quotations", "Quote", "QUOTE.CANCEL" },
                    { 19, "Confirm payments from trusted payment processing", "Payment", "PAYMENT.CONFIRM" },
                    { 20, "Cancel policies", "Policy", "POLICY.CANCEL" },
                    { 21, "Expire elapsed policies", "Policy", "POLICY.EXPIRE" }
                });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "permission_id", "role_id" },
                values: new object[,]
                {
                    { 17, 1 },
                    { 18, 1 },
                    { 19, 1 },
                    { 20, 1 },
                    { 21, 1 },
                    { 17, 3 },
                    { 18, 3 },
                    { 17, 5 },
                    { 18, 5 },
                    { 19, 5 },
                    { 20, 5 },
                    { 21, 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_quotes_owner_company_id",
                table: "quotes",
                column: "owner_company_id");

            migrationBuilder.CreateIndex(
                name: "IX_policies_idempotency_key",
                table: "policies",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_idempotency_key",
                table: "payments",
                column: "idempotency_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_policy_benefit_snapshots_policy_coverage_snapshot_id",
                table: "policy_benefit_snapshots",
                column: "policy_coverage_snapshot_id");

            migrationBuilder.CreateIndex(
                name: "IX_quote_benefit_snapshots_quote_coverage_snapshot_id",
                table: "quote_benefit_snapshots",
                column: "quote_coverage_snapshot_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "policy_benefit_snapshots");

            migrationBuilder.DropTable(
                name: "quote_benefit_snapshots");

            migrationBuilder.DropIndex(
                name: "IX_quotes_owner_company_id",
                table: "quotes");

            migrationBuilder.DropIndex(
                name: "IX_policies_idempotency_key",
                table: "policies");

            migrationBuilder.DropIndex(
                name: "IX_payments_idempotency_key",
                table: "payments");

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { 17, 1 });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { 18, 1 });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { 19, 1 });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { 20, 1 });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { 21, 1 });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { 17, 3 });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { 18, 3 });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { 17, 5 });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { 18, 5 });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { 19, 5 });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { 20, 5 });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { 21, 5 });

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DropColumn(
                name: "cancellation_reason",
                table: "quotes");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                table: "quotes");

            migrationBuilder.DropColumn(
                name: "cancelled_by_user_id",
                table: "quotes");

            migrationBuilder.DropColumn(
                name: "owner_company_id",
                table: "quotes");

            migrationBuilder.DropColumn(
                name: "sent_at",
                table: "quotes");

            migrationBuilder.DropColumn(
                name: "cancellation_reason",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "cancelled_at",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "cancelled_by_user_id",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "idempotency_key",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "confirmed_at",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "confirmed_by_user_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "idempotency_key",
                table: "payments");
        }
    }
}
