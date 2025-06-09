using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TimetableDateRange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassOccurrences_TimetableDays_DayId",
                table: "ClassOccurrences");

            migrationBuilder.DropIndex(
                name: "IX_ClassOccurrences_DayId",
                table: "ClassOccurrences");

            migrationBuilder.DropColumn(
                name: "DayId",
                table: "ClassOccurrences");

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "Timetables",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "StartDate",
                table: "Timetables",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "ClassOccurrences",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<int>(
                name: "TimetableDayEntityId",
                table: "ClassOccurrences",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassOccurrences_TimetableDayEntityId",
                table: "ClassOccurrences",
                column: "TimetableDayEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassOccurrences_TimetableDays_TimetableDayEntityId",
                table: "ClassOccurrences",
                column: "TimetableDayEntityId",
                principalTable: "TimetableDays",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassOccurrences_TimetableDays_TimetableDayEntityId",
                table: "ClassOccurrences");

            migrationBuilder.DropIndex(
                name: "IX_ClassOccurrences_TimetableDayEntityId",
                table: "ClassOccurrences");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Timetables");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "Timetables");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "ClassOccurrences");

            migrationBuilder.DropColumn(
                name: "TimetableDayEntityId",
                table: "ClassOccurrences");

            migrationBuilder.AddColumn<int>(
                name: "DayId",
                table: "ClassOccurrences",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ClassOccurrences_DayId",
                table: "ClassOccurrences",
                column: "DayId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassOccurrences_TimetableDays_DayId",
                table: "ClassOccurrences",
                column: "DayId",
                principalTable: "TimetableDays",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
