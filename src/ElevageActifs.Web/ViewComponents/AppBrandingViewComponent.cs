using ElevageActifs.Web.Models;
using ElevageActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ElevageActifs.Web.ViewComponents;

public class AppBrandingViewComponent(IAppContextService appContextService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var snapshot = await appContextService.GetSnapshotAsync();
        return View(snapshot);
    }
}
