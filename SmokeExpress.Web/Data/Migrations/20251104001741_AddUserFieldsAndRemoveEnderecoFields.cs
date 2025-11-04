using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmokeExpress.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFieldsAndRemoveEnderecoFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Endereco",
                table: "AspNetUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<bool>(
                name: "ConsentiuMarketing",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DocumentoFiscal",
                table: "AspNetUsers",
                type: "nvarchar(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Genero",
                table: "AspNetUsers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TermosAceitosEm",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoCliente",
                table: "AspNetUsers",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_DocumentoFiscal",
                table: "AspNetUsers",
                column: "DocumentoFiscal",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_DocumentoFiscal",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ConsentiuMarketing",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "DocumentoFiscal",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Genero",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TermosAceitosEm",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TipoCliente",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "Endereco",
                table: "AspNetUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }
    }
}
