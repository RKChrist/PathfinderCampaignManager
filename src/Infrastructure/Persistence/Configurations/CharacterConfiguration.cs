using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PathfinderCampaignManager.Domain.Entities;

namespace PathfinderCampaignManager.Infrastructure.Persistence.Configurations;

public class CharacterConfiguration : IEntityTypeConfiguration<Character>
{
    public void Configure(EntityTypeBuilder<Character> builder)
    {
        builder.ToTable("Characters");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(c => c.OwnerUserId)
            .IsRequired();

        builder.Property(c => c.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.Level)
            .HasDefaultValue(1);

        builder.Property(c => c.ClassRef)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.AncestryRef)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.BackgroundRef)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.AbilityScoresJson)
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue("{}");

        builder.Property(c => c.SkillsJson)
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue("{}");

        builder.Property(c => c.FeatsJson)
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue("{}");

        builder.Property(c => c.InventoryJson)
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue("{}");

        builder.Property(c => c.SpellsJson)
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue("{}");

        builder.Property(c => c.NotesJson)
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue("{}");

        builder.Property(c => c.Visibility)
            .HasConversion<int>()
            .HasDefaultValue(Domain.Enums.CharacterVisibility.Private);

        builder.Property(c => c.IsTemplate)
            .HasDefaultValue(false);

        builder.Property(c => c.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(c => c.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(c => c.RowVersion)
            .IsRowVersion();

        // Indexes
        builder.HasIndex(c => c.OwnerUserId);
        builder.HasIndex(c => c.SessionId);
        builder.HasIndex(c => new { c.OwnerUserId, c.Name })
            .IsUnique();

        // Relationships
        builder.HasMany(c => c.AuditLogs)
            .WithOne(al => al.Character)
            .HasForeignKey(al => al.CharacterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CharacterAuditLogConfiguration : IEntityTypeConfiguration<CharacterAuditLog>
{
    public void Configure(EntityTypeBuilder<CharacterAuditLog> builder)
    {
        builder.ToTable("CharacterAuditLogs");

        builder.HasKey(al => al.Id);

        builder.Property(al => al.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(al => al.UserId)
            .IsRequired();

        builder.Property(al => al.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(al => al.Timestamp)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(al => al.AdditionalData)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(al => al.CharacterId);
        builder.HasIndex(al => al.Timestamp);
    }
}