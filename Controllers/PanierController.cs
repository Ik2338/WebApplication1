using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace WebApplication1.Controllers
{
    public class PanierController : Controller
    {
        // GET: /Panier
        public IActionResult Index()
        {
            return View();
        }
    }
}