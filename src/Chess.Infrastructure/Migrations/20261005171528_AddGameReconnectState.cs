using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chess.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGameReconnectState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChessGames_GameCodes_GameCodeCode",
                table: "ChessGames");

            migrationBuilder.DropIndex(
                name: "IX_ChessGames_GameCodeCode",
                table: "ChessGames");

            migrationBuilder.DropColumn(
                name: "GameCodeCode",
                table: "ChessGames");

            migrationBuilder.AddColumn<string>(
                name: "DisconnectedPlayerId",
                table: "ChessGames",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReconnectDeadline",
                table: "ChessGames",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisconnectedPlayerId",
                table: "ChessGames");

            migrationBuilder.DropColumn(
                name: "ReconnectDeadline",
                table: "ChessGames");

            migrationBuilder.AddColumn<string>(
                name: "GameCodeCode",
                table: "ChessGames",
                type: "nvarchar(6)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChessGames_GameCodeCode",
                table: "ChessGames",
                column: "GameCodeCode");

            migrationBuilder.AddForeignKey(
                name: "FK_ChessGames_GameCodes_GameCodeCode",
                table: "ChessGames",
                column: "GameCodeCode",
                principalTable: "GameCodes",
                principalColumn: "Code");
        }
    }
}
