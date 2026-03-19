using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateJobAnalysisHistoryTable : Migration
    {
        /// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		// 1. CRIAR A TABELA NOVA
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
				OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
				IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
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

		// 2. MOVER OS DADOS (IMPORTANTE: Antes dos DropColumn)
		migrationBuilder.Sql(@"
			INSERT INTO JobAnalysis (Id, JobApplicationId, ResumeUrl, AnalysisStatus, CreatedAt, UpdatedAt, OwnerId, IsDeleted)
			SELECT NEWID(), Id, ResumeURL, 0, CreatedAt, GETUTCDATE(), OwnerId, 0
			FROM Jobs 
			WHERE ResumeURL IS NOT NULL AND ResumeURL <> ''");

		// 3. REMOVER AS COLUNAS ANTIGAS DA TABELA JOBS
		//migrationBuilder.DropColumn(name: "AiAnalysisResult", table: "Jobs");
		//migrationBuilder.DropColumn(name: "AnalysisStatus", table: "Jobs");
		// Se houver DropColumn de AnalysisErrorMessage, coloque aqui também.
	}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
