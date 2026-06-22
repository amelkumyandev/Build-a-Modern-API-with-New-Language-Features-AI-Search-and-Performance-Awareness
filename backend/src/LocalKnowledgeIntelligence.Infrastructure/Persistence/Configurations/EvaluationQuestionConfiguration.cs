using LocalKnowledgeIntelligence.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalKnowledgeIntelligence.Infrastructure.Persistence.Configurations;

public sealed class EvaluationQuestionConfiguration : IEntityTypeConfiguration<EvaluationQuestion>
{
    public void Configure(EntityTypeBuilder<EvaluationQuestion> entity)
    {
        entity.ToTable("evaluation_questions");
        entity.HasKey(question => question.Id);
        entity.Property(question => question.Id).HasColumnName("id");
        entity.Property(question => question.Question).HasColumnName("question").IsRequired();
        entity.Property(question => question.ExpectedAnswerKeywords)
            .HasColumnName("expected_answer_keywords")
            .HasColumnType("jsonb")
            .HasConversion(v => JsonValueConversions.Serialize(v), v => JsonValueConversions.DeserializeList<string>(v))
            .Metadata.SetValueComparer(JsonValueConversions.ListComparer<string>());
        entity.Property(question => question.ExpectedDocumentTitles)
            .HasColumnName("expected_document_titles")
            .HasColumnType("jsonb")
            .HasConversion(v => JsonValueConversions.Serialize(v), v => JsonValueConversions.DeserializeList<string>(v))
            .Metadata.SetValueComparer(JsonValueConversions.ListComparer<string>());
        entity.Property(question => question.Difficulty).HasColumnName("difficulty").HasMaxLength(30).IsRequired();
        entity.Property(question => question.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.HasIndex(question => question.Question).IsUnique();
    }
}
