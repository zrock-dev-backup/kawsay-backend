using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TimetableDayConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "FK_PeriodPreferences_TimetableDays_DayId",
                table: "PeriodPreferences",
                column: "DayId",
                principalTable: "TimetableDays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeriodPreferences_TimetableDays_DayId",
                table: "PeriodPreferences");

            migrationBuilder.DropIndex(
                name: "IX_PeriodPreferences_DayId",
                table: "PeriodPreferences");

            migrationBuilder.DropColumn(
                name: "DayId",
                table: "PeriodPreferences");
        }
    }
}
