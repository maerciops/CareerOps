using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobAnalysisTable : Migration
    {
        /// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// 1. Cria a tabela nova
			migrationBuilder.CreateTable(
				name: "JobAnalysis",
				columns: table => new {
					Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					JobApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
					ResumeUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
					AiAnalysisResult = table.Column<string>(type: "nvarchar(max)", nullable: true),
					AnalysisStatus = table.Column<int>(type: "int", nullable: false),
					AnalysisErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
					CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
					UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
					OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
				},
				constraints: table => {
					table.PrimaryKey("PK_JobAnalysis", x => x.Id);
					table.ForeignKey(
						name: "FK_JobAnalysis_Jobs_JobApplicationId",
						column: x => x.JobApplicationId,
						principalTable: "Jobs",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			// 2. DATA MIGRATION: Move o que você já tinha na Jobs para a nova tabela
			// Assim você não perde as análises que já fez nos testes anteriores!
			migrationBuilder.Sql(@"
				INSERT INTO JobAnalysis (Id, JobApplicationId, ResumeUrl, AiAnalysisResult, AnalysisStatus, CreatedAt, OwnerId)
				SELECT NEWID(), Id, ResumeURL, AiAnalysisResult, AnalysisStatus, CreatedAt, OwnerId 
				FROM Jobs 
				WHERE ResumeURL IS NOT NULL AND ResumeURL <> ''");

			// 3. Limpeza: Remove as colunas que não pertencem mais à tabela Jobs
			migrationBuilder.DropColumn(name: "AiAnalysisResult", table: "Jobs");
			migrationBuilder.DropColumn(name: "AnalysisStatus", table: "Jobs");
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobAnalysis");

            migrationBuilder.AddColumn<string>(
                name: "AiAnalysisResult",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: true);

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
    }
}
