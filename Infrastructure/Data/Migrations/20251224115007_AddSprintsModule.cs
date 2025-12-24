using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JSCHUB.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSprintsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SprintActivoId",
                table: "proyectos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsComprometida",
                table: "kanban_tareas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "SprintId",
                table: "kanban_tareas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SprintOrigenId",
                table: "kanban_tareas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SprintsTranscurridos",
                table: "kanban_tareas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "sprints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProyectoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Temporizacion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Objetivo = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TareasComprometidas = table.Column<int>(type: "integer", nullable: true),
                    TareasEntregadas = table.Column<int>(type: "integer", nullable: true),
                    PorcentajeCompletitud = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    FechaCierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreadoEl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModificadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModificadoEl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sprints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sprints_proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sprint_tareas_historico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SprintId = table.Column<Guid>(type: "uuid", nullable: false),
                    TareaId = table.Column<Guid>(type: "uuid", nullable: false),
                    TareaTitulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TareaDescripcion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AsignadoANombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FueEntregada = table.Column<bool>(type: "boolean", nullable: false),
                    ColumnaFinal = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EraComprometida = table.Column<bool>(type: "boolean", nullable: false),
                    SprintsTranscurridos = table.Column<int>(type: "integer", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sprint_tareas_historico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sprint_tareas_historico_sprints_SprintId",
                        column: x => x.SprintId,
                        principalTable: "sprints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_proyectos_SprintActivoId",
                table: "proyectos",
                column: "SprintActivoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_kanban_tareas_SprintId",
                table: "kanban_tareas",
                column: "SprintId");

            migrationBuilder.CreateIndex(
                name: "IX_kanban_tareas_SprintId_EsComprometida",
                table: "kanban_tareas",
                columns: new[] { "SprintId", "EsComprometida" });

            migrationBuilder.CreateIndex(
                name: "IX_sprint_tareas_historico_SprintId",
                table: "sprint_tareas_historico",
                column: "SprintId");

            migrationBuilder.CreateIndex(
                name: "IX_sprint_tareas_historico_SprintId_FueEntregada",
                table: "sprint_tareas_historico",
                columns: new[] { "SprintId", "FueEntregada" });

            migrationBuilder.CreateIndex(
                name: "IX_sprint_tareas_historico_TareaId",
                table: "sprint_tareas_historico",
                column: "TareaId");

            migrationBuilder.CreateIndex(
                name: "IX_sprints_Estado",
                table: "sprints",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_sprints_ProyectoId",
                table: "sprints",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_sprints_ProyectoId_Estado",
                table: "sprints",
                columns: new[] { "ProyectoId", "Estado" });

            migrationBuilder.AddForeignKey(
                name: "FK_kanban_tareas_sprints_SprintId",
                table: "kanban_tareas",
                column: "SprintId",
                principalTable: "sprints",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_proyectos_sprints_SprintActivoId",
                table: "proyectos",
                column: "SprintActivoId",
                principalTable: "sprints",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_kanban_tareas_sprints_SprintId",
                table: "kanban_tareas");

            migrationBuilder.DropForeignKey(
                name: "FK_proyectos_sprints_SprintActivoId",
                table: "proyectos");

            migrationBuilder.DropTable(
                name: "sprint_tareas_historico");

            migrationBuilder.DropTable(
                name: "sprints");

            migrationBuilder.DropIndex(
                name: "IX_proyectos_SprintActivoId",
                table: "proyectos");

            migrationBuilder.DropIndex(
                name: "IX_kanban_tareas_SprintId",
                table: "kanban_tareas");

            migrationBuilder.DropIndex(
                name: "IX_kanban_tareas_SprintId_EsComprometida",
                table: "kanban_tareas");

            migrationBuilder.DropColumn(
                name: "SprintActivoId",
                table: "proyectos");

            migrationBuilder.DropColumn(
                name: "EsComprometida",
                table: "kanban_tareas");

            migrationBuilder.DropColumn(
                name: "SprintId",
                table: "kanban_tareas");

            migrationBuilder.DropColumn(
                name: "SprintOrigenId",
                table: "kanban_tareas");

            migrationBuilder.DropColumn(
                name: "SprintsTranscurridos",
                table: "kanban_tareas");
        }
    }
}
