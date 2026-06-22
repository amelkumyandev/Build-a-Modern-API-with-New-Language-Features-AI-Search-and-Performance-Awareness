using LocalKnowledgeIntelligence.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalKnowledgeIntelligence.Infrastructure.Persistence.Configurations;

public sealed class AgentRunConfiguration : IEntityTypeConfiguration<AgentRun>
{
    public void Configure(EntityTypeBuilder<AgentRun> entity)
    {
        entity.ToTable("agent_runs");
        entity.HasKey(run => run.Id);
        entity.Property(run => run.Id).HasColumnName("id");
        entity.Property(run => run.SessionId).HasColumnName("session_id").IsRequired();
        entity.Property(run => run.UserMessageId).HasColumnName("user_message_id").IsRequired();
        entity.Property(run => run.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(run => run.Model).HasColumnName("model").HasMaxLength(100).IsRequired();
        entity.Property(run => run.SearchMode).HasColumnName("search_mode").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(run => run.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(run => run.CompletedAt).HasColumnName("completed_at");
        entity.HasMany(run => run.Steps).WithOne().HasForeignKey(step => step.AgentRunId).OnDelete(DeleteBehavior.Cascade);
    }
}
