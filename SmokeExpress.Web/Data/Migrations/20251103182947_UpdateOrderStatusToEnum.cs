using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmokeExpress.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderStatusToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Não é necessária alteração na estrutura do banco.
            // O enum OrderStatus é armazenado como string (nvarchar) através do HasConversion<string>().
            // Valores existentes como "Processando" serão automaticamente convertidos para OrderStatus.Processando.
            // Se houver outros valores de string que precisem ser migrados, adicionar lógica aqui.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reversão não necessária - o enum continua sendo armazenado como string no banco.
        }
    }
}
