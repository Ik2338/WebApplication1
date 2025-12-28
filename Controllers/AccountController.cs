using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApplication1.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = null)
        {
            // 1. Validation des credentials via la méthode IsValidUser
            if (IsValidUser(username, password, out var role))
            {
                // 2. Création des claims (identité de l'utilisateur)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, role)
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                // 3. Connexion officielle (création du cookie)
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // --- LOGIQUE DE REDIRECTION CORRIGÉE ---

                // A. Si l'utilisateur tentait d'accéder à une page précise (ex: /admin/create)
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // B. Redirection forcée selon le Rôle si returnUrl est vide
                if (role == "Admin" || role == "Manager")
                {
                    // Redirige directement vers l'Index du AdminController
                    return RedirectToAction("Index", "Admin");
                }

                // C. Par défaut pour les utilisateurs simples
                return RedirectToAction("Index", "Home");
            }

            // Si échec de connexion
            ModelState.AddModelError(string.Empty, "Nom d'utilisateur ou mot de passe incorrect");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Méthode de validation des utilisateurs (Simulée)
        private bool IsValidUser(string username, string password, out string role)
        {
            var users = new Dictionary<string, (string password, string role)>
            {
                { "admin", ("admin123", "Admin") },
                { "manager", ("manager123", "Manager") },
                { "user", ("user123", "User") }
            };

            if (username != null && users.TryGetValue(username.ToLower(), out var userInfo) &&
                userInfo.password == password)
            {
                role = userInfo.role;
                return true;
            }

            role = null;
            return false;
        }
    }
}