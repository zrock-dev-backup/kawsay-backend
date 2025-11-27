using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AutoMigration_20251127_034156 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StagedPlacements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseRequirementId = table.Column<int>(type: "integer", nullable: false),
                    DayId = table.Column<int>(type: "integer", nullable: false),
                    StartPeriodId = table.Column<int>(type: "integer", nullable: false),
                    Length = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StagedPlacements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StagedPlacements_CourseRequirements_CourseRequirementId",
                        column: x => x.CourseRequirementId,
                        principalTable: "CourseRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StagedPlacements_TimetableDays_DayId",
                        column: x => x.DayId,
                        principalTable: "TimetableDays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StagedPlacements_TimetablePeriods_StartPeriodId",
                        column: x => x.StartPeriodId,
                        principalTable: "TimetablePeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentIssues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    TimetableId = table.Column<int>(type: "integer", nullable: false),
                    IssueType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Details = table.Column<string>(type: "text", nullable: false),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentIssues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentIssues_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimetableAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TimetableId = table.Column<int>(type: "integer", nullable: false),
                    TeacherId = table.Column<int>(type: "integer", nullable: false),
                    StartWeek = table.Column<int>(type: "integer", nullable: false),
                    EndWeek = table.Column<int>(type: "integer", nullable: false),
                    MaximumWorkload = table.Column<int>(type: "integer", nullable: false),
                    WorkloadUnit = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimetableAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimetableAssignments_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TimetableAssignments_Timetables_TimetableId",
                        column: x => x.TimetableId,
                        principalTable: "Timetables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StagedPlacements_CourseRequirementId",
                table: "StagedPlacements",
                column: "CourseRequirementId");

            migrationBuilder.CreateIndex(
                name: "IX_StagedPlacements_DayId",
                table: "StagedPlacements",
                column: "DayId");

            migrationBuilder.CreateIndex(
                name: "IX_StagedPlacements_StartPeriodId",
                table: "StagedPlacements",
                column: "StartPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentIssues_StudentId",
                table: "StudentIssues",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentIssues_TimetableId_StudentId",
                table: "StudentIssues",
                columns: new[] { "TimetableId", "StudentId" });

            migrationBuilder.CreateIndex(
                name: "IX_TimetableAssignments_TeacherId",
                table: "TimetableAssignments",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_TimetableAssignments_TimetableId_TeacherId",
                table: "TimetableAssignments",
                columns: new[] { "TimetableId", "TeacherId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StagedPlacements");

            migrationBuilder.DropTable(
                name: "StudentIssues");

            migrationBuilder.DropTable(
                name: "TimetableAssignments");
        }
    }
}
