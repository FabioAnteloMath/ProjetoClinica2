using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projeto_Clínica.Migrations
{
    /// <inheritdoc />
    public partial class alter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MedicId",
                table: "Medicos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MedicId",
                table: "Medicos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
