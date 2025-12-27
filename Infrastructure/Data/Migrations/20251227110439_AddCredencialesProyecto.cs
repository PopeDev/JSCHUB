using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JSCHUB.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCredencialesProyecto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "credenciales_proyecto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnlaceProyectoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Usuario = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PasswordCifrado = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Notas = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Activa = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    UltimoAcceso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreadoEl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModificadoEl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credenciales_proyecto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_credenciales_proyecto_enlaces_proyecto_EnlaceProyectoId",
                        column: x => x.EnlaceProyectoId,
                        principalTable: "enlaces_proyecto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_credenciales_proyecto_Activa",
                table: "credenciales_proyecto",
                column: "Activa");

            migrationBuilder.CreateIndex(
                name: "IX_credenciales_proyecto_EnlaceProyectoId",
                table: "credenciales_proyecto",
                column: "EnlaceProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_credenciales_proyecto_EnlaceProyectoId_Nombre",
                table: "credenciales_proyecto",
                columns: new[] { "EnlaceProyectoId", "Nombre" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "credenciales_proyecto");
        }
    }
}
