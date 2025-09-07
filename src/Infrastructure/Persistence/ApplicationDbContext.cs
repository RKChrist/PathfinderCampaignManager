using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Entities.Auth;

namespace PathfinderCampaignManager.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Session> Sessions { get; set; }
    public DbSet<SessionMember> SessionMembers { get; set; }
    public DbSet<Character> Characters { get; set; }
    public DbSet<CharacterAuditLog> CharacterAuditLogs { get; set; }
    public DbSet<Encounter> Encounters { get; set; }
    public DbSet<Combatant> Combatants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RulesVersion> RulesVersions { get; set; }
    public DbSet<ClassDef> ClassDefs { get; set; }
    public DbSet<AncestryDef> AncestryDefs { get; set; }
    public DbSet<FeatDef> FeatDefs { get; set; }
    public DbSet<SpellDef> SpellDefs { get; set; }
    public DbSet<NpcMonster> NpcMonsters { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all configurations
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure Identity tables with Guid keys
        ConfigureIdentityTables(builder);
    }

    private static void ConfigureIdentityTables(ModelBuilder builder)
    {
        // Identity table configurations
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(e => e.Id).HasColumnName("Id");
        });

        builder.Entity<IdentityRole<Guid>>(entity =>
        {
            entity.ToTable("Roles");
        });

        builder.Entity<IdentityUserRole<Guid>>(entity =>
        {
            entity.ToTable("UserRoles");
        });

        builder.Entity<IdentityUserClaim<Guid>>(entity =>
        {
            entity.ToTable("UserClaims");
        });

        builder.Entity<IdentityUserLogin<Guid>>(entity =>
        {
            entity.ToTable("UserLogins");
        });

        builder.Entity<IdentityRoleClaim<Guid>>(entity =>
        {
            entity.ToTable("RoleClaims");
        });

        builder.Entity<IdentityUserToken<Guid>>(entity =>
        {
            entity.ToTable("UserTokens");
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Handle domain events before saving
        var domainEntities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents.Count != 0);

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        domainEntities.ToList()
            .ForEach(entity => entity.Entity.ClearDomainEvents());

        var result = await base.SaveChangesAsync(cancellationToken);

        // TODO: Publish domain events using MediatR
        // await PublishDomainEventsAsync(domainEvents, cancellationToken);

        return result;
    }
}

public class ApplicationUser : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;
    public UserRole Role { get; set; } = UserRole.Player;

    // Navigation to domain user
    public User? DomainUser { get; set; }
}