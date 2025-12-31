using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using WebApplication1.ViewModels; // AJOUTER CET USING !

namespace WebApplication1.Controllers
{
    public class PanierController : Controller
    {
        private const string PanierCookieName = "Panier";

        // GET: /Panier
        public IActionResult Index()
        {
            var panier = GetPanierFromCookie();
            return View(panier);
        }

        // POST: /Panier/Ajouter
        [HttpPost]
        public IActionResult Ajouter(int produitId, string nom, decimal prix, int quantite = 1)
        {
            var panier = GetPanierFromCookie();

            // Vérifier si le produit existe déjà
            var itemExist = panier.FirstOrDefault(p => p.ProduitId == produitId);

            if (itemExist != null)
            {
                itemExist.Quantite += quantite;
            }
            else
            {
                panier.Add(new PanierItemViewModel // CHANGER ICI : PanierItem → PanierItemViewModel
                {
                    ProduitId = produitId,
                    Nom = nom,
                    Prix = prix,
                    Quantite = quantite
                });
            }

            SavePanierToCookie(panier);
            TempData["SuccessMessage"] = $"{nom} ajouté au panier !";

            return RedirectToAction("Index", "Home");
        }

        // POST: /Panier/Modifier
        [HttpPost]
        public IActionResult Modifier(int produitId, int nouvelleQuantite)
        {
            var panier = GetPanierFromCookie();
            var item = panier.FirstOrDefault(p => p.ProduitId == produitId);

            if (item != null)
            {
                if (nouvelleQuantite <= 0)
                {
                    panier.Remove(item);
                    TempData["InfoMessage"] = $"{item.Nom} retiré du panier";
                }
                else
                {
                    item.Quantite = nouvelleQuantite;
                    TempData["SuccessMessage"] = $"Quantité de {item.Nom} mise à jour";
                }

                SavePanierToCookie(panier);
            }

            return RedirectToAction("Index");
        }

        // POST: /Panier/Supprimer
        [HttpPost]
        public IActionResult Supprimer(int produitId)
        {
            var panier = GetPanierFromCookie();
            var item = panier.FirstOrDefault(p => p.ProduitId == produitId);

            if (item != null)
            {
                panier.Remove(item);
                SavePanierToCookie(panier);
                TempData["WarningMessage"] = $"{item.Nom} supprimé du panier";
            }

            return RedirectToAction("Index");
        }

        // POST: /Panier/Vider
        [HttpPost]
        public IActionResult Vider()
        {
            Response.Cookies.Delete(PanierCookieName);
            TempData["SuccessMessage"] = "Panier vidé avec succès";

            return RedirectToAction("Index");
        }

        // POST: /Panier/Commander
        [HttpPost]
        public IActionResult Commander()
        {
            var panier = GetPanierFromCookie();

            if (!panier.Any())
            {
                TempData["ErrorMessage"] = "Votre panier est vide !";
                return RedirectToAction("Index");
            }

            // Calculer le total
            var total = panier.Sum(item => item.Total); // CHANGER ICI : item.Prix * item.Quantite → item.Total
            var nbArticles = panier.Sum(item => item.Quantite);

            // Vider le panier après commande
            Response.Cookies.Delete(PanierCookieName);

            TempData["SuccessMessage"] =
                $"Commande passée avec succès ! {nbArticles} article(s) pour un total de {total:C}";

            return RedirectToAction("Index");
        }

        private List<PanierItemViewModel> GetPanierFromCookie() // CHANGER LE TYPE DE RETOUR
        {
            var panierJson = Request.Cookies[PanierCookieName];

            if (string.IsNullOrEmpty(panierJson))
            {
                return new List<PanierItemViewModel>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<PanierItemViewModel>>(panierJson); // CHANGER ICI
            }
            catch
            {
                return new List<PanierItemViewModel>();
            }
        }

        private void SavePanierToCookie(List<PanierItemViewModel> panier) // CHANGER LE TYPE DE PARAMÈTRE
        {
            var panierJson = JsonSerializer.Serialize(panier);

            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7), // Conserve 7 jours
                HttpOnly = false, // FALSE pour pouvoir lire avec JavaScript
                IsEssential = true,
                Secure = false, // Mettez true en production avec HTTPS
                SameSite = SameSiteMode.Lax
            };

            Response.Cookies.Append(PanierCookieName, panierJson, cookieOptions);
        }
    }

    // SUPPRIMER OU COMMENTER CETTE CLASSE INTERNE !
    // Elle entre en conflit avec PanierItemViewModel dans ViewModels/
    /*
    public class PanierItem
    {
        public int ProduitId { get; set; }
        public string Nom { get; set; } = "";
        public decimal Prix { get; set; }
        public int Quantite { get; set; } = 1;

        public decimal Total => Prix * Quantite;
    }
    */
}