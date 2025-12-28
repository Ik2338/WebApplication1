using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

// Liste des catégories disponibles
var categoriesDisponibles = new[]
{
    "Électronique",
    "Audio",
    "Wearable",
    "Informatique",
    "Photo & Vidéo",
    "Maison Connectée",
    "Jeux Vidéo",
    "Sport & Loisirs",
    "Bureautique",
    "Impression",
    "Réseau",
    "Accessoires",
    "Bien-être",
    "Cuisine Connectée",
    "Jardinage",
    "Automobile"
};

// Afficher les catégories disponibles au démarrage
Console.WriteLine("");
Console.WriteLine("📊  CATÉGORIES DISPONIBLES DANS L'APPLICATION");
Console.WriteLine("=============================================");
Console.WriteLine($"   Nombre total : {categoriesDisponibles.Length} catégories");
Console.WriteLine("");
for (int i = 0; i < categoriesDisponibles.Length; i++)
{
    Console.WriteLine($"   {i + 1:00}. {categoriesDisponibles[i]}");
}
Console.WriteLine("");

var builder = WebApplication.CreateBuilder(args);

// 1. CONFIG DB
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. AUTHENTIFICATION AVEC COOKIES (SANS IDENTITY)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? Microsoft.AspNetCore.Http.CookieSecurePolicy.None
            : Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
        options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    });

// 3. AUTORISATION
builder.Services.AddAuthorization();

// 4. AJOUTER HTTP CONTEXT ACCESSOR (important pour le layout)
builder.Services.AddHttpContextAccessor();

// 5. SESSION (optionnel, pour stocker des données temporaires)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 6. MVC
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 7. CONFIGURATION POUR LES VUES
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// 8. ENREGISTRER LES CATÉGORIES COMME SERVICE SINGLETON
builder.Services.AddSingleton<string[]>(categoriesDisponibles);

var app = builder.Build();

// CONFIG HTTP PIPELINE
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// MIDDLEWARE DANS LE BON ORDRE
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// IMPORTANT : Authentication AVANT Authorization
app.UseAuthentication();
app.UseAuthorization();

// Session (si activé)
app.UseSession();

// Middleware pour rendre les catégories disponibles dans le contexte
app.Use(async (context, next) =>
{
    // Ajouter les catégories au HttpContext pour utilisation dans les vues
    context.Items["CategoriesDisponibles"] = categoriesDisponibles;
    await next();
});

