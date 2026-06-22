using LocalKnowledgeIntelligence.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocalKnowledgeIntelligence.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("users");
        entity.HasKey(user => user.Id);
        entity.Property(user => user.Id).HasColumnName("id");
        entity.Property(user => user.Username).HasColumnName("username").HasMaxLength(80).IsRequired();
        entity.Property(user => user.PasswordHash).HasColumnName("password_hash").IsRequired();
        entity.Property(user => user.Role).HasColumnName("role").HasConversion<string>().HasMaxLength(30).IsRequired();
        entity.Property(user => user.CreatedAt).HasColumnName("created_at").IsRequired();
        entity.Property(user => user.UpdatedAt).HasColumnName("updated_at").IsRequired();
        entity.HasIndex(user => user.Username).IsUnique();
    }
}
