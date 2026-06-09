using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Turnos.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMercadoPagoFieldsToPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "external_reference",
                table: "payments",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "mercado_pago_payment_id",
                table: "payments",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "preference_id",
                table: "payments",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_payments_external_reference",
                table: "payments",
                column: "external_reference");

            migrationBuilder.CreateIndex(
                name: "ix_payments_preference_id",
                table: "payments",
                column: "preference_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_payments_external_reference",
                table: "payments");

            migrationBuilder.DropIndex(
                name: "ix_payments_preference_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "external_reference",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "mercado_pago_payment_id",
                table: "payments");

            migrationBuilder.DropColumn(
                name: "preference_id",
                table: "payments");
        }
    }
}