// ROUTES
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// 🔥 **INITIALISATION DE LA BASE DE DONNÉES** 🔥
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();

        // 1. Créer la base si elle n'existe pas
        Console.WriteLine("🔄  Création/ Vérification de la base de données...");

        if (db.Database.CanConnect())
        {
            Console.WriteLine("✅  Base de données déjà connectée");

            // Vérifier si la table Produits existe
            var tableExists = db.Database.ExecuteSqlRaw(@"
                SELECT CASE WHEN EXISTS (
                    SELECT * FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'Produits'
                ) THEN 1 ELSE 0 END") == 1;

            if (!tableExists)
            {
                Console.WriteLine("📊  Création des tables...");
                db.Database.EnsureCreated();
            }
        }
        else
        {
            Console.WriteLine("📊  Création de la base de données...");
            db.Database.EnsureCreated();
        }

        // 2. Vérifier et ajouter des produits de test
        Console.WriteLine("📦  Vérification des produits...");
        if (!db.Produits.Any())
        {
            Console.WriteLine("➕  Ajout de produits de test...");

            var produitsTest = new List<Produit>
            {
                // ========== ÉLECTRONIQUE ==========
                new Produit
                {
                    Nom = "iPhone 15 Pro Max",
                    Description = "Smartphone Apple avec écran Super Retina XDR, chip A17 Pro, 48MP camera principale",
                    Prix = 1459.99m,
                    Quantite = 18,
                    Categorie = "Électronique",
                    ImageUrl = "https://images.unsplash.com/photo-1695048133142-1a20484d2569?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now
                },
                new Produit
                {
                    Nom = "Samsung Galaxy S24 Ultra",
                    Description = "Écran Dynamic AMOLED 2X 6.8\", S-Pen intégré, camera 200MP",
                    Prix = 1399.99m,
                    Quantite = 22,
                    Categorie = "Électronique",
                    ImageUrl = "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-1)
                },
                new Produit
                {
                    Nom = "iPad Air M2",
                    Description = "Tablette Apple 10.9\", chip M2, compatible Apple Pencil 2",
                    Prix = 899.99m,
                    Quantite = 15,
                    Categorie = "Électronique",
                    ImageUrl = "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-2)
                },
                new Produit
                {
                    Nom = "MacBook Pro 16\" M3 Pro",
                    Description = "Ordinateur portable Apple 16\", 36GB RAM, 1TB SSD, écran Liquid Retina XDR",
                    Prix = 3299.99m,
                    Quantite = 8,
                    Categorie = "Électronique",
                    ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-3)
                },

                // ========== AUDIO ==========
                new Produit
                {
                    Nom = "Sony WH-1000XM5",
                    Description = "Casque sans fil avec réduction de bruit optimale, 30h autonomie",
                    Prix = 399.99m,
                    Quantite = 25,
                    Categorie = "Audio",
                    ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-4)
                },
                new Produit
                {
                    Nom = "JBL Charge 5",
                    Description = "Enceinte portable Bluetooth waterproof, bass boost, 20h autonomie",
                    Prix = 179.99m,
                    Quantite = 35,
                    Categorie = "Audio",
                    ImageUrl = "https://images.unsplash.com/photo-1546435770-a3e426bf472b?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-5)
                },
                new Produit
                {
                    Nom = "AirPods Pro 2",
                    Description = "Écouteurs sans fil Apple avec annulation active du bruit, étui MagSafe",
                    Prix = 279.99m,
                    Quantite = 40,
                    Categorie = "Audio",
                    ImageUrl = "https://images.unsplash.com/photo-1590658165737-15a047b8b5e6?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-6)
                },

                // ========== WEARABLE ==========
                new Produit
                {
                    Nom = "Apple Watch Series 9",
                    Description = "Montre connectée GPS + Cellular, écran Retina, suivi santé avancé",
                    Prix = 529.99m,
                    Quantite = 30,
                    Categorie = "Wearable",
                    ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-7)
                },
                new Produit
                {
                    Nom = "Fitbit Charge 6",
                    Description = "Tracker d'activité avec GPS intégré, monitoring cardiaque 24/7",
                    Prix = 179.99m,
                    Quantite = 45,
                    Categorie = "Wearable",
                    ImageUrl = "https://images.unsplash.com/photo-1576243345690-4e4b79b63288?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-8)
                },

                // ========== INFORMATIQUE ==========
                new Produit
                {
                    Nom = "ASUS ROG Strix G18",
                    Description = "PC Gaming Intel i9-13980HX, RTX 4070 8GB, 32GB RAM, 1TB SSD",
                    Prix = 2499.99m,
                    Quantite = 12,
                    Categorie = "Informatique",
                    ImageUrl = "https://images.unsplash.com/photo-1603302576837-37561b2e2302?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-9)
                },
                new Produit
                {
                    Nom = "Dell XPS 15",
                    Description = "Ultrabook créatif 15.6\" OLED, Intel i7, 32GB RAM, RTX 4060",
                    Prix = 2199.99m,
                    Quantite = 10,
                    Categorie = "Informatique",
                    ImageUrl = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-10)
                },
                new Produit
                {
                    Nom = "Logitech MX Master 3S",
                    Description = "Souris ergonomique sans fil, défilement MagSpeed, 70 jours autonomie",
                    Prix = 119.99m,
                    Quantite = 60,
                    Categorie = "Informatique",
                    ImageUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-11)
                },

                // ========== PHOTO & VIDÉO ==========
                new Produit
                {
                    Nom = "Canon EOS R6 Mark II",
                    Description = "Appareil photo hybride 24MP, vidéo 4K 60p, stabilisation 8 stops",
                    Prix = 2899.99m,
                    Quantite = 7,
                    Categorie = "Photo & Vidéo",
                    ImageUrl = "https://images.unsplash.com/photo-1502920917128-1aa500764cbd?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-12)
                },
                new Produit
                {
                    Nom = "DJI Mini 4 Pro",
                    Description = "Drone compact 4K, poids <249g, transmission O4, suivi avancé",
                    Prix = 959.99m,
                    Quantite = 14,
                    Categorie = "Photo & Vidéo",
                    ImageUrl = "https://images.unsplash.com/photo-1473968512647-3e447244af8f?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-13)
                },
                new Produit
                {
                    Nom = "GoPro Hero 12 Black",
                    Description = "Caméra d'action 5.3K, HyperSmooth 6.0, HDR, waterproof 10m",
                    Prix = 449.99m,
                    Quantite = 28,
                    Categorie = "Photo & Vidéo",
                    ImageUrl = "https://images.unsplash.com/photo-1551816230-ef5deaed4a26?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-14)
                },

                // ========== MAISON CONNECTÉE ==========
                new Produit
                {
                    Nom = "Google Nest Hub (2nd Gen)",
                    Description = "Écran intelligent 7\", assistant Google, contrôle maison connectée",
                    Prix = 129.99m,
                    Quantite = 32,
                    Categorie = "Maison Connectée",
                    ImageUrl = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-15)
                },
                new Produit
                {
                    Nom = "Philips Hue Starter Kit",
                    Description = "Kit d'éclairage connecté 3 ampoules, bridge inclus, compatible Alexa",
                    Prix = 199.99m,
                    Quantite = 20,
                    Categorie = "Maison Connectée",
                    ImageUrl = "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-16)
                },
                new Produit
                {
                    Nom = "Robot Aspirateur Roborock S8 Pro Ultra",
                    Description = "Aspiration 6000Pa, lavage auto, vidage automatique, cartographie LiDAR",
                    Prix = 1499.99m,
                    Quantite = 9,
                    Categorie = "Maison Connectée",
                    ImageUrl = "https://images.unsplash.com/photo-1558618047-3c8c76ca7d13?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-17)
                },

                // ========== JEUX VIDÉO ==========
                new Produit
                {
                    Nom = "PlayStation 5 Slim",
                    Description = "Console de jeu 1TB SSD, 4K 120Hz, Ray Tracing, Blu-ray 4K",
                    Prix = 549.99m,
                    Quantite = 15,
                    Categorie = "Jeux Vidéo",
                    ImageUrl = "https://images.unsplash.com/photo-1606144042614-b2417e99c4e3?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-18)
                },
                new Produit
                {
                    Nom = "Xbox Series X",
                    Description = "Console 1TB SSD, 4K 120FPS, Quick Resume, Game Pass inclus 1 mois",
                    Prix = 549.99m,
                    Quantite = 18,
                    Categorie = "Jeux Vidéo",
                    ImageUrl = "https://images.unsplash.com/photo-1593305841991-05c297ba4575?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-19)
                },
                new Produit
                {
                    Nom = "Nintendo Switch OLED",
                    Description = "Console hybride écran OLED 7\", 64GB interne, Joy-Con améliorés",
                    Prix = 369.99m,
                    Quantite = 22,
                    Categorie = "Jeux Vidéo",
                    ImageUrl = "https://images.unsplash.com/photo-1578303512597-81e6cc155b3e?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-20)
                },

                // ========== SPORT & LOISIRS ==========
                new Produit
                {
                    Nom = "Peloton Bike+",
                    Description = "Vélo de sport connecté écran pivotant 24\", abonnement inclus 1 an",
                    Prix = 2495.00m,
                    Quantite = 5,
                    Categorie = "Sport & Loisirs",
                    ImageUrl = "https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-21)
                },
                new Produit
                {
                    Nom = "Apple Fitness+",
                    Description = "Abonnement annuel entraînements variés, intégration Apple Watch",
                    Prix = 99.99m,
                    Quantite = 100,
                    Categorie = "Sport & Loisirs",
                    ImageUrl = "https://images.unsplash.com/photo-1571902943202-507ec2618e8f?ixlib=rb-4.0.3&auto=format&fit=crop&w=500&q=80",
                    DateCreation = DateTime.Now.AddDays(-22)
                }
            };

            db.Produits.AddRange(produitsTest);
            db.SaveChanges();
            Console.WriteLine($"✅  {produitsTest.Count} produits ajoutés dans 8 catégories !");
        }
        else
        {
            var count = db.Produits.Count();
            Console.WriteLine($"📊  {count} produits trouvés dans la base");
        }

        // 3. ANALYSE DES CATÉGORIES UTILISÉES
        Console.WriteLine("");
        Console.WriteLine("📊  ANALYSE DES CATÉGORIES DANS LA BASE :");
        Console.WriteLine("==========================================");

        var categoriesUtilisees = db.Produits
            .Select(p => p.Categorie)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

        var categoriesNonUtilisees = categoriesDisponibles
            .Except(categoriesUtilisees)
            .OrderBy(c => c)
            .ToList();

        Console.WriteLine($"   Catégories utilisées : {categoriesUtilisees.Count}/{categoriesDisponibles.Length}");
        Console.WriteLine("");

        foreach (var cat in categoriesUtilisees)
        {
            var countProduits = db.Produits.Count(p => p.Categorie == cat);
            var stockTotal = db.Produits.Where(p => p.Categorie == cat).Sum(p => p.Quantite);
            var prixMoyen = db.Produits.Where(p => p.Categorie == cat).Average(p => p.Prix);

            Console.WriteLine($"   ✅ {cat}");
            Console.WriteLine($"        • Produits : {countProduits}");
            Console.WriteLine($"        • Stock total : {stockTotal} unités");
            Console.WriteLine($"        • Prix moyen : {prixMoyen:C}");
        }

        if (categoriesNonUtilisees.Any())
        {
            Console.WriteLine("");
            Console.WriteLine($"   Catégories non utilisées : {categoriesNonUtilisees.Count}");
            foreach (var cat in categoriesNonUtilisees)
            {
                Console.WriteLine($"   ⚠️  {cat} (aucun produit)");
            }
        }

        // 4. Afficher les informations de connexion
        Console.WriteLine("");
        Console.WriteLine("🎉  INITIALISATION TERMINÉE AVEC SUCCÈS !");
        Console.WriteLine("==========================================");
        Console.WriteLine("");
        Console.WriteLine("📋  COMPTES DE TEST POUR L'AUTHENTIFICATION :");
        Console.WriteLine("   👑 ADMIN :");
        Console.WriteLine("      • Nom d'utilisateur : admin");
        Console.WriteLine("      • Mot de passe : admin123");
        Console.WriteLine("      • Rôle : Admin");
        Console.WriteLine("");
        Console.WriteLine("   👔 MANAGER :");
        Console.WriteLine("      • Nom d'utilisateur : manager");
        Console.WriteLine("      • Mot de passe : manager123");
        Console.WriteLine("      • Rôle : Manager");
        Console.WriteLine("");
        Console.WriteLine("   👤 UTILISATEUR :");
        Console.WriteLine("      • Nom d'utilisateur : user");
        Console.WriteLine("      • Mot de passe : user123");
        Console.WriteLine("      • Rôle : User");
        Console.WriteLine("");
        Console.WriteLine("🔗  URLS IMPORTANTES :");
        Console.WriteLine("   • Accueil public : /");
        Console.WriteLine("   • Page login : /Account/Login");
        Console.WriteLine("   • Administration : /Admin");
        Console.WriteLine("   • Accès refusé : /Account/AccessDenied");
        Console.WriteLine("   • Mon profil : /Account/Profile (après connexion)");
        Console.WriteLine("");
        Console.WriteLine("💡  COMMENT TESTER :");
        Console.WriteLine("   1. Accédez à l'accueil (/)");
        Console.WriteLine("   2. Cliquez sur 'Administration' dans le menu");
        Console.WriteLine("   3. Vous serez redirigé vers la page de login");
        Console.WriteLine("   4. Connectez-vous avec admin/admin123");
        Console.WriteLine("   5. Vous serez automatiquement redirigé vers /Admin");
        Console.WriteLine("");

    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌  ERREUR D'INITIALISATION :");
        Console.WriteLine($"   Message : {ex.Message}");

        if (ex.InnerException != null)
        {
            Console.WriteLine($"   Détails : {ex.InnerException.Message}");
        }

        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erreur lors de l'initialisation de la base de données");
    }
}

