using ElevageActifs.Web.Configuration;
using ElevageActifs.Web.Constants;
using ElevageActifs.Web.Extensions;
using ElevageActifs.Web.Models.Authorization;
using ElevageActifs.Web.Models.Elevage;
using ElevageActifs.Web.Models.Identity;
using ElevageActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ElevageActifs.Web.Data;

public static class SeedData
{
    private const string SuperAdminEmail = "superadmin@elevageactifs.local";
    private const string SuperAdminPassword = "Elevage@Secure2026!";
    private const string AdminEmail = "admin@elevageactifs.local";
    private const string AdminPassword = "Elevage@Admin2026!";
    private const string DemoPassword = "Demo@Elevage2026!";

    /// <summary>Date UTC à minuit — Npgsql refuse Kind=Unspecified sur timestamptz.</summary>
    private static DateTime UtcDate(int year, int month, int day) =>
        new(year, month, day, 0, 0, 0, DateTimeKind.Utc);

    private static DateTime UtcToday() =>
        DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var context = provider.GetRequiredService<ApplicationDbContext>();
        var dbOptions = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        var configuration = provider.GetRequiredService<IConfiguration>();

        await EnsureSchemaAsync(context, dbOptions);
        await context.Database.MigrateAsync();

        var roleManager = provider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var permissionAdmin = provider.GetRequiredService<IPermissionAdminService>();

        await SeedRolesAsync(roleManager);
        await EnsureCatalogAsync(context);
        await SeedSuperAdminAsync(userManager);
        await EnsureUserAsync(userManager, AdminEmail, AdminPassword, "Admin", "Elevage", AppRoles.Admin);
        await permissionAdmin.EnsureSuperAdminGrantsAsync();
        await permissionAdmin.EnsureRoleCategoryGrantsAsync(AppRoles.User, "Elevage");
        await permissionAdmin.EnsureRoleCategoryGrantsAsync(AppRoles.Admin, "Elevage");

        if (configuration.GetValue("Seed:IncludeDemoData", true))
            await SeedDemoAsync(context, userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        foreach (var roleName in AppRoles.DefaultSeedRoles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            await roleManager.CreateAsync(new ApplicationRole
            {
                Name = roleName,
                Description = "Rôle fondateur — seul rôle créé automatiquement. Les autres rôles se créent via Admin > Rôles.",
                IsSystemRole = true
            });
        }
    }

