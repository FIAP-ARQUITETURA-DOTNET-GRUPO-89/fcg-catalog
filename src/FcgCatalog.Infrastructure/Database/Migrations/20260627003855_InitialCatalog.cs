using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FcgCatalog.Infrastructure.Database.Migrations;

/// <inheritdoc />
public partial class InitialCatalog : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Games",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Nome = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
                Descricao = table.Column<string>(type: "character varying(500)", unicode: false, maxLength: 500, nullable: false),
                Preco = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                DataLancamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ClassificacaoEtaria = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                Inativo = table.Column<bool>(type: "boolean", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Games", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Games_Nome",
            table: "Games",
            column: "Nome",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Games");
    }
}
