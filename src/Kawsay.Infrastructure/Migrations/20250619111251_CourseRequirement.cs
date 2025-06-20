using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CourseRequirement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseRequirements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TimetableId = table.Column<int>(type: "integer", nullable: false),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    StudentGroupId = table.Column<int>(type: "integer", nullable: true),
                    SectionId = table.Column<int>(type: "integer", nullable: true),
                    PreferredTeacherId = table.Column<int>(type: "integer", nullable: true),
                    Priority = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DurationInPeriods = table.Column<int>(type: "integer", nullable: false),
                    FrequencyPerWeek = table.Column<int>(type: "integer", nullable: false),
                    RequiredCapacity = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseRequirements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseRequirements_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseRequirements_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CourseRequirements_StudentGroups_StudentGroupId",
                        column: x => x.StudentGroupId,
                        principalTable: "StudentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CourseRequirements_Teachers_PreferredTeacherId",
                        column: x => x.PreferredTeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CourseRequirements_Timetables_TimetableId",
                        column: x => x.TimetableId,
                        principalTable: "Timetables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SoftSchedulingPreferenceEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseRequirementId = table.Column<int>(type: "integer", nullable: false),
                    PreferenceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Value = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Weight = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftSchedulingPreferenceEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SoftSchedulingPreferenceEntity_CourseRequirements_CourseReq~",
                        column: x => x.CourseRequirementId,
                        principalTable: "CourseRequirements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseRequirements_CourseId",
                table: "CourseRequirements",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseRequirements_PreferredTeacherId",
                table: "CourseRequirements",
                column: "PreferredTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseRequirements_SectionId",
                table: "CourseRequirements",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseRequirements_StudentGroupId",
                table: "CourseRequirements",
                column: "StudentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseRequirements_TimetableId",
                table: "CourseRequirements",
                column: "TimetableId");

            migrationBuilder.CreateIndex(
                name: "IX_SoftSchedulingPreferenceEntity_CourseRequirementId",
                table: "SoftSchedulingPreferenceEntity",
                column: "CourseRequirementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SoftSchedulingPreferenceEntity");

            migrationBuilder.DropTable(
                name: "CourseRequirements");
        }
    }
}
