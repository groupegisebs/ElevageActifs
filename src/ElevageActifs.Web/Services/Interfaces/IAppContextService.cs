using ElevageActifs.Web.Models;

namespace ElevageActifs.Web.Services.Interfaces;

public interface IAppContextService
{
    Task<AppContextSnapshot> GetSnapshotAsync(CancellationToken cancellationToken = default);
    void InvalidateCache();
}
