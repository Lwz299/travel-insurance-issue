using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TravelInsurance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PaymentGatewayCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_payments_target",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "confirmed_at",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "reference",
                table: "payments");

            migrationBuilder.RenameColumn(
                name: "paid_at",
                table: "payments",
                newName: "completed_at");

            migrationBuilder.RenameColumn(
                name: "confirmed_by_user_id",
                table: "payments",
                newName: "created_by_user_id");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "payments",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "method",
                table: "payments",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "business_reference",
                table: "payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "paid_amount",
                table: "payments",
                type: "numeric(14,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "purpose",
                table: "payments",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reference_number",
                table: "payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE payments
                SET
                    reference_number = 'LEGACY-' || replace("Id"::text, '-', ''),
                    business_reference = 'LEGACY-' || replace("Id"::text, '-', ''),
                    purpose = 'PolicyIssuance',
                    paid_amount = CASE WHEN status = 'Paid' THEN amount ELSE 0 END;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "business_reference",
                table: "payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "purpose",
                table: "payments",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "reference_number",
                table: "payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "payment_attempts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    provider_reference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    idempotency_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    checkout_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    failure_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    failure_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    safe_metadata = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_attempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payment_attempts_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_payments_reference_number",
                table: "payments",
                column: "reference_number",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_payments_single_target",
                table: "payments",
                sql: "(quote_id IS NOT NULL AND policy_id IS NULL) OR (quote_id IS NULL AND policy_id IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_payment_attempts_payment_id",
                table: "payment_attempts",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_attempts_provider_code_idempotency_key",
                table: "payment_attempts",
                columns: new[] { "provider_code", "idempotency_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payment_attempts_provider_code_provider_reference",
                table: "payment_attempts",
                columns: new[] { "provider_code", "provider_reference" },
                unique: true,
                filter: "provider_reference IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "payment_attempts");

            migrationBuilder.DropIndex(
                name: "IX_payments_reference_number",
                table: "payments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_payments_single_target",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "business_reference",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "paid_amount",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "purpose",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "reference_number",
                table: "payments");

            migrationBuilder.RenameColumn(
                name: "created_by_user_id",
                table: "payments",
                newName: "confirmed_by_user_id");

            migrationBuilder.RenameColumn(
                name: "completed_at",
                table: "payments",
                newName: "paid_at");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "payments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "method",
                table: "payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "confirmed_at",
                table: "payments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reference",
                table: "payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_payments_target",
                table: "payments",
                sql: "quote_id IS NOT NULL OR policy_id IS NOT NULL");
        }
    }
}
