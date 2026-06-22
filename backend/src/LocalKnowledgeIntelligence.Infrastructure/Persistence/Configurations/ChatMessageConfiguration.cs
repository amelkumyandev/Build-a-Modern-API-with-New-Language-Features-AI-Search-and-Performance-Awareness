using LocalKnowledgeIntelligence.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalKnowledgeIntelligence.Infrastructure.Persistence.Configurations;

public sealed class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> entity)
    {
        entity.ToTable("chat_messages");
        entity.HasKey(message => message.Id);
        entity.Property(message => message.Id).HasColumnName("id");
        entity.Property(message => message.SessionId).HasColumnName("session_id").IsRequired();
        entity.Property(message => message.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(message => message.Content).HasColumnName("content").IsRequired();
        entity.Property(message => message.Citations)
            .HasColumnName("citations")
            .HasColumnType("jsonb")
            .HasConversion(v => JsonValueConversions.Serialize(v), v => JsonValueConversions.DeserializeCitations(v))
            .Metadata.SetValueComparer(JsonValueConversions.ListComparer<Citation>());
        entity.Property(message => message.CreatedAt).HasColumnName("created_at").IsRequired();
    }
}
