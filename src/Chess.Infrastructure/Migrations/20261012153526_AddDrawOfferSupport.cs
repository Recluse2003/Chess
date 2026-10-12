using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chess.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDrawOfferSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DrawOfferedByPlayerId",
                table: "ChessGames",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DrawOfferedByPlayerId",
                table: "ChessGames");
        }
    }
}
