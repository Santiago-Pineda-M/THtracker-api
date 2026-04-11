using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace THtracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixActivityValueDefCascade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_activity_value_definitions_ActivityId",
                table: "activity_value_definitions",
                column: "ActivityId");

            migrationBuilder.AddForeignKey(
                name: "FK_activity_value_definitions_activities_ActivityId",
                table: "activity_value_definitions",
                column: "ActivityId",
                principalTable: "activities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_activity_value_definitions_activities_ActivityId",
                table: "activity_value_definitions");

            migrationBuilder.DropIndex(
                name: "IX_activity_value_definitions_ActivityId",
                table: "activity_value_definitions");
        }
    }
}
