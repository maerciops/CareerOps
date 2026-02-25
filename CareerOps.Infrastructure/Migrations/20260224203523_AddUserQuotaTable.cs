using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerOps.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserQuotaTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserQuotas",
                columns: table => new
                {
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaxDailyRequests = table.Column<int>(type: "int", nullable: false),
                    UsedDailyRequests = table.Column<int>(type: "int", nullable: false),
                    LastResetDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserQuotas", x => x.OwnerId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserQuotas");
        }
    }
}
