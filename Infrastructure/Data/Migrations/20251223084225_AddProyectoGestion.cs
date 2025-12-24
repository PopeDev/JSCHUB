using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JSCHUB.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProyectoGestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_reminder_items_Assignee",
                table: "reminder_items");

            migrationBuilder.DropColumn(
                name: "Assignee",
                table: "reminder_items");

            migrationBuilder.AddColumn<Guid>(
                name: "AsignadoAId",
                table: "reminder_items",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EsGeneral",
                table: "proyectos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "gastos_proyectos",
                columns: table => new
                {
                    GastoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProyectoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gastos_proyectos", x => new { x.GastoId, x.ProyectoId });
                    table.ForeignKey(
                        name: "FK_gastos_proyectos_gastos_GastoId",
                        column: x => x.GastoId,
                        principalTable: "gastos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_gastos_proyectos_proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "reminder_items_proyectos",
                columns: table => new
                {
                    ReminderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProyectoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminder_items_proyectos", x => new { x.ReminderItemId, x.ProyectoId });
                    table.ForeignKey(
                        name: "FK_reminder_items_proyectos_proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_reminder_items_proyectos_reminder_items_ReminderItemId",
                        column: x => x.ReminderItemId,
                        principalTable: "reminder_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreadoEl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tools",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreadoEl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tools", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios_proyectos",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProyectoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AsignadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_proyectos", x => new { x.UsuarioId, x.ProyectoId });
                    table.ForeignKey(
                        name: "FK_usuarios_proyectos_proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_usuarios_proyectos_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prompts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ProyectoId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToolId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreadoEl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    ModificadoEl = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prompts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prompts_proyectos_ProyectoId",
                        column: x => x.ProyectoId,
                        principalTable: "proyectos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_prompts_tools_ToolId",
                        column: x => x.ToolId,
                        principalTable: "tools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prompts_usuarios_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "prompt_tags",
                columns: table => new
                {
                    PromptId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prompt_tags", x => new { x.PromptId, x.TagId });
                    table.ForeignKey(
                        name: "FK_prompt_tags_prompts_PromptId",
                        column: x => x.PromptId,
                        principalTable: "prompts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prompt_tags_tags_TagId",
                        column: x => x.TagId,
                        principalTable: "tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_reminder_items_AsignadoAId",
                table: "reminder_items",
                column: "AsignadoAId");

            migrationBuilder.CreateIndex(
                name: "IX_proyectos_EsGeneral_Unique",
                table: "proyectos",
                column: "EsGeneral",
                unique: true,
                filter: "\"EsGeneral\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_gastos_proyectos_GastoId",
                table: "gastos_proyectos",
                column: "GastoId");

            migrationBuilder.CreateIndex(
                name: "IX_gastos_proyectos_ProyectoId",
                table: "gastos_proyectos",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_prompt_tags_TagId",
                table: "prompt_tags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_prompts_Activo",
                table: "prompts",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_prompts_CreatedByUserId",
                table: "prompts",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_prompts_ModificadoEl",
                table: "prompts",
                column: "ModificadoEl");

            migrationBuilder.CreateIndex(
                name: "IX_prompts_ProyectoId",
                table: "prompts",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_prompts_ToolId",
                table: "prompts",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_reminder_items_proyectos_ProyectoId",
                table: "reminder_items_proyectos",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_reminder_items_proyectos_ReminderItemId",
                table: "reminder_items_proyectos",
                column: "ReminderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_tags_Activo",
                table: "tags",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_tags_Name",
                table: "tags",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tools_Activo",
                table: "tools",
                column: "Activo");

            migrationBuilder.CreateIndex(
                name: "IX_tools_Name",
                table: "tools",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_proyectos_ProyectoId",
                table: "usuarios_proyectos",
                column: "ProyectoId");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_proyectos_Rol",
                table: "usuarios_proyectos",
                column: "Rol");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_proyectos_UsuarioId",
                table: "usuarios_proyectos",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_reminder_items_usuarios_AsignadoAId",
                table: "reminder_items",
                column: "AsignadoAId",
                principalTable: "usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_reminder_items_usuarios_AsignadoAId",
                table: "reminder_items");

            migrationBuilder.DropTable(
                name: "gastos_proyectos");

            migrationBuilder.DropTable(
                name: "prompt_tags");

            migrationBuilder.DropTable(
                name: "reminder_items_proyectos");

            migrationBuilder.DropTable(
                name: "usuarios_proyectos");

            migrationBuilder.DropTable(
                name: "prompts");

            migrationBuilder.DropTable(
                name: "tags");

            migrationBuilder.DropTable(
                name: "tools");

            migrationBuilder.DropIndex(
                name: "IX_reminder_items_AsignadoAId",
                table: "reminder_items");

            migrationBuilder.DropIndex(
                name: "IX_proyectos_EsGeneral_Unique",
                table: "proyectos");

            migrationBuilder.DropColumn(
                name: "AsignadoAId",
                table: "reminder_items");

            migrationBuilder.DropColumn(
                name: "EsGeneral",
                table: "proyectos");

            migrationBuilder.AddColumn<string>(
                name: "Assignee",
                table: "reminder_items",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_reminder_items_Assignee",
                table: "reminder_items",
                column: "Assignee");
        }
    }
}
