using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TravelInsurance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AgencyFundingAndSettlements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agency_credit_accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    agency_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    credit_limit = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    outstanding_amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    reserved_amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    billing_day = table.Column<int>(type: "integer", nullable: false),
                    payment_due_days = table.Column<int>(type: "integer", nullable: false),
                    grace_period_days = table.Column<int>(type: "integer", nullable: false),
                    auto_suspend_on_overdue = table.Column<bool>(type: "boolean", nullable: false),
                    is_suspended = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agency_credit_accounts", x => x.Id);
                    table.CheckConstraint("CK_agency_credit_accounts_amounts", "credit_limit >= 0 AND outstanding_amount >= 0 AND reserved_amount >= 0");
                    table.ForeignKey(
                        name: "FK_agency_credit_accounts_companies_agency_company_id",
                        column: x => x.agency_company_id,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "agency_funding_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    agency_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    maximum_transaction_amount = table.Column<decimal>(type: "numeric(14,2)", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agency_funding_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agency_funding_profiles_companies_agency_company_id",
                        column: x => x.agency_company_id,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "agency_settlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    agency_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    settlement_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    period_from = table.Column<DateOnly>(type: "date", nullable: false),
                    period_to = table.Column<DateOnly>(type: "date", nullable: false),
                    gross_premium = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    agency_commission = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    refunds = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    net_amount_due = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    paid_amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    remaining_amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IssuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agency_settlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agency_settlements_companies_agency_company_id",
                        column: x => x.agency_company_id,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "agency_wallets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    agency_company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    balance = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    reserved_balance = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agency_wallets", x => x.Id);
                    table.CheckConstraint("CK_agency_wallets_balances", "balance >= 0 AND reserved_balance >= 0 AND reserved_balance <= balance");
                    table.ForeignKey(
                        name: "FK_agency_wallets_companies_agency_company_id",
                        column: x => x.agency_company_id,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "policy_fundings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    issuance_request_id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: true),
                    agency_company_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    funding_mode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    funding_reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    FundedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReleasedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_policy_fundings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_policy_fundings_companies_agency_company_id",
                        column: x => x.agency_company_id,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_policy_fundings_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_policy_fundings_policies_policy_id",
                        column: x => x.policy_id,
                        principalTable: "policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_policy_fundings_policy_issuance_requests_issuance_request_id",
                        column: x => x.issuance_request_id,
                        principalTable: "policy_issuance_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "agency_credit_transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    credit_account_id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: true),
                    issuance_request_id = table.Column<Guid>(type: "uuid", nullable: true),
                    settlement_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    outstanding_before = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    outstanding_after = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agency_credit_transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_agency_credit_transactions_agency_credit_accounts_credit_ac~",
                        column: x => x.credit_account_id,
                        principalTable: "agency_credit_accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_agency_credit_transactions_agency_settlements_settlement_id",
                        column: x => x.settlement_id,
                        principalTable: "agency_settlements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_agency_credit_transactions_policies_policy_id",
                        column: x => x.policy_id,
                        principalTable: "policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_agency_credit_transactions_policy_issuance_requests_issuanc~",
                        column: x => x.issuance_request_id,
                        principalTable: "policy_issuance_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "wallet_transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    wallet_id = table.Column<Guid>(type: "uuid", nullable: false),
                    policy_id = table.Column<Guid>(type: "uuid", nullable: true),
                    issuance_request_id = table.Column<Guid>(type: "uuid", nullable: true),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    balance_before = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    balance_after = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallet_transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_wallet_transactions_agency_wallets_wallet_id",
                        column: x => x.wallet_id,
                        principalTable: "agency_wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_wallet_transactions_policies_policy_id",
                        column: x => x.policy_id,
                        principalTable: "policies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_wallet_transactions_policy_issuance_requests_issuance_reque~",
                        column: x => x.issuance_request_id,
                        principalTable: "policy_issuance_requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "Id", "description", "module", "name" },
                values: new object[] { 23, "Manage agency wallets, credit and settlements", "Agency Finance", "AGENCY.FINANCE.MANAGE" });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "permission_id", "role_id" },
                values: new object[,]
                {
                    { 23, 1 },
                    { 23, 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_agency_credit_accounts_agency_company_id_currency",
                table: "agency_credit_accounts",
                columns: new[] { "agency_company_id", "currency" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agency_credit_transactions_credit_account_id",
                table: "agency_credit_transactions",
                column: "credit_account_id");

            migrationBuilder.CreateIndex(
                name: "IX_agency_credit_transactions_issuance_request_id",
                table: "agency_credit_transactions",
                column: "issuance_request_id");

            migrationBuilder.CreateIndex(
                name: "IX_agency_credit_transactions_policy_id",
                table: "agency_credit_transactions",
                column: "policy_id");

            migrationBuilder.CreateIndex(
                name: "IX_agency_credit_transactions_reference",
                table: "agency_credit_transactions",
                column: "reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agency_credit_transactions_settlement_id",
                table: "agency_credit_transactions",
                column: "settlement_id");

            migrationBuilder.CreateIndex(
                name: "IX_agency_funding_profiles_agency_company_id_is_default",
                table: "agency_funding_profiles",
                columns: new[] { "agency_company_id", "is_default" },
                unique: true,
                filter: "is_default = true");

            migrationBuilder.CreateIndex(
                name: "IX_agency_funding_profiles_agency_company_id_mode",
                table: "agency_funding_profiles",
                columns: new[] { "agency_company_id", "mode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agency_settlements_agency_company_id_currency_period_from_p~",
                table: "agency_settlements",
                columns: new[] { "agency_company_id", "currency", "period_from", "period_to" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agency_settlements_settlement_number",
                table: "agency_settlements",
                column: "settlement_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_agency_wallets_agency_company_id_currency",
                table: "agency_wallets",
                columns: new[] { "agency_company_id", "currency" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_policy_fundings_agency_company_id",
                table: "policy_fundings",
                column: "agency_company_id");

            migrationBuilder.CreateIndex(
                name: "IX_policy_fundings_funding_reference",
                table: "policy_fundings",
                column: "funding_reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_policy_fundings_issuance_request_id",
                table: "policy_fundings",
                column: "issuance_request_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_policy_fundings_payment_id",
                table: "policy_fundings",
                column: "payment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_policy_fundings_policy_id",
                table: "policy_fundings",
                column: "policy_id");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transactions_issuance_request_id",
                table: "wallet_transactions",
                column: "issuance_request_id");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transactions_policy_id",
                table: "wallet_transactions",
                column: "policy_id");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transactions_reference",
                table: "wallet_transactions",
                column: "reference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transactions_wallet_id",
                table: "wallet_transactions",
                column: "wallet_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agency_credit_transactions");

            migrationBuilder.DropTable(
                name: "agency_funding_profiles");

            migrationBuilder.DropTable(
                name: "policy_fundings");

            migrationBuilder.DropTable(
                name: "wallet_transactions");

            migrationBuilder.DropTable(
                name: "agency_credit_accounts");

            migrationBuilder.DropTable(
                name: "agency_settlements");

            migrationBuilder.DropTable(
                name: "agency_wallets");

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { 23, 1 });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { 23, 5 });

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: 23);
        }
    }
}
