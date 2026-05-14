using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoLivingApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChatScopeBuildingEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ApartmentId",
                table: "ChatMessages",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "BuildingId",
                table: "ChatMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                table: "ChatMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Scope",
                table: "ChatMessages",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_BuildingId_SentAt",
                table: "ChatMessages",
                columns: new[] { "BuildingId", "SentAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_EventId_SentAt",
                table: "ChatMessages",
                columns: new[] { "EventId", "SentAt" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ChatMessages_ScopeRef",
                table: "ChatMessages",
                sql: "(\"Scope\" = 1 AND \"ApartmentId\" IS NOT NULL AND \"BuildingId\" IS NULL AND \"EventId\" IS NULL) OR (\"Scope\" = 2 AND \"ApartmentId\" IS NULL AND \"BuildingId\" IS NOT NULL AND \"EventId\" IS NULL) OR (\"Scope\" = 3 AND \"ApartmentId\" IS NULL AND \"BuildingId\" IS NULL AND \"EventId\" IS NOT NULL)");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Buildings_BuildingId",
                table: "ChatMessages",
                column: "BuildingId",
                principalTable: "Buildings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Buildings_BuildingId",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_BuildingId_SentAt",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_EventId_SentAt",
                table: "ChatMessages");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ChatMessages_ScopeRef",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "BuildingId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "Scope",
                table: "ChatMessages");

            migrationBuilder.AlterColumn<Guid>(
                name: "ApartmentId",
                table: "ChatMessages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
