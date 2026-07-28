using ElevageActifs.Web.Models.Authorization;
using ElevageActifs.Web.Models.ViewModels;

namespace ElevageActifs.Web.Services.Interfaces;

public interface IPermissionAdminService
{
    Task<PermissionMatrixViewModel> GetMatrixAsync(CancellationToken cancellationToken = default);
    Task<HabilitationMatrixViewModel> GetHabilitationMatrixAsync(CancellationToken cancellationToken = default);
    Task<ModelPermissionViewModel> GetModelPermissionsAsync(string resource, CancellationToken cancellationToken = default);
    Task SaveRoleGrantsAsync(string roleId, IEnumerable<int> grantedPermissionIds, CancellationToken cancellationToken = default);
    Task SaveHabilitationMatrixAsync(IEnumerable<string> grantTokens, CancellationToken cancellationToken = default);
    Task EnsureSuperAdminGrantsAsync(CancellationToken cancellationToken = default);
    /// <summary>Accorde toutes les permissions actives d'une catégorie à un rôle (ex. User + Elevage).</summary>
    Task EnsureRoleCategoryGrantsAsync(string roleName, string category, CancellationToken cancellationToken = default);
}
