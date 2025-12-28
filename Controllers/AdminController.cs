using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Authorize(Roles = "Admin,Manager")] // Seulement pour Admin et Manager
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin
        public async Task<IActionResult> Index()
        {
            var produits = await _context.Produits
                .OrderByDescending(p => p.DateCreation)
                .ToListAsync();
            return View(produits);
        }

        // GET: Admin/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produit = await _context.Produits
                .FirstOrDefaultAsync(m => m.Id == id);

            if (produit == null)
            {
                return NotFound();
            }

            return View(produit);
        }

        // GET: Admin/Create
        [Authorize(Roles = "Admin")] // Seulement Admin peut créer
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Nom,Description,Prix,Categorie,Quantite,ImageUrl")] Produit produit)
        {
            if (ModelState.IsValid)
            {
                produit.DateCreation = DateTime.Now;
                _context.Add(produit);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Produit créé avec succès !";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Veuillez corriger les erreurs du formulaire.";
            return View(produit);
        }

        // GET: Admin/Edit/5
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produit = await _context.Produits.FindAsync(id);
            if (produit == null)
            {
                return NotFound();
            }
            return View(produit);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Description,Prix,Categorie,Quantite,ImageUrl,DateCreation")] Produit produit)
        {
            if (id != produit.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(produit);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Produit modifié avec succès !";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProduitExists(produit.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Veuillez corriger les erreurs du formulaire.";
            return View(produit);
        }

        // GET: Admin/Delete/5
        [Authorize(Roles = "Admin")] // Seulement Admin peut supprimer
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var produit = await _context.Produits
                .FirstOrDefaultAsync(m => m.Id == id);

            if (produit == null)
            {
                return NotFound();
            }

            return View(produit);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var produit = await _context.Produits.FindAsync(id);
            if (produit != null)
            {
                _context.Produits.Remove(produit);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Produit supprimé avec succès !";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProduitExists(int id)
        {
            return _context.Produits.Any(e => e.Id == id);
        }
    }
}