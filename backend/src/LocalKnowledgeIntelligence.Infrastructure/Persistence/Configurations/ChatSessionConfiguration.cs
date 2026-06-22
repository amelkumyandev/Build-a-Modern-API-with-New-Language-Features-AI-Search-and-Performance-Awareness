using LocalKnowledgeIntelligence.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalKnowledgeIntelligence.Infrastructure.Persistence.Configurations;

public sealed class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
{
    public void Configure(EntityTypeBuilder<ChatSession> entity)
    {
        entity.ToTable("chat_sessions");
        entity.HasKey(session => session.Id);
        entity.Property(session => session.Id).HasColumnName("id");
        entity.Property(session => session.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        entity.Property(session => session.UserId).HasColumnName("user_id").IsRequired();
        entity.Property(session => session.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(session => session.UpdatedAt).HasColumnName("updated_at").IsRequired();
    }
}
