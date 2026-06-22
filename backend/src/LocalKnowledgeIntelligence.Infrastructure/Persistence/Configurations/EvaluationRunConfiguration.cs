using LocalKnowledgeIntelligence.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalKnowledgeIntelligence.Infrastructure.Persistence.Configurations;

public sealed class EvaluationRunConfiguration : IEntityTypeConfiguration<EvaluationRun>
{
    public void Configure(EntityTypeBuilder<EvaluationRun> entity)
    {
        entity.ToTable("evaluation_runs");
        entity.HasKey(run => run.Id);
        entity.Property(run => run.Id).HasColumnName("id");
        entity.Property(run => run.SearchMode).HasColumnName("search_mode").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(run => run.QuestionCount).HasColumnName("question_count").IsRequired();
        entity.Property(run => run.Score).HasColumnName("score").IsRequired();
        entity.Property(run => run.Results)
            .HasColumnName("results")
            .HasColumnType("jsonb")
            .HasConversion(v => JsonValueConversions.Serialize(v), v => JsonValueConversions.DeserializeList<EvaluationQuestionResult>(v))
            .Metadata.SetValueComparer(JsonValueConversions.ListComparer<EvaluationQuestionResult>());
        entity.Property(run => run.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(run => run.CompletedAt).HasColumnName("completed_at").IsRequired();
    }
}
