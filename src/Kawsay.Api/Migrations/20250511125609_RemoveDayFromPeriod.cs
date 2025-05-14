using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDayFromPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Courses_CourseId",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_PeriodPreferences_TimetableDays_DayId",
                table: "PeriodPreferences");

            migrationBuilder.DropIndex(
                name: "IX_PeriodPreferences_DayId",
                table: "PeriodPreferences");

            migrationBuilder.DropColumn(
                name: "DayId",
                table: "PeriodPreferences");

            migrationBuilder.AddColumn<int>(
                name: "TimetableDayEntityId",
                table: "PeriodPreferences",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PeriodPreferences_TimetableDayEntityId",
                table: "PeriodPreferences",
                column: "TimetableDayEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Courses_CourseId",
                table: "Classes",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodPreferences_TimetableDays_TimetableDayEntityId",
                table: "PeriodPreferences",
                column: "TimetableDayEntityId",
                principalTable: "TimetableDays",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_Courses_CourseId",
                table: "Classes");

            migrationBuilder.DropForeignKey(
                name: "FK_PeriodPreferences_TimetableDays_TimetableDayEntityId",
                table: "PeriodPreferences");

            migrationBuilder.DropIndex(
                name: "IX_PeriodPreferences_TimetableDayEntityId",
                table: "PeriodPreferences");

            migrationBuilder.DropColumn(
                name: "TimetableDayEntityId",
                table: "PeriodPreferences");

            migrationBuilder.AddColumn<int>(
                name: "DayId",
                table: "PeriodPreferences",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PeriodPreferences_DayId",
                table: "PeriodPreferences",
                column: "DayId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_Courses_CourseId",
                table: "Classes",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PeriodPreferences_TimetableDays_DayId",
                table: "PeriodPreferences",
                column: "DayId",
                principalTable: "TimetableDays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
