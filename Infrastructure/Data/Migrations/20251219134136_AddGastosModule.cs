using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JSCHUB.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGastosModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "personas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gastos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Concepto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Notas = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Importe = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "EUR"),
                    PagadoPorId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaPago = table.Column<DateOnly>(type: "date", nullable: false),
                    HoraPago = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gastos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_gastos_personas_PagadoPorId",
                        column: x => x.PagadoPorId,
                        principalTable: "personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_gastos_Concepto",
                table: "gastos",
                column: "Concepto");

            migrationBuilder.CreateIndex(
                name: "IX_gastos_Estado",
                table: "gastos",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_gastos_FechaPago",
                table: "gastos",
                column: "FechaPago");

            migrationBuilder.CreateIndex(
                name: "IX_gastos_PagadoPorId",
                table: "gastos",
                column: "PagadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_personas_Activo",
                table: "personas",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_personas_Nombre",
                table: "personas",
                column: "Nombre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gastos");

            migrationBuilder.DropTable(
                name: "personas");
        }
    }
}
