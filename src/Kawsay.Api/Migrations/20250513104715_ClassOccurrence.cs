using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class ClassOccurrence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PeriodPreferences_TimetableDays_TimetableDayEntityId",
                table: "PeriodPreferences");

            migrationBuilder.DropIndex(
                name: "IX_PeriodPreferences_TimetableDayEntityId",
                table: "PeriodPreferences");

            migrationBuilder.DropColumn(
                name: "TimetableDayEntityId",
                table: "PeriodPreferences");

            migrationBuilder.CreateTable(
                name: "ClassOccurence",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClassId = table.Column<int>(type: "integer", nullable: false),
                    DayId = table.Column<int>(type: "integer", nullable: false),
                    StartPeriodId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassOccurence", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassOccurence_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassOccurence_TimetableDays_DayId",
                        column: x => x.DayId,
                        principalTable: "TimetableDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassOccurence_TimetablePeriods_StartPeriodId",
                        column: x => x.StartPeriodId,
                        principalTable: "TimetablePeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassOccurence_ClassId",
                table: "ClassOccurence",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassOccurence_DayId",
                table: "ClassOccurence",
                column: "DayId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassOccurence_StartPeriodId",
                table: "ClassOccurence",
                column: "StartPeriodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassOccurence");

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
                name: "FK_PeriodPreferences_TimetableDays_TimetableDayEntityId",
                table: "PeriodPreferences",
                column: "TimetableDayEntityId",
                principalTable: "TimetableDays",
                principalColumn: "Id");
        }
    }
}
