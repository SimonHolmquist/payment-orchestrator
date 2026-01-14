using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentOrchestrator.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class M2_ReliabilityFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IdempotencyKeys",
                table: "IdempotencyKeys");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "InboxMessages");

            migrationBuilder.RenameColumn(
                name: "ProcessedAt",
                table: "OutboxMessages",
                newName: "PublishedAt");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "InboxMessages",
                newName: "RawPayload");

            migrationBuilder.RenameColumn(
                name: "OccurredAt",
                table: "InboxMessages",
                newName: "ReceivedAt");

            migrationBuilder.Sql(@"
        UPDATE Payments SET Status = CASE Status
            WHEN 'Created' THEN '0'
            WHEN 'AuthorizationRequested' THEN '10'
            WHEN 'CaptureRequested' THEN '11'
            WHEN 'RefundRequested' THEN '12'
            WHEN 'CancelRequested' THEN '13'
            WHEN 'Authorized' THEN '20'
            WHEN 'PartiallyCaptured' THEN '30'
            WHEN 'Captured' THEN '31'
            WHEN 'PartiallyRefunded' THEN '40'
            WHEN 'Refunded' THEN '41'
            WHEN 'Cancelled' THEN '50'
            WHEN 'Failed' THEN '60'
            WHEN 'Unknown' THEN '70'
            ELSE '70' -- Default a Unknown por seguridad
        END
    ");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Payments",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PspReference",
                table: "Payments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FailureReason",
                table: "Payments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "OutboxMessages",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "AggregateId",
                table: "OutboxMessages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "AggregateType",
                table: "OutboxMessages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Attempts",
                table: "OutboxMessages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "OutboxMessages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "InboxMessages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "InboxMessages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProviderEventId",
                table: "InboxMessages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "IdempotencyKeys",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CompletedAt",
                table: "IdempotencyKeys",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                table: "IdempotencyKeys",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResponseBody",
                table: "IdempotencyKeys",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponseContentType",
                table: "IdempotencyKeys",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResponseStatusCode",
                table: "IdempotencyKeys",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdempotencyKeys",
                table: "IdempotencyKeys",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_PublishedAt",
                table: "OutboxMessages",
                column: "PublishedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_Provider_ProviderEventId",
                table: "InboxMessages",
                columns: new[] { "Provider", "ProviderEventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IdempotencyKeys_ClientId_Key_OperationName",
                table: "IdempotencyKeys",
                columns: new[] { "ClientId", "Key", "OperationName" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_PublishedAt",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_InboxMessages_Provider_ProviderEventId",
                table: "InboxMessages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IdempotencyKeys",
                table: "IdempotencyKeys");

            migrationBuilder.DropIndex(
                name: "IX_IdempotencyKeys_ClientId_Key_OperationName",
                table: "IdempotencyKeys");

            migrationBuilder.DropColumn(
                name: "AggregateId",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "AggregateType",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "Attempts",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "EventType",
                table: "InboxMessages");

            migrationBuilder.DropColumn(
                name: "Provider",
                table: "InboxMessages");

            migrationBuilder.DropColumn(
                name: "ProviderEventId",
                table: "InboxMessages");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "IdempotencyKeys");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "IdempotencyKeys");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                table: "IdempotencyKeys");

            migrationBuilder.DropColumn(
                name: "ResponseBody",
                table: "IdempotencyKeys");

            migrationBuilder.DropColumn(
                name: "ResponseContentType",
                table: "IdempotencyKeys");

            migrationBuilder.DropColumn(
                name: "ResponseStatusCode",
                table: "IdempotencyKeys");

            migrationBuilder.RenameColumn(
                name: "PublishedAt",
                table: "OutboxMessages",
                newName: "ProcessedAt");

            migrationBuilder.RenameColumn(
                name: "ReceivedAt",
                table: "InboxMessages",
                newName: "OccurredAt");

            migrationBuilder.RenameColumn(
                name: "RawPayload",
                table: "InboxMessages",
                newName: "Type");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "PspReference",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FailureReason",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "OutboxMessages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(300)",
                oldMaxLength: 300);

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "InboxMessages",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdempotencyKeys",
                table: "IdempotencyKeys",
                column: "Key");
        }
    }
}
