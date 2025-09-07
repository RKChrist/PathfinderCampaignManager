using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.ValueObjects;

namespace PathfinderCampaignManager.Infrastructure.Persistence.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ToTable("Sessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasDefaultValueSql("NEWID()");

        builder.Property(s => s.Code)
            .HasMaxLength(6)
            .IsRequired()
            .HasConversion(
                code => code.Value,
                value => SessionCode.FromString(value));

        builder.HasIndex(s => s.Code)
            .IsUnique();

        builder.Property(s => s.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasMaxLength(500);

        builder.Property(s => s.SettingsJson)
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue("{}");

        builder.Property(s => s.DMUserId)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        builder.Property(s => s.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(s => s.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()")
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(s => s.RowVersion)
            .IsRowVersion();

        // Relationships
        builder.HasMany(s => s.Members)
            .WithOne(sm => sm.Session)
            .HasForeignKey(sm => sm.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Characters)
            .WithOne(c => c.Session!)
            .HasForeignKey(c => c.SessionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(s => s.Encounters)
            .WithOne(e => e.Session)
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SessionMemberConfiguration : IEntityTypeConfiguration<SessionMember>
{
    public void Configure(EntityTypeBuilder<SessionMember> builder)
    {
        builder.ToTable("SessionMembers");

        builder.HasKey(sm => new { sm.SessionId, sm.UserId });

        builder.Property(sm => sm.Alias)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(sm => sm.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(sm => sm.JoinedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(sm => new { sm.SessionId, sm.Alias })
            .IsUnique();
    }
}