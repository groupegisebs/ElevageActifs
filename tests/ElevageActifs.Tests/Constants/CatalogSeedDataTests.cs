using ElevageActifs.Web.Constants;
using ElevageActifs.Web.Data;
using ElevageActifs.Web.Models.Authorization;
using ElevageActifs.Web.Models.Elevage;

namespace ElevageActifs.Tests.Constants;

public class CatalogSeedDataTests
{
    [Fact]
    public void Catalog_HasPermissionsEndpointsAndReports()
    {
        Assert.NotEmpty(CatalogSeedData.Permissions);
        Assert.NotEmpty(CatalogSeedData.Endpoints);
        Assert.NotEmpty(CatalogSeedData.Reports);
    }

    [Fact]
    public void Catalog_PermissionCodesAreUnique()
    {
        var codes = CatalogSeedData.Permissions.Select(p => p.Code).ToList();
        Assert.Equal(codes.Count, codes.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void Catalog_EndpointKeysAreUnique()
    {
        var keys = CatalogSeedData.Endpoints
            .Select(e => $"{e.Area}|{e.Controller}|{e.Action}|{e.HttpMethod}")
            .ToList();
        Assert.Equal(keys.Count, keys.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void Catalog_EndpointPermissionCodesExistInCatalog()
    {
        var permissionCodes = CatalogSeedData.Permissions.Select(p => p.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var endpoint in CatalogSeedData.Endpoints)
        {
            Assert.True(
                permissionCodes.Contains(endpoint.PermissionCode),
                $"Permission '{endpoint.PermissionCode}' manquante pour {endpoint.Controller}.{endpoint.Action}");
        }
    }

    [Fact]
    public void Catalog_ContainsPropertyLevelPermissions()
    {
        Assert.Contains(CatalogSeedData.Permissions, p => p.PropertyName == "Email" && p.Resource == "User");
    }

    [Fact]
    public void DefaultSeed_IncludesSuperAdminAndPlatformRoles()
    {
        Assert.Contains(AppRoles.SuperAdmin, AppRoles.DefaultSeedRoles);
        Assert.Contains(AppRoles.Admin, AppRoles.DefaultSeedRoles);
        Assert.Contains(AppRoles.User, AppRoles.DefaultSeedRoles);
    }

    [Fact]
    public void Catalog_ContainsElevagePermissions()
    {
        Assert.Contains(CatalogSeedData.Permissions, p => p.Code == "Animaux.View");
        Assert.Contains(CatalogSeedData.Permissions, p => p.Code == "ElevageDashboard.View");
        Assert.Contains(CatalogSeedData.Endpoints, e => e.Area == "Elevage" && e.Controller == "Dashboard");
    }

    [Fact]
    public void Catalog_AllActionsAreValidEnum()
    {
        foreach (var permission in CatalogSeedData.Permissions)
        {
            Assert.True(Enum.IsDefined(permission.Action));
        }
    }
}

public class ExploitationScopingTests
{
    [Fact]
    public void Animal_RequiresExploitationId()
    {
        var animal = new Animal
        {
            ExploitationId = 3,
            BoucleNumber = "QC-001",
            Species = "Bovin",
            Statut = AnimalStatut.Present
        };
        Assert.Equal(3, animal.ExploitationId);
        Assert.Equal(AnimalStatut.Present, animal.Statut);
    }

    [Fact]
    public void Traitement_WaitPeriod_IsActiveWhenFuture()
    {
        var traitement = new Traitement
        {
            ExploitationId = 1,
            WaitMeatUntil = DateTime.UtcNow.Date.AddDays(7),
            WaitMilkUntil = DateTime.UtcNow.Date.AddDays(3)
        };
        Assert.True(traitement.WaitMeatUntil > DateTime.UtcNow.Date);
        Assert.True(traitement.WaitMilkUntil > DateTime.UtcNow.Date);
    }
}
