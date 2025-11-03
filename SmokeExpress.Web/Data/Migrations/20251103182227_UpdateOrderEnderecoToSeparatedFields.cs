using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmokeExpress.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderEnderecoToSeparatedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bairro",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Cidade",
                table: "Orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Complemento",
                table: "Orders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Numero",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rua",
                table: "Orders",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.DropColumn(
                name: "EnderecoEntrega",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnderecoEntrega",
                table: "Orders",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.DropColumn(
                name: "Bairro",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Cidade",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Complemento",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Numero",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Rua",
                table: "Orders");
        }
    }
}
