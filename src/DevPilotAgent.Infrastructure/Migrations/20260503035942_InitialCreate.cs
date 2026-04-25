using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevPilotAgent.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalysisRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ProjectFolderPath = table.Column<string>(type: "TEXT", nullable: false),
                    ErrorLog = table.Column<string>(type: "TEXT", nullable: false),
                    ExtractedKeywordsJson = table.Column<string>(type: "TEXT", nullable: false),
                    RelatedFilesJson = table.Column<string>(type: "TEXT", nullable: false),
                    RootCauseAnalysis = table.Column<string>(type: "TEXT", nullable: false),
                    FixSuggestionsJson = table.Column<string>(type: "TEXT", nullable: false),
                    TestScenariosJson = table.Column<string>(type: "TEXT", nullable: false),
                    PrDescription = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisRecords_CreatedAt",
                table: "AnalysisRecords",
                column: "CreatedAt",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisRecords");
        }
    }
}