    private static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager) =>
        await EnsureUserAsync(userManager, SuperAdminEmail, SuperAdminPassword, "Super", "Admin", AppRoles.SuperAdmin);

    private static async Task EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string firstName,
        string lastName,
        string role)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
            return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, role);
    }

    public static async Task EnsureCatalogAsync(ApplicationDbContext context)
    {
        var schema = SqlIdent(context.Schema);
        var permissions = $"{schema}.\"PermissionDefinitions\"";
        var endpoints = $"{schema}.\"SecuredEndpoints\"";
        var reports = $"{schema}.\"ReportDefinitions\"";

        foreach (var permission in CatalogSeedData.Permissions)
        {
            var propertyName = permission.PropertyName is null
                ? "NULL"
                : $"'{SqlLiteral(permission.PropertyName)}'";

#pragma warning disable EF1002
            await context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {permissions} ("Code", "Resource", "Action", "PropertyName", "DisplayName", "Category", "IsSystem", "IsActive")
                SELECT '{SqlLiteral(permission.Code)}', '{SqlLiteral(permission.Resource)}', {(int)permission.Action}, {propertyName}, '{SqlLiteral(permission.DisplayName)}', '{SqlLiteral(permission.Category)}', TRUE, TRUE
                WHERE NOT EXISTS (SELECT 1 FROM {permissions} WHERE "Code" = '{SqlLiteral(permission.Code)}');
                """);
#pragma warning restore EF1002
        }

        var catalogEndpoints = CatalogSeedData.Endpoints
            .GroupBy(e => $"{e.Area}|{e.Controller}|{e.Action}|{e.HttpMethod}", StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First());

        foreach (var endpoint in catalogEndpoints)
        {
            var area = endpoint.Area is null ? "NULL" : $"'{SqlLiteral(endpoint.Area)}'";
            var httpMethod = endpoint.HttpMethod is null ? "NULL" : $"'{SqlLiteral(endpoint.HttpMethod)}'";
            var areaMatch = endpoint.Area is null
                ? "e.\"Area\" IS NULL"
                : $"e.\"Area\" = '{SqlLiteral(endpoint.Area)}'";
            var httpMatch = endpoint.HttpMethod is null
                ? "e.\"HttpMethod\" IS NULL"
                : $"e.\"HttpMethod\" = '{SqlLiteral(endpoint.HttpMethod)}'";

#pragma warning disable EF1002
            await context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {endpoints} ("Area", "Controller", "Action", "HttpMethod", "PermissionDefinitionId", "IsActive")
                SELECT {area}, '{SqlLiteral(endpoint.Controller)}', '{SqlLiteral(endpoint.Action)}', {httpMethod}, p."Id", TRUE
                FROM {permissions} p
                WHERE p."Code" = '{SqlLiteral(endpoint.PermissionCode)}'
                AND NOT EXISTS (
                    SELECT 1 FROM {endpoints} e
                    WHERE {areaMatch}
                      AND e."Controller" = '{SqlLiteral(endpoint.Controller)}'
                      AND e."Action" = '{SqlLiteral(endpoint.Action)}'
                      AND {httpMatch});
                """);
#pragma warning restore EF1002
        }

        foreach (var report in CatalogSeedData.Reports)
        {
#pragma warning disable EF1002
            await context.Database.ExecuteSqlRawAsync($"""
                INSERT INTO {reports} ("Code", "Name", "Category", "RequiredPermissionCode", "IsActive")
                SELECT '{SqlLiteral(report.Code)}', '{SqlLiteral(report.Name)}', '{SqlLiteral(report.Category)}', '{SqlLiteral(report.RequiredPermissionCode)}', TRUE
                WHERE NOT EXISTS (SELECT 1 FROM {reports} WHERE "Code" = '{SqlLiteral(report.Code)}');
                """);
#pragma warning restore EF1002
        }
    }

    private static string SqlIdent(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";

    private static string SqlLiteral(string value) => value.Replace("'", "''", StringComparison.Ordinal);

    private static async Task SeedDemoAsync(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        if (await db.Exploitations.AnyAsync(e => e.Name == "Ranch Belle-Rivière"))
            return;

        var exploitation = new Exploitation
        {
            Name = "Ranch Belle-Rivière",
            Address = "450 Chemin de la Rivière",
            City = "Thetford Mines",
            Province = "QC",
            PostalCode = "G6G 1A1",
            TotalAreaHa = 120,
            ProductionType = "Bovins laitiers",
            Email = "info@belleriviere.demo"
        };
        db.Exploitations.Add(exploitation);
        await db.SaveChangesAsync();

        var demoUsers = new (string Email, string First, string Last, ExploitationUserRole Role)[]
        {
            ("gerant@belleriviere.demo", "Marc", "Gérant", ExploitationUserRole.Gerant),
            ("zoo@belleriviere.demo", "Isabelle", "Zootechnicienne", ExploitationUserRole.Zootechnicien),
            ("tech@belleriviere.demo", "Luc", "Technicien", ExploitationUserRole.Technicien),
            ("ouvrier@belleriviere.demo", "Jean", "Ouvrier", ExploitationUserRole.Ouvrier),
            ("lecture@belleriviere.demo", "Claire", "Lecture", ExploitationUserRole.Observateur)
        };

        foreach (var (email, first, last, role) in demoUsers)
        {
            await EnsureUserAsync(userManager, email, DemoPassword, first, last, AppRoles.User);
            var user = await userManager.FindByEmailAsync(email);
            if (user is null) continue;
            db.ExploitationUsers.Add(new ExploitationUser
            {
                ExploitationId = exploitation.Id,
                UserId = user.Id,
                Role = role
            });
        }

        var super = await userManager.FindByEmailAsync(SuperAdminEmail);
        if (super is not null)
        {
            db.ExploitationUsers.Add(new ExploitationUser
            {
                ExploitationId = exploitation.Id,
                UserId = super.Id,
                Role = ExploitationUserRole.Proprietaire
            });
        }

        var troupeaux = new[]
        {
            new Troupeau { ExploitationId = exploitation.Id, Code = "T01", Name = "Troupeau laitier principal", Species = "Bovin" },
            new Troupeau { ExploitationId = exploitation.Id, Code = "T02", Name = "Génisses", Species = "Bovin" },
            new Troupeau { ExploitationId = exploitation.Id, Code = "T03", Name = "Veaux", Species = "Bovin" }
        };
        db.Troupeaux.AddRange(troupeaux);
        await db.SaveChangesAsync();

        var lots = new[]
        {
            new Lot { TroupeauId = troupeaux[0].Id, Code = "L01", Name = "Vaches laitières A" },
            new Lot { TroupeauId = troupeaux[0].Id, Code = "L02", Name = "Vaches laitières B" },
            new Lot { TroupeauId = troupeaux[1].Id, Code = "L03", Name = "Génisses 2025" },
            new Lot { TroupeauId = troupeaux[2].Id, Code = "L04", Name = "Veaux sevrés" }
        };
        db.Lots.AddRange(lots);
        await db.SaveChangesAsync();

        var enclos = new[]
        {
            new Enclos { ExploitationId = exploitation.Id, Code = "E01", Name = "Étable principale", Capacity = 80 },
            new Enclos { ExploitationId = exploitation.Id, Code = "E02", Name = "Parc génisses", Capacity = 40 },
            new Enclos { ExploitationId = exploitation.Id, Code = "E03", Name = "Parc veaux", Capacity = 25 },
            new Enclos { ExploitationId = exploitation.Id, Code = "E04", Name = "Salle de traite", Capacity = 24 }
        };
        db.Enclos.AddRange(enclos);
        await db.SaveChangesAsync();

        var animals = new List<Animal>();
        for (var i = 1; i <= 25; i++)
        {
            var troupeau = i <= 15 ? troupeaux[0] : (i <= 20 ? troupeaux[1] : troupeaux[2]);
            var lot = i <= 10 ? lots[0] : (i <= 15 ? lots[1] : (i <= 20 ? lots[2] : lots[3]));
            var enclosRef = i <= 15 ? enclos[0] : (i <= 20 ? enclos[1] : enclos[2]);
            animals.Add(new Animal
            {
                ExploitationId = exploitation.Id,
                BoucleNumber = $"QC{i:D6}",
                RfidTag = i <= 10 ? $"RFID-{i:D4}" : null,
                Species = "Bovin",
                Race = i % 3 == 0 ? "Holstein" : (i % 3 == 1 ? "Jersey" : "Simmental"),
                Sex = i <= 20 ? "F" : "M",
                BirthDate = UtcToday().AddYears(-3).AddDays(i * 12),
                Statut = AnimalStatut.Present,
                TroupeauId = troupeau.Id,
                LotId = lot.Id,
                EnclosId = enclosRef.Id
            });
        }
        db.Animaux.AddRange(animals);
        await db.SaveChangesAsync();

        db.AnimalEvenements.AddRange(
            new AnimalEvenement { AnimalId = animals[0].Id, Type = AnimalEvenementType.Entree, EventDate = UtcToday().AddMonths(-6), Notes = "Achat" },
            new AnimalEvenement { AnimalId = animals[0].Id, Type = AnimalEvenementType.Pesee, EventDate = UtcToday().AddDays(-7), WeightKg = 625 },
            new AnimalEvenement { AnimalId = animals[1].Id, Type = AnimalEvenementType.Pesee, EventDate = UtcToday().AddDays(-14), WeightKg = 580 },
            new AnimalEvenement { AnimalId = animals[5].Id, Type = AnimalEvenementType.Observation, EventDate = UtcToday().AddDays(-2), Notes = "Boiterie légère" });

        var protocoles = new[]
        {
            new ProtocoleSanitaire { ExploitationId = exploitation.Id, Name = "Vaccination annuelle", Description = "Protocole vaccinal bovins laitiers", Species = "Bovin" },
            new ProtocoleSanitaire { ExploitationId = exploitation.Id, Name = "Traitement mammites", Description = "Protocole mammites cliniques", Species = "Bovin" }
        };
        db.ProtocolesSanitaires.AddRange(protocoles);
        await db.SaveChangesAsync();

        var today = UtcToday();
        db.Traitements.AddRange(
            new Traitement { ExploitationId = exploitation.Id, AnimalId = animals[0].Id, ProtocoleSanitaireId = protocoles[0].Id, Product = "Vaccin IBR-BVD", Dose = "2 ml", AdministeredAt = today.AddDays(-30) },
            new Traitement { ExploitationId = exploitation.Id, LotId = lots[0].Id, Product = "Antiparasitaire", Dose = "10 ml/animal", AdministeredAt = today.AddDays(-15), WaitMilkUntil = today.AddDays(5), WaitMeatUntil = today.AddDays(28) },
            new Traitement { ExploitationId = exploitation.Id, AnimalId = animals[5].Id, Product = "Antibiotique mammites", Dose = "5 ml", AdministeredAt = today.AddDays(-3), WaitMilkUntil = today.AddDays(4), WaitMeatUntil = today.AddDays(14) },
            new Traitement { ExploitationId = exploitation.Id, AnimalId = animals[2].Id, Product = "Vitamine ADE", Dose = "3 ml", AdministeredAt = today.AddDays(-45) },
            new Traitement { ExploitationId = exploitation.Id, LotId = lots[2].Id, Product = "Vermifuge", Dose = "8 ml", AdministeredAt = today.AddDays(-10), WaitMeatUntil = today.AddDays(21) });

        db.EvenementsReproduction.AddRange(
            new EvenementReproduction { ExploitationId = exploitation.Id, AnimalId = animals[0].Id, Type = ReproductionType.Chaleur, StartDate = today.AddDays(-20) },
            new EvenementReproduction { ExploitationId = exploitation.Id, AnimalId = animals[1].Id, Type = ReproductionType.IA, StartDate = today.AddDays(-15), Notes = "Semence Holstein" },
            new EvenementReproduction { ExploitationId = exploitation.Id, AnimalId = animals[2].Id, Type = ReproductionType.Gestation, StartDate = today.AddDays(-60), EndDate = today.AddDays(220) });

        var actifs = new[]
        {
            new ActifMateriel { ExploitationId = exploitation.Id, InternalCode = "BA-01", Name = "Étable principale", Categorie = ActifCategorie.BatimentElevage, AcquisitionValue = 450000, AcquisitionDate = UtcDate(2010, 5, 1), EnclosId = enclos[0].Id },
            new ActifMateriel { ExploitationId = exploitation.Id, InternalCode = "TR-01", Name = "Robot de traite Lely", Categorie = ActifCategorie.MaterielTraite, Brand = "Lely", Model = "A5", Year = 2021, AcquisitionValue = 185000, EnclosId = enclos[3].Id },
            new ActifMateriel { ExploitationId = exploitation.Id, InternalCode = "AL-01", Name = "Mélangeur vertical", Categorie = ActifCategorie.Alimentation, Brand = "Trioliet", AcquisitionValue = 42000, Year = 2019 },
            new ActifMateriel { ExploitationId = exploitation.Id, InternalCode = "CO-01", Name = "Contenants silo grains", Categorie = ActifCategorie.Contenue, AcquisitionValue = 65000, Year = 2015 },
            new ActifMateriel { ExploitationId = exploitation.Id, InternalCode = "MR-01", Name = "Chargeur frontal", Categorie = ActifCategorie.MaterielRoulant, Brand = "John Deere", Model = "544K", Year = 2018, AcquisitionValue = 95000 },
            new ActifMateriel { ExploitationId = exploitation.Id, InternalCode = "SA-01", Name = "Équipement insémination", Categorie = ActifCategorie.EquipementSante, AcquisitionValue = 3500, Year = 2022 }
        };
        db.ActifsMateriel.AddRange(actifs);
        await db.SaveChangesAsync();

        db.StockArticles.AddRange(
            new StockArticle { ExploitationId = exploitation.Id, Sku = "ALI-FOIN", Name = "Foin première coupe", Categorie = StockCategorie.Alimentation, Unit = "balle", QuantityOnHand = 120, ReorderLevel = 50, UnitCost = 8 },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "ALI-GRAIN", Name = "Mélange lactation", Categorie = StockCategorie.Alimentation, Unit = "t", QuantityOnHand = 8, ReorderLevel = 3, UnitCost = 420 },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "ALI-MIN", Name = "Minéral bovin", Categorie = StockCategorie.Alimentation, Unit = "sac", QuantityOnHand = 15, ReorderLevel = 8, UnitCost = 45 },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "MED-ANTB", Name = "Antibiotique intrammamire", Categorie = StockCategorie.Medicament, Unit = "tube", QuantityOnHand = 12, ReorderLevel = 10, UnitCost = 18, ExpirationDate = today.AddMonths(8) },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "MED-VACC", Name = "Vaccin IBR-BVD", Categorie = StockCategorie.Medicament, Unit = "flacon", QuantityOnHand = 4, ReorderLevel = 5, UnitCost = 85, ExpirationDate = today.AddMonths(4) },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "HYG-DES", Name = "Désinfectant salle traite", Categorie = StockCategorie.Hygiene, Unit = "L", QuantityOnHand = 25, ReorderLevel = 15, UnitCost = 12 },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "HYG-GANT", Name = "Gants obstétriques", Categorie = StockCategorie.Hygiene, Unit = "boîte", QuantityOnHand = 2, ReorderLevel = 3, UnitCost = 22 },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "PIE-FIL", Name = "Filtres traite", Categorie = StockCategorie.Pieces, Unit = "u", QuantityOnHand = 8, ReorderLevel = 5, UnitCost = 15 },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "CON-SEM", Name = "Semence Holstein", Categorie = StockCategorie.Consommable, Unit = "dose", QuantityOnHand = 20, ReorderLevel = 10, UnitCost = 35 },
            new StockArticle { ExploitationId = exploitation.Id, Sku = "CON-ETIQ", Name = "Étiquettes boucles", Categorie = StockCategorie.Consommable, Unit = "rouleau", QuantityOnHand = 3, ReorderLevel = 2, UnitCost = 28 });

        db.Fournisseurs.AddRange(
            new Fournisseur { ExploitationId = exploitation.Id, Name = "Nutrition Bovine Québec", ContactName = "Paul Tremblay", Email = "ventes@nbq.demo", Phone = "418-555-0101" },
            new Fournisseur { ExploitationId = exploitation.Id, Name = "Vétérinaire des Cantons", ContactName = "Dr Sophie Roy", Email = "clinique@vdc.demo", Phone = "418-555-0102" },
            new Fournisseur { ExploitationId = exploitation.Id, Name = "Équipement Laitier Inc.", ContactName = "Marc Lapointe", Email = "service@eli.demo", Phone = "418-555-0103" });

        db.Interventions.AddRange(
            new InterventionMaintenance { ExploitationId = exploitation.Id, ActifMaterielId = actifs[1].Id, Title = "Entretien robot traite", Type = InterventionType.Preventif, Statut = InterventionStatut.Ouverte, PlannedDate = today.AddDays(5), LaborCost = 450, Description = "Inspection annuelle" },
            new InterventionMaintenance { ExploitationId = exploitation.Id, ActifMaterielId = actifs[4].Id, Title = "Réparation chargeur", Type = InterventionType.Correctif, Statut = InterventionStatut.EnCours, PlannedDate = today.AddDays(-2), LaborCost = 600, PartsCost = 320 });

        actifs[4].Statut = ActifStatut.EnMaintenance;
        await db.SaveChangesAsync();
    }

    private static async Task EnsureSchemaAsync(ApplicationDbContext context, DatabaseOptions dbOptions)
    {
        var provider = dbOptions.Provider.Trim().ToUpperInvariant();
        if (provider is not ("POSTGRESQL" or "POSTGRES" or "NPGSQL"))
            return;

        var schema = DatabaseExtensions.NormalizeSchema(dbOptions.Schema);
        if (string.Equals(schema, "public", StringComparison.OrdinalIgnoreCase))
            return;

#pragma warning disable EF1002
        await context.Database.ExecuteSqlRawAsync($"CREATE SCHEMA IF NOT EXISTS \"{schema.Replace("\"", "\"\"")}\"");
#pragma warning restore EF1002
    }
}
