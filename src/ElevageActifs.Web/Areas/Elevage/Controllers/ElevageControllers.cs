using ElevageActifs.Web.Data;
using ElevageActifs.Web.Models.Elevage;
using ElevageActifs.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace ElevageActifs.Web.Areas.Elevage.Controllers;

[Area("Elevage")]
[Authorize]
public abstract class ElevageControllerBase(IExploitationContextService exploitationContext) : Controller
{
    protected Task<int> GetExploitationIdAsync(CancellationToken cancellationToken = default) =>
        exploitationContext.GetCurrentExploitationIdAsync(cancellationToken);

    protected string? CurrentUserId =>
        User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
}

[Area("Elevage")]
public class ContextController(IExploitationContextService exploitationContext) : ElevageControllerBase(exploitationContext)
{
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetExploitation(int exploitationId, string? returnUrl, CancellationToken cancellationToken)
    {
        await exploitationContext.EnsureAccessAsync(exploitationId, cancellationToken);
        await exploitationContext.SetCurrentExploitationIdAsync(exploitationId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Dashboard");
    }
}

[Area("Elevage")]
public class DashboardController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : ElevageControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var exploitation = await db.Exploitations.AsNoTracking()
            .FirstAsync(e => e.Id == exploitationId, cancellationToken);

        var today = DateTime.UtcNow.Date;
        ViewBag.ExploitationName = exploitation.Name;
        ViewBag.EffectifsPresent = await db.Animaux.CountAsync(a => a.ExploitationId == exploitationId && a.Statut == AnimalStatut.Present, cancellationToken);
        ViewBag.TraitementsAttente = await db.Traitements.CountAsync(t =>
            t.ExploitationId == exploitationId &&
            ((t.WaitMilkUntil != null && t.WaitMilkUntil >= today) || (t.WaitMeatUntil != null && t.WaitMeatUntil >= today)), cancellationToken);
        ViewBag.GestationsOuvertes = await db.EvenementsReproduction.CountAsync(e =>
            e.ExploitationId == exploitationId && e.Type == ReproductionType.Gestation && (e.EndDate == null || e.EndDate >= today), cancellationToken);
        ViewBag.StocksBas = await db.StockArticles.CountAsync(s => s.ExploitationId == exploitationId && s.IsActive && s.QuantityOnHand <= s.ReorderLevel, cancellationToken);
        ViewBag.InterventionsOuvertes = await db.Interventions.CountAsync(i =>
            i.ExploitationId == exploitationId && i.Statut != InterventionStatut.Cloturee && i.Statut != InterventionStatut.Annulee, cancellationToken);
        ViewBag.Accessible = await exploitationContext.GetAccessibleExploitationsAsync(cancellationToken);
        ViewBag.CurrentId = exploitationId;
        return View();
    }
}

[Area("Elevage")]
public class ExploitationsController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : ElevageControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var list = await exploitationContext.GetAccessibleExploitationsAsync(cancellationToken);
        var ids = list.Select(x => x.Id).ToList();
        var items = await db.Exploitations.AsNoTracking()
            .Where(e => ids.Contains(e.Id))
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
        return View(items);
    }

    public IActionResult Create() => View(new Exploitation());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Exploitation model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        db.Exploitations.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        if (!string.IsNullOrEmpty(CurrentUserId))
        {
            db.ExploitationUsers.Add(new ExploitationUser
            {
                ExploitationId = model.Id,
                UserId = CurrentUserId,
                Role = ExploitationUserRole.Proprietaire
            });
            await db.SaveChangesAsync(cancellationToken);
        }
        await exploitationContext.SetCurrentExploitationIdAsync(model.Id, cancellationToken);
        TempData["Success"] = "Exploitation créée.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        await exploitationContext.EnsureAccessAsync(id, cancellationToken);
        var item = await db.Exploitations.FindAsync([id], cancellationToken);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Exploitation model, CancellationToken cancellationToken)
    {
        if (id != model.Id) return BadRequest();
        await exploitationContext.EnsureAccessAsync(id, cancellationToken);
        if (!ModelState.IsValid) return View(model);
        db.Update(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Exploitation mise à jour.";
        return RedirectToAction(nameof(Index));
    }
}

