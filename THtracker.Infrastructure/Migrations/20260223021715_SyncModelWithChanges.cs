using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace THtracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelWithChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_activity_log_values_ActivityLogId",
                table: "activity_log_values",
                column: "ActivityLogId");

            migrationBuilder.CreateIndex(
                name: "IX_activity_log_values_ValueDefinitionId",
                table: "activity_log_values",
                column: "ValueDefinitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_activity_log_values_activity_logs_ActivityLogId",
                table: "activity_log_values",
                column: "ActivityLogId",
                principalTable: "activity_logs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_activity_log_values_activity_value_definitions_ValueDefinitionId",
                table: "activity_log_values",
                column: "ValueDefinitionId",
                principalTable: "activity_value_definitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_activity_log_values_activity_logs_ActivityLogId",
                table: "activity_log_values");

            migrationBuilder.DropForeignKey(
                name: "FK_activity_log_values_activity_value_definitions_ValueDefinitionId",
                table: "activity_log_values");

            migrationBuilder.DropIndex(
                name: "IX_activity_log_values_ActivityLogId",
                table: "activity_log_values");

            migrationBuilder.DropIndex(
                name: "IX_activity_log_values_ValueDefinitionId",
                table: "activity_log_values");
        }
    }
}
