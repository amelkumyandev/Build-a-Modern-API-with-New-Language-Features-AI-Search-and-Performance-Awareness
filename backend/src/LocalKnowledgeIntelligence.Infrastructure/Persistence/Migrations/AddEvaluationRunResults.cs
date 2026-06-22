using Microsoft.EntityFrameworkCore.Migrations;

namespace LocalKnowledgeIntelligence.Infrastructure;

[Microsoft.EntityFrameworkCore.Infrastructure.DbContextAttribute(typeof(AppDbContext))]
[Migration("202606190002_AddEvaluationRunResults")]
public sealed class AddEvaluationRunResults : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "results",
            table: "evaluation_runs",
            type: "jsonb",
            nullable: false,
            defaultValueSql: "'[]'::jsonb");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn("results", "evaluation_runs");
    }
}
