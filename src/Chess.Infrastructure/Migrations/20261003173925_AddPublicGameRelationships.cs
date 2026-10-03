using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chess.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicGameRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WhitePlayerId",
                table: "ChessGames",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BlackPlayerId",
                table: "ChessGames",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GameCodeCode",
                table: "ChessGames",
                type: "nvarchar(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrivate",
                table: "ChessGames",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ChessGames_BlackPlayerId",
                table: "ChessGames",
                column: "BlackPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChessGames_GameCodeCode",
                table: "ChessGames",
                column: "GameCodeCode");

            migrationBuilder.CreateIndex(
                name: "IX_ChessGames_WhitePlayerId",
                table: "ChessGames",
                column: "WhitePlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChessGames_AspNetUsers_BlackPlayerId",
                table: "ChessGames",
                column: "BlackPlayerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChessGames_AspNetUsers_WhitePlayerId",
                table: "ChessGames",
                column: "WhitePlayerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ChessGames_GameCodes_GameCodeCode",
                table: "ChessGames",
                column: "GameCodeCode",
                principalTable: "GameCodes",
                principalColumn: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChessGames_AspNetUsers_BlackPlayerId",
                table: "ChessGames");

            migrationBuilder.DropForeignKey(
                name: "FK_ChessGames_AspNetUsers_WhitePlayerId",
                table: "ChessGames");

            migrationBuilder.DropForeignKey(
                name: "FK_ChessGames_GameCodes_GameCodeCode",
                table: "ChessGames");

            migrationBuilder.DropIndex(
                name: "IX_ChessGames_BlackPlayerId",
                table: "ChessGames");

            migrationBuilder.DropIndex(
                name: "IX_ChessGames_GameCodeCode",
                table: "ChessGames");

            migrationBuilder.DropIndex(
                name: "IX_ChessGames_WhitePlayerId",
                table: "ChessGames");

            migrationBuilder.DropColumn(
                name: "GameCodeCode",
                table: "ChessGames");

            migrationBuilder.DropColumn(
                name: "IsPrivate",
                table: "ChessGames");

            migrationBuilder.AlterColumn<string>(
                name: "WhitePlayerId",
                table: "ChessGames",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BlackPlayerId",
                table: "ChessGames",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
