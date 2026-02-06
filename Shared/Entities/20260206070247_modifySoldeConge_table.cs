using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Entities
{
    /// <inheritdoc />
    public partial class modifySoldeConge_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Month",
                table: "SoldeConges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "SoldeConges",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Month",
                table: "SoldeConges");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "SoldeConges");
        }
    }
}
