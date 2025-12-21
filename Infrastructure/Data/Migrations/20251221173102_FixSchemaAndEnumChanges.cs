using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JSCHUB.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixSchemaAndEnumChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_gastos_personas_PagadoPorId",
                table: "gastos");

            migrationBuilder.DropTable(
                name: "personas");

            migrationBuilder.CreateTable(
                name: "proyectos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EnlacePrincipal = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Etiquetas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreadoEl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModificadoEl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proyectos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
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
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "enlaces_proyecto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProyectoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreadoEl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModificadoEl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enlaces_proyecto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_enlaces_proyecto_proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recursos_proyecto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProyectoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Contenido = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    Etiquetas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreadoEl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModificadoEl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recursos_proyecto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recursos_proyecto_proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_enlaces_proyecto_ProyectoId",
                table: "enlaces_proyecto",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_enlaces_proyecto_ProyectoId_Orden",
                table: "enlaces_proyecto",
                columns: new[] { "ProyectoId", "Orden" });

            migrationBuilder.CreateIndex(
                name: "IX_enlaces_proyecto_Tipo",
                table: "enlaces_proyecto",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_proyectos_Estado",
                table: "proyectos",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_proyectos_ModificadoEl",
                table: "proyectos",
                column: "ModificadoEl");

            migrationBuilder.CreateIndex(
                name: "IX_proyectos_Nombre",
                table: "proyectos",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_recursos_proyecto_ModificadoEl",
                table: "recursos_proyecto",
                column: "ModificadoEl");

            migrationBuilder.CreateIndex(
                name: "IX_recursos_proyecto_Nombre",
                table: "recursos_proyecto",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_recursos_proyecto_ProyectoId",
                table: "recursos_proyecto",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_recursos_proyecto_Tipo",
                table: "recursos_proyecto",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Activo",
                table: "usuarios",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Nombre",
                table: "usuarios",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_gastos_usuarios_PagadoPorId",
                table: "gastos",
                column: "PagadoPorId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_gastos_usuarios_PagadoPorId",
                table: "gastos");

            migrationBuilder.DropTable(
                name: "enlaces_proyecto");

            migrationBuilder.DropTable(
                name: "recursos_proyecto");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "proyectos");

            migrationBuilder.CreateTable(
                name: "personas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_personas_Activo",
                table: "personas",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_personas_Nombre",
                table: "personas",
                column: "Nombre");

            migrationBuilder.AddForeignKey(
                name: "FK_gastos_personas_PagadoPorId",
                table: "gastos",
                column: "PagadoPorId",
                principalTable: "personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
