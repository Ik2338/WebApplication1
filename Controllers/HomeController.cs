using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Client Side - Public
        public async Task<IActionResult> Index()
        {
            // Récupère seulement les produits disponibles
            var produits = await _context.Produits
                .Where(p => p.Quantite > 0)
                .OrderByDescending(p => p.DateCreation)
                .ToListAsync();

            return View(produits);
        }
        public IActionResult Details(int id)
{
    // On cherche le produit dans la table 'Produits' via son ID
    var produit = _context.Produits.FirstOrDefault(p => p.Id == id);

    if (produit == null)
    {
        return NotFound(); // Si le produit n'existe pas
    }

    return View(produit); // On envoie l'objet produit à la vue Details.cshtml
}
    }
}


