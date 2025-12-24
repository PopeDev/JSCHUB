using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JSCHUB.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKanbanModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "kanban_columnas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProyectoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Posicion = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreadoEl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModificadoEl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kanban_columnas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_kanban_columnas_proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "kanban_tareas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProyectoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ColumnaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AsignadoAId = table.Column<Guid>(type: "uuid", nullable: true),
                    Prioridad = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    HorasEstimadas = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false, defaultValue: 0m),
                    Posicion = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreadoEl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModificadoEl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kanban_tareas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_kanban_tareas_kanban_columnas_ColumnaId",
                        column: x => x.ColumnaId,
                        principalTable: "kanban_columnas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_kanban_tareas_proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_kanban_tareas_usuarios_AsignadoAId",
                        column: x => x.AsignadoAId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_kanban_columnas_ProyectoId",
                table: "kanban_columnas",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_kanban_columnas_ProyectoId_Posicion",
                table: "kanban_columnas",
                columns: new[] { "ProyectoId", "Posicion" });

            migrationBuilder.CreateIndex(
                name: "IX_kanban_tareas_AsignadoAId",
                table: "kanban_tareas",
                column: "AsignadoAId");

            migrationBuilder.CreateIndex(
                name: "IX_kanban_tareas_ColumnaId",
                table: "kanban_tareas",
                column: "ColumnaId");

            migrationBuilder.CreateIndex(
                name: "IX_kanban_tareas_ColumnaId_Posicion",
                table: "kanban_tareas",
                columns: new[] { "ColumnaId", "Posicion" });

            migrationBuilder.CreateIndex(
                name: "IX_kanban_tareas_Prioridad",
                table: "kanban_tareas",
                column: "Prioridad");

            migrationBuilder.CreateIndex(
                name: "IX_kanban_tareas_ProyectoId",
                table: "kanban_tareas",
                column: "ProyectoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kanban_tareas");

            migrationBuilder.DropTable(
                name: "kanban_columnas");
        }
    }
}
