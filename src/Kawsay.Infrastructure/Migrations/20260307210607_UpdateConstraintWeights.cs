using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateConstraintWeights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Level",
                table: "TeacherAvailabilities",
                newName: "WeightPercentage");

            migrationBuilder.RenameColumn(
                name: "Level",
                table: "StudentAvailabilities",
                newName: "WeightPercentage");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WeightPercentage",
                table: "TeacherAvailabilities",
                newName: "Level");

            migrationBuilder.RenameColumn(
                name: "WeightPercentage",
                table: "StudentAvailabilities",
                newName: "Level");
        }
    }
}