[Area("Elevage")]
public class TroupeauxController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : ElevageControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var items = await db.Troupeaux.AsNoTracking()
            .Include(t => t.Lots)
            .Where(t => t.ExploitationId == exploitationId)
            .OrderBy(t => t.Code)
            .ToListAsync(cancellationToken);
        ViewBag.Enclos = await db.Enclos.AsNoTracking()
            .Where(e => e.ExploitationId == exploitationId && e.IsActive)
            .OrderBy(e => e.Code)
            .ToListAsync(cancellationToken);
        return View(items);
    }

    public IActionResult Create() => View(new Troupeau());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Troupeau model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (!ModelState.IsValid) return View(model);
        db.Troupeaux.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Troupeau créé.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.Troupeaux.FirstOrDefaultAsync(t => t.Id == id && t.ExploitationId == exploitationId, cancellationToken);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Troupeau model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        if (id != model.Id || model.ExploitationId != exploitationId) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        db.Update(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Troupeau mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> CreateLot(int troupeauId, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var troupeau = await db.Troupeaux.FirstOrDefaultAsync(t => t.Id == troupeauId && t.ExploitationId == exploitationId, cancellationToken);
        if (troupeau is null) return NotFound();
        ViewBag.TroupeauName = troupeau.Name;
        return View(new Lot { TroupeauId = troupeauId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLot(Lot model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var troupeau = await db.Troupeaux.FirstOrDefaultAsync(t => t.Id == model.TroupeauId && t.ExploitationId == exploitationId, cancellationToken);
        if (troupeau is null) return NotFound();
        if (!ModelState.IsValid)
        {
            ViewBag.TroupeauName = troupeau.Name;
            return View(model);
        }
        db.Lots.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Lot créé.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult CreateEnclos() => View(new Enclos());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEnclos(Enclos model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (!ModelState.IsValid) return View(model);
        db.Enclos.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Enclos créé.";
        return RedirectToAction(nameof(Index));
    }
}

[Area("Elevage")]
public class AnimauxController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : ElevageControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(AnimalStatut? statut, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var query = db.Animaux.AsNoTracking()
            .Include(a => a.Troupeau)
            .Include(a => a.Lot)
            .Where(a => a.ExploitationId == exploitationId);
        if (statut is not null) query = query.Where(a => a.Statut == statut);
        ViewBag.Statut = statut;
        return View(await query.OrderBy(a => a.BoucleNumber).ToListAsync(cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.Animaux.AsNoTracking()
            .Include(a => a.Troupeau)
            .Include(a => a.Lot)
            .Include(a => a.Enclos)
            .Include(a => a.Evenements.OrderByDescending(e => e.EventDate))
            .FirstOrDefaultAsync(a => a.Id == id && a.ExploitationId == exploitationId, cancellationToken);
        return item is null ? NotFound() : View(item);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadLookupsAsync(cancellationToken);
        return View(new Animal());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Animal model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(cancellationToken);
            return View(model);
        }
        db.Animaux.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Animal créé.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.Animaux.FirstOrDefaultAsync(a => a.Id == id && a.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();
        await LoadLookupsAsync(cancellationToken);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Animal model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        if (id != model.Id || model.ExploitationId != exploitationId) return BadRequest();
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(cancellationToken);
            return View(model);
        }
        db.Update(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Animal mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> AddEvent(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var animal = await db.Animaux.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.ExploitationId == exploitationId, cancellationToken);
        if (animal is null) return NotFound();
        ViewBag.AnimalLabel = animal.BoucleNumber;
        return View(new AnimalEvenement { AnimalId = id, EventDate = DateTime.UtcNow.Date });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEvent(int id, AnimalEvenement model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var animal = await db.Animaux.FirstOrDefaultAsync(a => a.Id == id && a.ExploitationId == exploitationId, cancellationToken);
        if (animal is null) return NotFound();
        model.AnimalId = id;
        if (!ModelState.IsValid)
        {
            ViewBag.AnimalLabel = animal.BoucleNumber;
            return View(model);
        }
        db.AnimalEvenements.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Événement ajouté.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Export(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var items = await db.Animaux.AsNoTracking()
            .Where(a => a.ExploitationId == exploitationId)
            .OrderBy(a => a.BoucleNumber)
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Boucle;RFID;Espèce;Race;Sexe;Naissance;Statut");
        foreach (var a in items)
            sb.AppendLine($"{a.BoucleNumber};{a.RfidTag};{a.Species};{a.Race};{a.Sex};{a.BirthDate:yyyy-MM-dd};{a.Statut}");

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "inventaire-animaux.csv");
    }

    private async Task LoadLookupsAsync(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        ViewBag.Troupeaux = new SelectList(
            await db.Troupeaux.AsNoTracking().Where(t => t.ExploitationId == exploitationId && t.IsActive).OrderBy(t => t.Code).ToListAsync(cancellationToken),
            "Id", "Name");
        ViewBag.Lots = new SelectList(
            await db.Lots.AsNoTracking().Include(l => l.Troupeau).Where(l => l.Troupeau.ExploitationId == exploitationId && l.IsActive).OrderBy(l => l.Code).ToListAsync(cancellationToken),
            "Id", "Name");
        ViewBag.Enclos = new SelectList(
            await db.Enclos.AsNoTracking().Where(e => e.ExploitationId == exploitationId && e.IsActive).OrderBy(e => e.Code).ToListAsync(cancellationToken),
            "Id", "Name");
        ViewBag.Animaux = new SelectList(
            await db.Animaux.AsNoTracking().Where(a => a.ExploitationId == exploitationId).OrderBy(a => a.BoucleNumber).ToListAsync(cancellationToken),
            "Id", "BoucleNumber");
    }
}

[Area("Elevage")]
public class SanteController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : ElevageControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        ViewBag.Protocoles = await db.ProtocolesSanitaires.AsNoTracking()
            .Where(p => p.ExploitationId == exploitationId)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
        ViewBag.Traitements = await db.Traitements.AsNoTracking()
            .Include(t => t.Animal)
            .Include(t => t.Lot)
            .Include(t => t.ProtocoleSanitaire)
            .Where(t => t.ExploitationId == exploitationId)
            .OrderByDescending(t => t.AdministeredAt)
            .ToListAsync(cancellationToken);
        return View();
    }

    public IActionResult CreateProtocole() => View(new ProtocoleSanitaire());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateProtocole(ProtocoleSanitaire model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (!ModelState.IsValid) return View(model);
        db.ProtocolesSanitaires.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Protocole créé.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> EditProtocole(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.ProtocolesSanitaires.FirstOrDefaultAsync(p => p.Id == id && p.ExploitationId == exploitationId, cancellationToken);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProtocole(int id, ProtocoleSanitaire model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        if (id != model.Id || model.ExploitationId != exploitationId) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        db.Update(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Protocole mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> CreateTraitement(CancellationToken cancellationToken)
    {
        await LoadLookupsAsync(cancellationToken);
        return View(new Traitement { AdministeredAt = DateTime.UtcNow.Date });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTraitement(Traitement model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync(cancellationToken);
            return View(model);
        }
        db.Traitements.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Traitement enregistré.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadLookupsAsync(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        ViewBag.Animaux = new SelectList(
            await db.Animaux.AsNoTracking().Where(a => a.ExploitationId == exploitationId).OrderBy(a => a.BoucleNumber).ToListAsync(cancellationToken),
            "Id", "BoucleNumber");
        ViewBag.Lots = new SelectList(
            await db.Lots.AsNoTracking().Include(l => l.Troupeau).Where(l => l.Troupeau.ExploitationId == exploitationId).OrderBy(l => l.Code).ToListAsync(cancellationToken),
            "Id", "Name");
        ViewBag.Protocoles = new SelectList(
            await db.ProtocolesSanitaires.AsNoTracking().Where(p => p.ExploitationId == exploitationId && p.IsActive).OrderBy(p => p.Name).ToListAsync(cancellationToken),
            "Id", "Name");
    }
}

[Area("Elevage")]
public class ReproductionController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : ElevageControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var items = await db.EvenementsReproduction.AsNoTracking()
            .Include(e => e.Animal)
            .Where(e => e.ExploitationId == exploitationId)
            .OrderByDescending(e => e.StartDate)
            .ToListAsync(cancellationToken);
        return View(items);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadAnimauxAsync(cancellationToken);
        return View(new EvenementReproduction { StartDate = DateTime.UtcNow.Date });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EvenementReproduction model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (!ModelState.IsValid)
        {
            await LoadAnimauxAsync(cancellationToken);
            return View(model);
        }
        db.EvenementsReproduction.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Événement enregistré.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.EvenementsReproduction.FirstOrDefaultAsync(e => e.Id == id && e.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();
        await LoadAnimauxAsync(cancellationToken);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EvenementReproduction model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        if (id != model.Id || model.ExploitationId != exploitationId) return BadRequest();
        if (!ModelState.IsValid)
        {
            await LoadAnimauxAsync(cancellationToken);
            return View(model);
        }
        db.Update(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Événement mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadAnimauxAsync(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        ViewBag.Animaux = new SelectList(
            await db.Animaux.AsNoTracking().Where(a => a.ExploitationId == exploitationId && a.Sex == "F").OrderBy(a => a.BoucleNumber).ToListAsync(cancellationToken),
            "Id", "BoucleNumber");
    }
}

[Area("Elevage")]
public class ActifsController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : ElevageControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(ActifCategorie? categorie, ActifStatut? statut, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var query = db.ActifsMateriel.AsNoTracking()
            .Where(a => a.ExploitationId == exploitationId && a.IsActive);
        if (categorie is not null) query = query.Where(a => a.Categorie == categorie);
        if (statut is not null) query = query.Where(a => a.Statut == statut);
        ViewBag.Categorie = categorie;
        ViewBag.Statut = statut;
        return View(await query.OrderBy(a => a.InternalCode).ToListAsync(cancellationToken));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.ActifsMateriel.AsNoTracking()
            .Include(a => a.Enclos)
            .FirstOrDefaultAsync(a => a.Id == id && a.ExploitationId == exploitationId, cancellationToken);
        return item is null ? NotFound() : View(item);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadEnclosAsync(cancellationToken);
        return View(new ActifMateriel());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ActifMateriel model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (!ModelState.IsValid)
        {
            await LoadEnclosAsync(cancellationToken);
            return View(model);
        }
        db.ActifsMateriel.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Actif créé.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.ActifsMateriel.FirstOrDefaultAsync(a => a.Id == id && a.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();
        await LoadEnclosAsync(cancellationToken);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ActifMateriel model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        if (id != model.Id || model.ExploitationId != exploitationId) return BadRequest();
        if (!ModelState.IsValid)
        {
            await LoadEnclosAsync(cancellationToken);
            return View(model);
        }
        db.Update(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Actif mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Export(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var items = await db.ActifsMateriel.AsNoTracking()
            .Where(a => a.ExploitationId == exploitationId && a.IsActive)
            .OrderBy(a => a.InternalCode)
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("Code;Nom;Categorie;Statut;Marque;Modele;Valeur");
        foreach (var a in items)
            sb.AppendLine($"{a.InternalCode};{a.Name};{a.Categorie};{a.Statut};{a.Brand};{a.Model};{a.AcquisitionValue}");

        return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "inventaire-actifs-materiel.csv");
    }

    private async Task LoadEnclosAsync(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        ViewBag.Enclos = new SelectList(
            await db.Enclos.AsNoTracking().Where(e => e.ExploitationId == exploitationId && e.IsActive).OrderBy(e => e.Code).ToListAsync(cancellationToken),
            "Id", "Name");
    }
}

[Area("Elevage")]
public class StocksController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : ElevageControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var items = await db.StockArticles.AsNoTracking()
            .Where(s => s.ExploitationId == exploitationId)
            .OrderBy(s => s.Sku)
            .ToListAsync(cancellationToken);
        return View(items);
    }

    public IActionResult Create() => View(new StockArticle());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StockArticle model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (!ModelState.IsValid) return View(model);
        db.StockArticles.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Article créé.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.StockArticles.FirstOrDefaultAsync(s => s.Id == id && s.ExploitationId == exploitationId, cancellationToken);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, StockArticle model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        if (id != model.Id || model.ExploitationId != exploitationId) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        db.Update(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Article mis à jour.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Adjust(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.StockArticles.FirstOrDefaultAsync(s => s.Id == id && s.ExploitationId == exploitationId, cancellationToken);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Adjust(int id, StockMouvementType type, decimal quantity, string? notes, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.StockArticles.FirstOrDefaultAsync(s => s.Id == id && s.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();

        var delta = type switch
        {
            StockMouvementType.Entree => Math.Abs(quantity),
            StockMouvementType.Sortie => -Math.Abs(quantity),
            _ => quantity
        };
        item.QuantityOnHand += delta;
        db.StockMouvements.Add(new StockMouvement
        {
            StockArticleId = item.Id,
            Type = type,
            Quantity = quantity,
            Notes = notes,
            CreatedByUserId = CurrentUserId
        });
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Stock ajusté.";
        return RedirectToAction(nameof(Index));
    }
}

[Area("Elevage")]
public class MaintenanceController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : ElevageControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var items = await db.Interventions.AsNoTracking()
            .Include(i => i.Actif)
            .Where(i => i.ExploitationId == exploitationId)
            .OrderByDescending(i => i.PlannedDate)
            .ToListAsync(cancellationToken);
        return View(items);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadActifsAsync(cancellationToken);
        return View(new InterventionMaintenance { PlannedDate = DateTime.UtcNow.Date });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InterventionMaintenance model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (!ModelState.IsValid)
        {
            await LoadActifsAsync(cancellationToken);
            return View(model);
        }
        db.Interventions.Add(model);
        if (model.ActifMaterielId is int actifId && model.Type == InterventionType.Correctif)
        {
            var actif = await db.ActifsMateriel.FirstOrDefaultAsync(a => a.Id == actifId && a.ExploitationId == model.ExploitationId, cancellationToken);
            if (actif is not null) actif.Statut = ActifStatut.EnMaintenance;
        }
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Intervention créée.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.Interventions.FirstOrDefaultAsync(i => i.Id == id && i.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();
        await LoadActifsAsync(cancellationToken);
        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InterventionMaintenance model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        if (id != model.Id || model.ExploitationId != exploitationId) return BadRequest();
        if (!ModelState.IsValid)
        {
            await LoadActifsAsync(cancellationToken);
            return View(model);
        }
        db.Update(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Intervention mise à jour.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id, string? report, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.Interventions.FirstOrDefaultAsync(i => i.Id == id && i.ExploitationId == exploitationId, cancellationToken);
        if (item is null) return NotFound();
        item.Statut = InterventionStatut.Cloturee;
        item.CompletedDate = DateTime.UtcNow;
        item.Report = report;
        if (item.ActifMaterielId is int actifId)
        {
            var actif = await db.ActifsMateriel.FirstOrDefaultAsync(a => a.Id == actifId && a.ExploitationId == exploitationId, cancellationToken);
            if (actif is not null) actif.Statut = ActifStatut.EnService;
        }
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Intervention clôturée.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadActifsAsync(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        ViewBag.Actifs = new SelectList(
            await db.ActifsMateriel.AsNoTracking()
                .Where(a => a.ExploitationId == exploitationId && a.IsActive)
                .OrderBy(a => a.InternalCode)
                .ToListAsync(cancellationToken),
            "Id", "Name");
    }
}

[Area("Elevage")]
public class FournisseursController(
    IExploitationContextService exploitationContext,
    ApplicationDbContext db) : ElevageControllerBase(exploitationContext)
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        return View(await db.Fournisseurs.AsNoTracking()
            .Where(f => f.ExploitationId == exploitationId)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken));
    }

    public IActionResult Create() => View(new Fournisseur());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Fournisseur model, CancellationToken cancellationToken)
    {
        model.ExploitationId = await GetExploitationIdAsync(cancellationToken);
        if (!ModelState.IsValid) return View(model);
        db.Fournisseurs.Add(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Fournisseur créé.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        var item = await db.Fournisseurs.FirstOrDefaultAsync(f => f.Id == id && f.ExploitationId == exploitationId, cancellationToken);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Fournisseur model, CancellationToken cancellationToken)
    {
        var exploitationId = await GetExploitationIdAsync(cancellationToken);
        if (id != model.Id || model.ExploitationId != exploitationId) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        db.Update(model);
        await db.SaveChangesAsync(cancellationToken);
        TempData["Success"] = "Fournisseur mis à jour.";
        return RedirectToAction(nameof(Index));
    }
}
