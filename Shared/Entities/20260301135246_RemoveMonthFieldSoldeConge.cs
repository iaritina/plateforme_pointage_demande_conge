using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Entities
{
    /// <inheritdoc />
    public partial class RemoveMonthFieldSoldeConge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Month",
                table: "SoldeConges");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Month",
                table: "SoldeConges",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
