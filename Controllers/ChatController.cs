using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;

namespace WebApplication1.Controllers // Assure-toi que le namespace est correct
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly GeminiService _geminiService;
        private readonly ApplicationDbContext _context;

        public ChatController(GeminiService geminiService, ApplicationDbContext context)
        {
            _geminiService = geminiService;
            _context = context;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
        {
            // Vérification de sécurité si la requête ou le message est nul
            if (request == null || string.IsNullOrEmpty(request.Message))
                return BadRequest("Le message ne peut pas être vide.");

            // On récupère les noms et prix des produits depuis la DB
            var listeProduits = await _context.Produits
                .Select(p => $"{p.Nom} ({p.Prix}€)")
                .ToListAsync();

            string contexte = listeProduits.Any()
                ? string.Join(", ", listeProduits)
                : "Aucun produit en stock actuellement.";

            // Appel au service Gemini
            var reply = await _geminiService.GetResponseAsync(request.Message, contexte);

            return Ok(new { reply });
        }
    }

    // --- CETTE CLASSE ÉTAIT MANQUANTE DANS TON CODE ---
    // Elle permet de recevoir le JSON { "message": "..." } envoyé par le JavaScript
    public class ChatRequest
    {
        public string Message { get; set; }
    }
}