using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Text.Json;
using TippspielApp.Domain;
using TippspielApp.Infrastructure;

namespace TippspielApp.Application
{
    /// <summary>
    /// Konfiguriert und startet den ASP.NET Minimal-API-Webserver.
    /// Bündelt alle HTTP-Routen an einem Ort.
    /// </summary>
    public static class WebServer
    {
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented               = true,
            PropertyNamingPolicy        = null
        };

        public static void Run(string[] args)
        {
            string root        = FindProjectRoot(AppContext.BaseDirectory);
            string usersPath   = Path.Combine(root, "Data", "Input",  "users.json");
            string tourneyPath = Path.Combine(root, "Data", "Input",  "tournament_data.json");
            string rankingPath = Path.Combine(root, "Data", "Output", "ranking_current.json");
            string wwwrootPath = Path.Combine(root, "wwwroot");

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddCors(o => o.AddDefaultPolicy(
                p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            var app = builder.Build();
            app.UseCors();
            app.UseDefaultFiles(new DefaultFilesOptions
            {
                FileProvider = new PhysicalFileProvider(wwwrootPath),
                RequestPath  = ""
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(wwwrootPath),
                RequestPath  = ""
            });

            RegisterRoutes(app, root, usersPath, tourneyPath, rankingPath);

            Console.WriteLine();
            Console.WriteLine("WM 2026 Tippspiel - Web-Interface");
            Console.WriteLine("  URL: http://localhost:5174");
            Console.WriteLine("  Beenden mit Strg+C");
            Console.WriteLine();
            app.Run("http://localhost:5174");
        }

        private static void RegisterRoutes(
            WebApplication app,
            string root,
            string usersPath,
            string tourneyPath,
            string rankingPath)
        {
            // GET /api/tournament?dataset=vor_turnier|nach_gruppe|nach_turnier
            app.MapGet("/api/tournament", (HttpContext ctx) =>
            {
                string datasetKey = ctx.Request.Query["dataset"].FirstOrDefault() ?? "";
                string tdPath = string.IsNullOrEmpty(datasetKey)
                    ? tourneyPath  // legacy fallback
                    : Path.Combine(root, "Data", "Input", $"tournament_data_{datasetKey}.json");

                // Fallback-Kette: gesuchte Datei → vor_turnier → erste vorhandene
                if (!File.Exists(tdPath))
                    tdPath = Path.Combine(root, "Data", "Input", "tournament_data_vor_turnier.json");
                if (!File.Exists(tdPath))
                    tdPath = Directory.GetFiles(Path.Combine(root, "Data", "Input"), "tournament_data*.json")
                                      .OrderBy(f => f).FirstOrDefault() ?? tdPath;
                if (!File.Exists(tdPath))
                    return Results.NotFound("Turnierdaten nicht gefunden.");

                return Results.Content(File.ReadAllText(tdPath, System.Text.Encoding.UTF8), "application/json");
            });

            // GET /api/users
            app.MapGet("/api/users", () =>
                Results.Content(File.ReadAllText(usersPath, System.Text.Encoding.UTF8), "application/json"));

            // GET /api/users/lookup?q=... – sucht per Name (case-insensitiv) oder UserId
            app.MapGet("/api/users/lookup", (HttpContext ctx) =>
            {
                string q = (ctx.Request.Query["q"].FirstOrDefault() ?? "").Trim();
                if (string.IsNullOrEmpty(q))
                    return Results.BadRequest("Parameter 'q' fehlt");

                var allUsers = JsonSerializer.Deserialize<List<User>>(
                    File.ReadAllText(usersPath, System.Text.Encoding.UTF8), JsonOpts) ?? [];

                var found = allUsers.FirstOrDefault(u =>
                    string.Equals(u.UserId, q, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(u.Name,   q, StringComparison.OrdinalIgnoreCase));

                if (found == null)
                    return Results.NotFound("Tipper nicht gefunden");

                return Results.Json(found, JsonOpts);
            });

            // GET /api/user-template
            app.MapGet("/api/user-template", () =>
            {
                var tourney = JsonSerializer.Deserialize<TournamentData>(
                    File.ReadAllText(tourneyPath, System.Text.Encoding.UTF8), JsonOpts)!;
                return Results.Json(UserTemplateBuilder.CreateEmpty(tourney), JsonOpts);
            });

            // GET /api/tournament-datasets
            app.MapGet("/api/tournament-datasets", () =>
            {
                string inputDir = Path.Combine(root, "Data", "Input");
                var files = Directory.GetFiles(inputDir, "tournament_data*.json")
                    .Select(f =>
                    {
                        string name  = Path.GetFileNameWithoutExtension(f);
                        string key   = name == "tournament_data" ? "" : name["tournament_data_".Length..];
                        string label = string.IsNullOrEmpty(key)
                            ? "Standard (live)"
                            : char.ToUpper(key[0]) + key[1..].Replace("_", " ");
                        return new { Key = key, Label = label };
                    })
                    .OrderBy(x => x.Key.Length);
                return Results.Json(files, JsonOpts);
            });

            // GET /api/ranking
            app.MapGet("/api/ranking", () =>
            {
                if (!File.Exists(rankingPath))
                    return Results.NotFound("Noch kein Ranking berechnet. Bitte /api/ranking/recalculate aufrufen.");
                return Results.Content(File.ReadAllText(rankingPath, System.Text.Encoding.UTF8), "application/json");
            });

            // POST /api/ranking/recalculate?dataset=test
            app.MapPost("/api/ranking/recalculate", (HttpContext ctx) =>
            {
                string datasetKey = ctx.Request.Query["dataset"].FirstOrDefault() ?? "";
                string tdPath = string.IsNullOrEmpty(datasetKey)
                    ? tourneyPath
                    : Path.Combine(root, "Data", "Input", $"tournament_data_{datasetKey}.json");

                if (!File.Exists(tdPath))
                    return Results.NotFound($"Datensatz '{datasetKey}' nicht gefunden.");

                var store  = new JsonDataStore();
                var users  = store.LoadUsers(usersPath);
                var td     = store.LoadTournamentData(tdPath);
                var report = new RankingCalculator().Run(users, td);
                store.ExportRanking(report, rankingPath);
                return Results.Json(report, JsonOpts);
            });

            // POST /api/users
            app.MapPost("/api/users", async (HttpContext ctx) =>
            {
                string body   = await new StreamReader(ctx.Request.Body).ReadToEndAsync();
                var newUser   = JsonSerializer.Deserialize<User>(body, JsonOpts);
                if (newUser == null) return Results.BadRequest("Ungueltige User-Daten");

                var users = JsonSerializer.Deserialize<List<User>>(
                    File.ReadAllText(usersPath, System.Text.Encoding.UTF8), JsonOpts) ?? [];

                if (string.IsNullOrWhiteSpace(newUser.UserId))
                    newUser.UserId = "USR-" + (users.Count + 1).ToString("D3");

                if (users.Any(u => u.UserId == newUser.UserId))
                    return Results.Conflict($"User-ID {newUser.UserId} existiert bereits");

                users.Add(newUser);
                File.WriteAllText(usersPath, JsonSerializer.Serialize(users, JsonOpts), System.Text.Encoding.UTF8);
                return Results.Created("/api/users/" + newUser.UserId, newUser);
            });

            // PUT /api/users/{id}
            app.MapPut("/api/users/{id}", async (string id, HttpContext ctx) =>
            {
                string body   = await new StreamReader(ctx.Request.Body).ReadToEndAsync();
                var updated   = JsonSerializer.Deserialize<User>(body, JsonOpts);
                if (updated == null) return Results.BadRequest("Ungueltige User-Daten");

                var users = JsonSerializer.Deserialize<List<User>>(
                    File.ReadAllText(usersPath, System.Text.Encoding.UTF8), JsonOpts) ?? [];
                int idx = users.FindIndex(u => u.UserId == id);
                if (idx < 0) return Results.NotFound($"User {id} nicht gefunden");

                updated.UserId = id;
                users[idx] = updated;
                File.WriteAllText(usersPath, JsonSerializer.Serialize(users, JsonOpts), System.Text.Encoding.UTF8);
                return Results.Ok(updated);
            });

            // DELETE /api/users/{id}
            app.MapDelete("/api/users/{id}", (string id) =>
            {
                var users = JsonSerializer.Deserialize<List<User>>(
                    File.ReadAllText(usersPath, System.Text.Encoding.UTF8), JsonOpts) ?? [];
                int removed = users.RemoveAll(u => u.UserId == id);
                if (removed == 0) return Results.NotFound($"User {id} nicht gefunden");
                File.WriteAllText(usersPath, JsonSerializer.Serialize(users, JsonOpts), System.Text.Encoding.UTF8);
                return Results.Ok($"User {id} geloescht");
            });
        }

        private static string FindProjectRoot(string startDir)
        {
            string? dir = startDir;
            while (dir != null)
            {
                if (Directory.GetFiles(dir, "*.csproj").Length > 0) return dir;
                dir = Directory.GetParent(dir)?.FullName;
            }
            return startDir;
        }
    }
}
