using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoLivingApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChatModeration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Users_SenderId",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_ApartmentId",
                table: "ChatMessages");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "ChatMessages",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "ChatMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ChatMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ChatBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BlockerId = table.Column<string>(type: "text", nullable: false),
                    BlockedUserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatBlocks_Users_BlockedUserId",
                        column: x => x.BlockedUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatBlocks_Users_BlockerId",
                        column: x => x.BlockerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessageReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterId = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessageReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessageReports_ChatMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "ChatMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatMessageReports_Users_ReporterId",
                        column: x => x.ReporterId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ApartmentId_SentAt",
                table: "ChatMessages",
                columns: new[] { "ApartmentId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatBlocks_BlockedUserId",
                table: "ChatBlocks",
                column: "BlockedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatBlocks_BlockerId_BlockedUserId",
                table: "ChatBlocks",
                columns: new[] { "BlockerId", "BlockedUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessageReports_MessageId_ReporterId",
                table: "ChatMessageReports",
                columns: new[] { "MessageId", "ReporterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessageReports_ReporterId",
                table: "ChatMessageReports",
                column: "ReporterId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Users_SenderId",
                table: "ChatMessages",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Users_SenderId",
                table: "ChatMessages");

            migrationBuilder.DropTable(
                name: "ChatBlocks");

            migrationBuilder.DropTable(
                name: "ChatMessageReports");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_ApartmentId_SentAt",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ChatMessages");

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "ChatMessages",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ApartmentId",
                table: "ChatMessages",
                column: "ApartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Users_SenderId",
                table: "ChatMessages",
                column: "SenderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
