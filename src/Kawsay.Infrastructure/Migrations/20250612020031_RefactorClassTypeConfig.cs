using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorClassTypeConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseConfigurations");

            migrationBuilder.CreateTable(
                name: "ClassTypeConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClassType = table.Column<string>(type: "text", nullable: false),
                    DefaultLength = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassTypeConfigurations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassTypeConfigurations_ClassType",
                table: "ClassTypeConfigurations",
                column: "ClassType",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassTypeConfigurations");

            migrationBuilder.CreateTable(
                name: "CourseConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CourseId = table.Column<int>(type: "integer", nullable: false),
                    ClassType = table.Column<string>(type: "text", nullable: false),
                    DefaultFrequency = table.Column<int>(type: "integer", nullable: false),
                    DefaultLength = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseConfigurations_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseConfigurations_CourseId",
                table: "CourseConfigurations",
                column: "CourseId");
        }
    }
}
