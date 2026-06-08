using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnos.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentFileCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "appointment_files",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "category",
                table: "appointment_files");
        }
    }
}
