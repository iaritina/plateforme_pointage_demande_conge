using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Entities
{
    /// <inheritdoc />
    public partial class AddDecisionYearToDemandeConge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "decisionYear",
                table: "DemandeConges",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "decisionYear",
                table: "DemandeConges");
        }
    }
}
