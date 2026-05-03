using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace THtracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addinedx : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tasks_task_lists_TaskListId1",
                table: "tasks");

            migrationBuilder.DropIndex(
                name: "IX_tasks_TaskListId1",
                table: "tasks");

            migrationBuilder.DropColumn(
                name: "TaskListId1",
                table: "tasks");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "categories",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_categories_Name",
                table: "categories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_ActivityId_StartedAt",
                table: "activity_logs",
                columns: new[] { "ActivityId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_EndedAt",
                table: "activity_logs",
                column: "EndedAt");

            migrationBuilder.CreateIndex(
                name: "IX_activity_logs_StartedAt",
                table: "activity_logs",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_activities_Name",
                table: "activities",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_categories_Name",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "IX_activity_logs_ActivityId_StartedAt",
                table: "activity_logs");

            migrationBuilder.DropIndex(
                name: "IX_activity_logs_EndedAt",
                table: "activity_logs");

            migrationBuilder.DropIndex(
                name: "IX_activity_logs_StartedAt",
                table: "activity_logs");

            migrationBuilder.DropIndex(
                name: "IX_activities_Name",
                table: "activities");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "categories");

            migrationBuilder.AddColumn<Guid>(
                name: "TaskListId1",
                table: "tasks",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tasks_TaskListId1",
                table: "tasks",
                column: "TaskListId1");

            migrationBuilder.AddForeignKey(
                name: "FK_tasks_task_lists_TaskListId1",
                table: "tasks",
                column: "TaskListId1",
                principalTable: "task_lists",
                principalColumn: "Id");
        }
    }
}
