using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JSCHUB.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Changes = table.Column<string>(type: "jsonb", nullable: true),
                    User = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    At = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reminder_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Tags = table.Column<string>(type: "jsonb", nullable: false),
                    Assignee = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScheduleType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecurrenceFrequency = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CustomIntervalDays = table.Column<int>(type: "integer", nullable: true),
                    Timezone = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Europe/Madrid"),
                    LeadTimeDays = table.Column<string>(type: "jsonb", nullable: false),
                    NextOccurrenceAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastOccurrenceAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reminder_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReminderItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TriggerAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    SnoozedUntil = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OccurrenceAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_alerts_reminder_items_ReminderItemId",
                        column: x => x.ReminderItemId,
                        principalTable: "reminder_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alerts_ReminderItemId",
                table: "alerts",
                column: "ReminderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_alerts_ReminderItemId_OccurrenceAt",
                table: "alerts",
                columns: new[] { "ReminderItemId", "OccurrenceAt" });

            migrationBuilder.CreateIndex(
                name: "IX_alerts_Severity",
                table: "alerts",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_alerts_State",
                table: "alerts",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_alerts_TriggerAt",
                table: "alerts",
                column: "TriggerAt");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_At",
                table: "audit_logs",
                column: "At");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EntityType_EntityId",
                table: "audit_logs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_reminder_items_Assignee",
                table: "reminder_items",
                column: "Assignee");

            migrationBuilder.CreateIndex(
                name: "IX_reminder_items_Category",
                table: "reminder_items",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_reminder_items_NextOccurrenceAt",
                table: "reminder_items",
                column: "NextOccurrenceAt");

            migrationBuilder.CreateIndex(
                name: "IX_reminder_items_Status",
                table: "reminder_items",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alerts");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "reminder_items");
        }
    }
}
