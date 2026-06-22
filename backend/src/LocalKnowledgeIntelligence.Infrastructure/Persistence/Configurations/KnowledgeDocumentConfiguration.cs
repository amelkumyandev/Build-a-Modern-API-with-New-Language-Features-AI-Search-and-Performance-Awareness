using LocalKnowledgeIntelligence.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalKnowledgeIntelligence.Infrastructure.Persistence.Configurations;

public sealed class KnowledgeDocumentConfiguration : IEntityTypeConfiguration<KnowledgeDocument>
{
    public void Configure(EntityTypeBuilder<KnowledgeDocument> entity)
    {
        entity.ToTable("documents");
        entity.HasKey(document => document.Id);
        entity.Property(document => document.Id).HasColumnName("id");
        entity.Property(document => document.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        entity.Property(document => document.SourceType).HasColumnName("source_type").HasMaxLength(50).IsRequired();
        entity.Property(document => document.Content).HasColumnName("content").IsRequired();
        entity.Property(document => document.Summary).HasColumnName("summary");
        entity.Property(document => document.Tags)
            .HasColumnName("tags")
            .HasColumnType("jsonb")
            .HasConversion(v => JsonValueConversions.Serialize(v), v => JsonValueConversions.DeserializeList<string>(v))
            .Metadata.SetValueComparer(JsonValueConversions.ListComparer<string>());
        entity.Property(document => document.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb")
            .HasConversion(v => JsonValueConversions.Serialize(v), v => JsonValueConversions.DeserializeDictionary(v))
            .Metadata.SetValueComparer(JsonValueConversions.DictionaryComparer());
        entity.Property(document => document.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(document => document.ChunkingStatus).HasColumnName("chunking_status").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(document => document.EmbeddingStatus).HasColumnName("embedding_status").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(document => document.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();
        entity.Property(document => document.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(document => document.UpdatedAt).HasColumnName("updated_at").IsRequired();
        entity.Property(document => document.DeletedAt).HasColumnName("deleted_at");
        entity.Ignore(document => document.IsDeleted);
        entity.HasIndex(document => document.Status);
        entity.HasIndex(document => document.DeletedAt);
        entity.HasIndex(document => document.CreatedAt);
    }
}