// 🔍 **PAGE DE DIAGNOSTIC (utile pour le débogage)**
if (app.Environment.IsDevelopment())
{
    app.MapGet("/debug", async (ApplicationDbContext db, HttpContext context) =>
    {
        var html = $@"
            <html>
            <head>
                <title>Debug - MaBoutique</title>
                <style>
                    body {{ font-family: Arial, sans-serif; padding: 20px; background: #f5f5f5; }}
                    .card {{ background: white; padding: 20px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); margin-bottom: 20px; }}
                    .success {{ color: #28a745; }}
                    .error {{ color: #dc3545; }}
                    .info {{ color: #17a2b8; }}
                    table {{ width: 100%; border-collapse: collapse; margin-top: 10px; }}
                    th, td {{ padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }}
                    th {{ background: #f8f9fa; }}
                    .badge {{ padding: 5px 10px; border-radius: 20px; font-size: 12px; }}
                    .badge-success {{ background: #d4edda; color: #155724; }}
                    .badge-danger {{ background: #f8d7da; color: #721c24; }}
                    .badge-warning {{ background: #fff3cd; color: #856404; }}
                </style>
            </head>
            <body>
                <h1>🛠️ Page de Debug - MaBoutique</h1>
                
                <div class='card'>
                    <h2>📊  Catégories disponibles</h2>
                    <p><strong>Nombre total :</strong> {categoriesDisponibles.Length} catégories</p>
                    <ul>
            ";

        foreach (var categorie in categoriesDisponibles)
        {
            html += $"<li>{categorie}</li>";
        }

        html += $@"
                    </ul>
                </div>

                <div class='card'>
                    <h2>🔐 Authentification</h2>
                    <p><strong>Utilisateur :</strong> {context.User.Identity?.Name ?? "Non connecté"}</p>
                    <p><strong>Authentifié :</strong> {context.User.Identity?.IsAuthenticated}</p>
                    <p><strong>Rôles :</strong> {string.Join(", ", context.User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value))}</p>
                </div>

                <div class='card'>
                    <h2>🗄️ Base de données</h2>
        ";

        try
        {
            var produitsCount = await db.Produits.CountAsync();
            var categoriesCount = await db.Produits.Select(p => p.Categorie).Distinct().CountAsync();

            html += $@"
                    <p class='success'>✅ Connexion à la base réussie</p>
                    <p><strong>Produits dans la base :</strong> {produitsCount}</p>
                    <p><strong>Catégories utilisées :</strong> {categoriesCount}/{categoriesDisponibles.Length}</p>
                    
                    <table>
                        <thead>
                            <tr>
                                <th>ID</th>
                                <th>Nom</th>
                                <th>Catégorie</th>
                                <th>Prix</th>
                                <th>Stock</th>
                                <th>Date création</th>
                            </tr>
                        </thead>
                        <tbody>
            ";

            var produits = await db.Produits.OrderByDescending(p => p.Id).Take(10).ToListAsync();
            foreach (var p in produits)
            {
                html += $@"
                            <tr>
                                <td>{p.Id}</td>
                                <td>{p.Nom}</td>
                                <td><span class='badge badge-info'>{p.Categorie}</span></td>
                                <td>{p.Prix:C}</td>
                                <td><span class='badge {(p.Quantite > 0 ? "badge-success" : "badge-danger")}'>{p.Quantite}</span></td>
                                <td>{p.DateCreation:dd/MM/yyyy HH:mm}</td>
                            </tr>
                ";
            }

            html += @"
                        </tbody>
                    </table>
                    
                    <h3>📈 Statistiques par catégorie</h3>
                    <table>
                        <thead>
                            <tr>
                                <th>Catégorie</th>
                                <th>Produits</th>
                                <th>Stock total</th>
                                <th>Prix moyen</th>
                            </tr>
                        </thead>
                        <tbody>
            ";

            var stats = await db.Produits
                .GroupBy(p => p.Categorie)
                .Select(g => new
                {
                    Categorie = g.Key,
                    Count = g.Count(),
                    Stock = g.Sum(p => p.Quantite),
                    PrixMoyen = g.Average(p => p.Prix)
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            foreach (var stat in stats)
            {
                html += $@"
                            <tr>
                                <td><strong>{stat.Categorie}</strong></td>
                                <td>{stat.Count}</td>
                                <td>{stat.Stock}</td>
                                <td>{stat.PrixMoyen:C}</td>
                            </tr>
                ";
            }

            html += @"
                        </tbody>
                    </table>
            ";
        }
        catch (Exception ex)
        {
            html += $@"
                    <p class='error'>❌ Erreur base de données : {ex.Message}</p>
            ";
        }

        html += @"
                </div>

                <div class='card'>
                    <h2>🔄 Routes disponibles</h2>
                    <ul>
                        <li><a href='/'>/ - Accueil public</a></li>
                        <li><a href='/Account/Login'>/Account/Login - Page de connexion</a></li>
                        <li><a href='/Admin'>/Admin - Administration (nécessite connexion)</a></li>
                        <li><a href='/Account/Profile'>/Account/Profile - Profil utilisateur</a></li>
                        <li><a href='/debug'>/debug - Cette page</a></li>
                    </ul>
                </div>

                <div class='card'>
                    <h2>🔑 Test rapide de connexion</h2>
                    <form action='/Account/Login' method='post' style='background: #f8f9fa; padding: 15px; border-radius: 5px;'>
                        <input type='hidden' name='returnUrl' value='/Admin' />
                        <div style='margin-bottom: 10px;'>
                            <label>Nom d'utilisateur :</label><br>
                            <input type='text' name='username' value='admin' style='width: 100%; padding: 8px; margin-top: 5px;'>
                        </div>
                        <div style='margin-bottom: 10px;'>
                            <label>Mot de passe :</label><br>
                            <input type='password' name='password' value='admin123' style='width: 100%; padding: 8px; margin-top: 5px;'>
                        </div>
                        <button type='submit' style='background: #007bff; color: white; border: none; padding: 10px 20px; border-radius: 5px; cursor: pointer;'>
                            🔐 Se connecter comme Admin
                        </button>
                    </form>
                </div>
            </body>
            </html>
        ";

        return Results.Content(html, "text/html");
    });
}

// Lancement de l'application
Console.WriteLine("");
Console.WriteLine("🚀  APPLICATION DÉMARRÉE !");
Console.WriteLine("==========================");
Console.WriteLine($"   URL : https://localhost:5001");
Console.WriteLine($"   Environnement : {app.Environment.EnvironmentName}");
Console.WriteLine($"   Catégories : {categoriesDisponibles.Length} disponibles");
Console.WriteLine($"   Date/heure : {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
Console.WriteLine("");

app.Run();