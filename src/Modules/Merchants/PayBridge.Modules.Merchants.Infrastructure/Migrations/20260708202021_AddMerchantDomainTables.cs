using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PayBridge.Modules.Merchants.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMerchantDomainTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "merchants");

            migrationBuilder.CreateTable(
                name: "MerchantCategoryCodes",
                schema: "merchants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsRestricted = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeactivatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantCategoryCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MerchantSectors",
                schema: "merchants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsHighRisk = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeactivatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantSectors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Merchants",
                schema: "merchants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    LegalName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    TaxNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    TaxOffice = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    SectorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MccId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActivatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SuspendedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClosedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Merchants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Merchants_MerchantCategoryCodes_MccId",
                        column: x => x.MccId,
                        principalSchema: "merchants",
                        principalTable: "MerchantCategoryCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Merchants_MerchantSectors_SectorId",
                        column: x => x.SectorId,
                        principalSchema: "merchants",
                        principalTable: "MerchantSectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MerchantPaymentChannelSettings",
                schema: "merchants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    MinAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DailyAmountLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Require3DS = table.Column<bool>(type: "bit", nullable: false),
                    AllowRefund = table.Column<bool>(type: "bit", nullable: false),
                    AllowVoid = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EnabledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisabledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantPaymentChannelSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantPaymentChannelSettings_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalSchema: "merchants",
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MerchantProviderAccounts",
                schema: "merchants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProviderCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AllowECommerce = table.Column<bool>(type: "bit", nullable: false),
                    AllowPhysicalPos = table.Column<bool>(type: "bit", nullable: false),
                    AllowRefund = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActivatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeactivatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantProviderAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantProviderAccounts_Merchants_MerchantId",
                        column: x => x.MerchantId,
                        principalSchema: "merchants",
                        principalTable: "Merchants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MerchantProviderCredentials",
                schema: "merchants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MerchantProviderAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EncryptedCredentialPayload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EncryptedKeyVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RotatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MerchantProviderCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MerchantProviderCredentials_MerchantProviderAccounts_MerchantProviderAccountId",
                        column: x => x.MerchantProviderAccountId,
                        principalSchema: "merchants",
                        principalTable: "MerchantProviderAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MerchantCategoryCodes_Code",
                schema: "merchants",
                table: "MerchantCategoryCodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MerchantCategoryCodes_IsActive",
                schema: "merchants",
                table: "MerchantCategoryCodes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantCategoryCodes_IsRestricted",
                schema: "merchants",
                table: "MerchantCategoryCodes",
                column: "IsRestricted");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantPaymentChannelSettings_IsEnabled",
                schema: "merchants",
                table: "MerchantPaymentChannelSettings",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantPaymentChannelSettings_MerchantId_Channel",
                schema: "merchants",
                table: "MerchantPaymentChannelSettings",
                columns: new[] { "MerchantId", "Channel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MerchantProviderAccounts_IsActive",
                schema: "merchants",
                table: "MerchantProviderAccounts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantProviderAccounts_MerchantId_ProviderCode",
                schema: "merchants",
                table: "MerchantProviderAccounts",
                columns: new[] { "MerchantId", "ProviderCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MerchantProviderAccounts_Priority",
                schema: "merchants",
                table: "MerchantProviderAccounts",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantProviderAccounts_ProviderCode",
                schema: "merchants",
                table: "MerchantProviderAccounts",
                column: "ProviderCode");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantProviderCredentials_IsActive",
                schema: "merchants",
                table: "MerchantProviderCredentials",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantProviderCredentials_MerchantProviderAccountId",
                schema: "merchants",
                table: "MerchantProviderCredentials",
                column: "MerchantProviderAccountId",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_MccId",
                schema: "merchants",
                table: "Merchants",
                column: "MccId");

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_MerchantCode",
                schema: "merchants",
                table: "Merchants",
                column: "MerchantCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_SectorId",
                schema: "merchants",
                table: "Merchants",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_Merchants_Status",
                schema: "merchants",
                table: "Merchants",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MerchantSectors_Code",
                schema: "merchants",
                table: "MerchantSectors",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MerchantSectors_IsActive",
                schema: "merchants",
                table: "MerchantSectors",
                column: "IsActive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MerchantPaymentChannelSettings",
                schema: "merchants");

            migrationBuilder.DropTable(
                name: "MerchantProviderCredentials",
                schema: "merchants");

            migrationBuilder.DropTable(
                name: "MerchantProviderAccounts",
                schema: "merchants");

            migrationBuilder.DropTable(
                name: "Merchants",
                schema: "merchants");

            migrationBuilder.DropTable(
                name: "MerchantCategoryCodes",
                schema: "merchants");

            migrationBuilder.DropTable(
                name: "MerchantSectors",
                schema: "merchants");
        }
    }
}
