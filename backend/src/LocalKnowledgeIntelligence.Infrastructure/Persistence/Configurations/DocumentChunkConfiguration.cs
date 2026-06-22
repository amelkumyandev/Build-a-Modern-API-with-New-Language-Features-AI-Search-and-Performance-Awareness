using LocalKnowledgeIntelligence.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalKnowledgeIntelligence.Infrastructure.Persistence.Configurations;

public sealed class DocumentChunkConfiguration : IEntityTypeConfiguration<DocumentChunk>
{
    public void Configure(EntityTypeBuilder<DocumentChunk> entity)
    {
        entity.ToTable("document_chunks");
        entity.HasKey(chunk => chunk.Id);
        entity.Property(chunk => chunk.Id).HasColumnName("id");
        entity.Property(chunk => chunk.DocumentId).HasColumnName("document_id").IsRequired();
        entity.Property(chunk => chunk.ChunkIndex).HasColumnName("chunk_index").IsRequired();
        entity.Property(chunk => chunk.Content).HasColumnName("content").IsRequired();
        entity.Property(chunk => chunk.TokenEstimate).HasColumnName("token_estimate").IsRequired();
        entity.Property(chunk => chunk.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb")
            .HasConversion(v => JsonValueConversions.Serialize(v), v => JsonValueConversions.DeserializeDictionary(v))
            .Metadata.SetValueComparer(JsonValueConversions.DictionaryComparer());
        entity.Property(chunk => chunk.EmbeddingModel).HasColumnName("embedding_model");
        entity.Property(chunk => chunk.EmbeddingDimensions).HasColumnName("embedding_dimensions");
        entity.Property(chunk => chunk.EmbeddingGeneratedAt).HasColumnName("embedding_generated_at");
        entity.Property(chunk => chunk.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(chunk => chunk.UpdatedAt).HasColumnName("updated_at").IsRequired();
        entity.HasOne(chunk => chunk.Document)
            .WithMany()
            .HasForeignKey(chunk => chunk.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
        entity.HasIndex(chunk => new { chunk.DocumentId, chunk.ChunkIndex }).IsUnique();
    }
}
