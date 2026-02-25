using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalysisStatusToJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Status",
                table: "Jobs",
                newName: "ApplicationStatus");

            migrationBuilder.AddColumn<string>(
                name: "AnalysisErrorMessage",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AnalysisStatus",
                table: "Jobs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnalysisErrorMessage",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "AnalysisStatus",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "ApplicationStatus",
                table: "Jobs",
                newName: "Status");
        }
    }
}
