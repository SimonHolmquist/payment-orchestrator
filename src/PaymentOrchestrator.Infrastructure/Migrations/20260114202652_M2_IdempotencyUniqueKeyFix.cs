using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentOrchestrator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class M2_IdempotencyUniqueKeyFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IdempotencyKeys_ClientId_Key_OperationName",
                table: "IdempotencyKeys");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyKeys_ClientId_Key",
                table: "IdempotencyKeys",
                columns: new[] { "ClientId", "Key" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IdempotencyKeys_ClientId_Key",
                table: "IdempotencyKeys");

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyKeys_ClientId_Key_OperationName",
                table: "IdempotencyKeys",
                columns: new[] { "ClientId", "Key", "OperationName" },
                unique: true);
        }
    }
}
