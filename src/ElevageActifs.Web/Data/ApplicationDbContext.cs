using ElevageActifs.Web.Configuration;
using ElevageActifs.Web.Extensions;
using ElevageActifs.Web.Models;
using ElevageActifs.Web.Models.Authorization;
using ElevageActifs.Web.Models.Elevage;
using ElevageActifs.Web.Models.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ElevageActifs.Web.Data;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IOptions<DatabaseOptions> databaseOptions)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    private readonly string _schema = DatabaseExtensions.NormalizeSchema(databaseOptions.Value.Schema);

    public string Schema => _schema;
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<SystemSettings> SystemSettings => Set<SystemSettings>();
    public DbSet<ReportDefinition> ReportDefinitions => Set<ReportDefinition>();
    public DbSet<PermissionDefinition> PermissionDefinitions => Set<PermissionDefinition>();
    public DbSet<RolePermissionGrant> RolePermissionGrants => Set<RolePermissionGrant>();
    public DbSet<SecuredEndpoint> SecuredEndpoints => Set<SecuredEndpoint>();
    public DbSet<ThemeDefinition> ThemeDefinitions => Set<ThemeDefinition>();

    public DbSet<Exploitation> Exploitations => Set<Exploitation>();
    public DbSet<ExploitationUser> ExploitationUsers => Set<ExploitationUser>();
    public DbSet<Troupeau> Troupeaux => Set<Troupeau>();
    public DbSet<Lot> Lots => Set<Lot>();
    public DbSet<Enclos> Enclos => Set<Enclos>();
    public DbSet<Animal> Animaux => Set<Animal>();
    public DbSet<AnimalEvenement> AnimalEvenements => Set<AnimalEvenement>();
    public DbSet<ProtocoleSanitaire> ProtocolesSanitaires => Set<ProtocoleSanitaire>();
    public DbSet<Traitement> Traitements => Set<Traitement>();
    public DbSet<EvenementReproduction> EvenementsReproduction => Set<EvenementReproduction>();
    public DbSet<ActifMateriel> ActifsMateriel => Set<ActifMateriel>();
    public DbSet<StockArticle> StockArticles => Set<StockArticle>();
    public DbSet<StockMouvement> StockMouvements => Set<StockMouvement>();
    public DbSet<InterventionMaintenance> Interventions => Set<InterventionMaintenance>();
    public DbSet<Fournisseur> Fournisseurs => Set<Fournisseur>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        if (UseExplicitSchema())
            builder.HasDefaultSchema(_schema);

        base.OnModelCreating(builder);

        builder.Entity<UserProfile>(entity =>
        {
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasOne(x => x.User)
                .WithOne(x => x.Profile)
                .HasForeignKey<UserProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.UserId);
        });

        builder.Entity<ReportDefinition>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.RequiredPermissionCode);
            entity.HasOne(x => x.RequiredPermission)
                .WithMany()
                .HasForeignKey(x => x.RequiredPermissionCode)
                .HasPrincipalKey(p => p.Code)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PermissionDefinition>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => new { x.Resource, x.Action, x.PropertyName });
        });

        builder.Entity<RolePermissionGrant>(entity =>
        {
            entity.HasIndex(x => new { x.RoleId, x.PermissionDefinitionId }).IsUnique();
            entity.HasOne(x => x.Permission)
                .WithMany(x => x.RoleGrants)
                .HasForeignKey(x => x.PermissionDefinitionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SecuredEndpoint>(entity =>
        {
            entity.HasIndex(x => new { x.Area, x.Controller, x.Action, x.HttpMethod }).IsUnique();
            entity.HasOne(x => x.Permission)
                .WithMany()
                .HasForeignKey(x => x.PermissionDefinitionId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<SystemSettings>(entity =>
        {
            entity.HasOne<ThemeDefinition>()
                .WithMany()
                .HasForeignKey(x => x.ActiveThemeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasData(new SystemSettings
            {
                Id = 1,
                ActiveThemeId = 1,
                DefaultCulture = "fr-FR",
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        });

        builder.Entity<ThemeDefinition>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            foreach (var theme in ThemeDefaults.SeedThemes)
            {
                entity.HasData(new ThemeDefinition
                {
                    Id = theme.Id,
                    Code = theme.Code,
                    Name = theme.Name,
                    Description = theme.Description,
                    CssVariables = theme.CssVariables,
                    IsSystem = true,
                    IsActive = true,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });
            }
        });

        builder.Entity<Exploitation>(entity =>
        {
            entity.Property(x => x.TotalAreaHa).HasPrecision(18, 2);
        });

        builder.Entity<ExploitationUser>(entity =>
        {
            entity.HasIndex(x => new { x.ExploitationId, x.UserId }).IsUnique();
            entity.HasOne(x => x.Exploitation)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.ExploitationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Troupeau>(entity =>
        {
            entity.HasIndex(x => new { x.ExploitationId, x.Code }).IsUnique();
            entity.HasOne(x => x.Exploitation)
                .WithMany(x => x.Troupeaux)
                .HasForeignKey(x => x.ExploitationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Lot>(entity =>
        {
            entity.HasIndex(x => new { x.TroupeauId, x.Code }).IsUnique();
            entity.HasOne(x => x.Troupeau)
                .WithMany(x => x.Lots)
                .HasForeignKey(x => x.TroupeauId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Enclos>(entity =>
        {
            entity.HasIndex(x => new { x.ExploitationId, x.Code }).IsUnique();
            entity.HasOne(x => x.Exploitation)
                .WithMany(x => x.Enclos)
                .HasForeignKey(x => x.ExploitationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Animal>(entity =>
        {
            entity.HasIndex(x => new { x.ExploitationId, x.BoucleNumber }).IsUnique();
            entity.HasOne(x => x.Exploitation)
                .WithMany(x => x.Animaux)
                .HasForeignKey(x => x.ExploitationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Mother)
                .WithMany()
                .HasForeignKey(x => x.MotherId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Father)
                .WithMany()
                .HasForeignKey(x => x.FatherId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Troupeau)
                .WithMany(x => x.Animaux)
                .HasForeignKey(x => x.TroupeauId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Lot)
                .WithMany(x => x.Animaux)
                .HasForeignKey(x => x.LotId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Enclos)
                .WithMany(x => x.Animaux)
                .HasForeignKey(x => x.EnclosId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<AnimalEvenement>(entity =>
        {
            entity.Property(x => x.WeightKg).HasPrecision(18, 2);
            entity.HasOne(x => x.Animal)
                .WithMany(x => x.Evenements)
                .HasForeignKey(x => x.AnimalId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProtocoleSanitaire>(entity =>
        {
            entity.HasOne(x => x.Exploitation)
                .WithMany(x => x.ProtocolesSanitaires)
                .HasForeignKey(x => x.ExploitationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Traitement>(entity =>
        {
            entity.HasOne(x => x.Exploitation)
                .WithMany(x => x.Traitements)
                .HasForeignKey(x => x.ExploitationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Animal)
                .WithMany(x => x.Traitements)
                .HasForeignKey(x => x.AnimalId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.Lot)
                .WithMany(x => x.Traitements)
                .HasForeignKey(x => x.LotId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(x => x.ProtocoleSanitaire)
                .WithMany(x => x.Traitements)
                .HasForeignKey(x => x.ProtocoleSanitaireId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<EvenementReproduction>(entity =>
        {
            entity.HasOne(x => x.Exploitation)
                .WithMany(x => x.EvenementsReproduction)
                .HasForeignKey(x => x.ExploitationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Animal)
                .WithMany(x => x.EvenementsReproduction)
                .HasForeignKey(x => x.AnimalId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ActifMateriel>(entity =>
        {
            entity.HasIndex(x => new { x.ExploitationId, x.InternalCode }).IsUnique();
            entity.Property(x => x.AcquisitionValue).HasPrecision(18, 2);
            entity.Property(x => x.ResidualValue).HasPrecision(18, 2);
            entity.HasOne(x => x.Exploitation)
                .WithMany(x => x.Actifs)
                .HasForeignKey(x => x.ExploitationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Enclos)
                .WithMany()
                .HasForeignKey(x => x.EnclosId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<StockArticle>(entity =>
        {
            entity.HasIndex(x => new { x.ExploitationId, x.Sku }).IsUnique();
            entity.Property(x => x.QuantityOnHand).HasPrecision(18, 3);
            entity.Property(x => x.ReorderLevel).HasPrecision(18, 3);
            entity.Property(x => x.UnitCost).HasPrecision(18, 2);
            entity.HasOne(x => x.Exploitation)
                .WithMany(x => x.Stocks)
                .HasForeignKey(x => x.ExploitationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<StockMouvement>(entity =>
        {
            entity.Property(x => x.Quantity).HasPrecision(18, 3);
            entity.HasOne(x => x.StockArticle)
                .WithMany(x => x.Mouvements)
                .HasForeignKey(x => x.StockArticleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<InterventionMaintenance>(entity =>
        {
            entity.Property(x => x.LaborCost).HasPrecision(18, 2);
            entity.Property(x => x.PartsCost).HasPrecision(18, 2);
            entity.HasOne(x => x.Exploitation)
                .WithMany(x => x.Interventions)
                .HasForeignKey(x => x.ExploitationId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Actif)
                .WithMany(x => x.Interventions)
                .HasForeignKey(x => x.ActifMaterielId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Fournisseur>(entity =>
        {
            entity.HasOne(x => x.Exploitation)
                .WithMany(x => x.Fournisseurs)
                .HasForeignKey(x => x.ExploitationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private bool UseExplicitSchema() =>
        !string.Equals(_schema, "public", StringComparison.OrdinalIgnoreCase)
        && !string.Equals(_schema, "dbo", StringComparison.OrdinalIgnoreCase);
}
