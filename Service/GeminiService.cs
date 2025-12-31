using System.Text;
using System.Text.Json;

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    // On injecte IConfiguration pour lire le appsettings.json
    public GeminiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"];
    }

    public async Task<string> GetResponseAsync(string userPrompt, string contextProduits)
    {
        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={_apiKey}";

            var requestBody = new
            {
                contents = new[] {
                    new {
                        parts = new[] {
                            new { text = $"Tu es l'assistant de 'MaBoutique'. Voici nos produits : {contextProduits}. Réponds poliment et brièvement." }
                        }
                    },
                    new {
                        parts = new[] {
                            new { text = userPrompt }
                        }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
                return "Désolé, je rencontre une petite erreur technique.";

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);

            // Extraction sécurisée de la réponse
            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "Je ne sais pas quoi répondre à cela.";
        }
        catch (Exception ex)
        {
            return "Une erreur est survenue lors de la communication avec l'IA.";
        }
    }
}