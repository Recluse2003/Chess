using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chess.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMoveNotation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notation",
                table: "Moves",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notation",
                table: "Moves");
        }
    }
}
