using LocalKnowledgeIntelligence.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalKnowledgeIntelligence.Infrastructure.Persistence.Configurations;

public sealed class AgentStepConfiguration : IEntityTypeConfiguration<AgentStep>
{
    public void Configure(EntityTypeBuilder<AgentStep> entity)
    {
        entity.ToTable("agent_steps");
        entity.HasKey(step => step.Id);
        entity.Property(step => step.Id).HasColumnName("id");
        entity.Property(step => step.AgentRunId).HasColumnName("agent_run_id").IsRequired();
        entity.Property(step => step.StepIndex).HasColumnName("step_index").IsRequired();
        entity.Property(step => step.ToolType).HasColumnName("tool_type").HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(step => step.Input)
            .HasColumnName("input")
            .HasColumnType("jsonb")
            .HasConversion(v => JsonValueConversions.Serialize(v), v => JsonValueConversions.DeserializeString(v));
        entity.Property(step => step.Output)
            .HasColumnName("output")
            .HasColumnType("jsonb")
            .HasConversion(v => JsonValueConversions.Serialize(v), v => JsonValueConversions.DeserializeString(v));
        entity.Property(step => step.DurationMs).HasColumnName("duration_ms").IsRequired();
        entity.Property(step => step.CreatedAt).HasColumnName("created_at").IsRequired();
    }
}
