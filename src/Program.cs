using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Text.Json;
using TippspielApp.Models;
using TippspielApp.Services;

namespace TippspielApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            string projectRoot = FindProjectRoot(AppContext.BaseDirectory);

            if (args.Contains("--web"))
                RunWebServer(args, projectRoot);
            else
                RunConsoleCalculation(projectRoot);
        }

        // CONSOLE MODE
        static void RunConsoleCalculation(string projectRoot)
        {
            Console.WriteLine("WM 2026 - Tippspiel & Bingo Backend");
            Console.WriteLine();
            string usersPath   = Path.Combine(projectRoot, "Data", "Input",  "users.json");
            string tourneyPath = Path.Combine(projectRoot, "Data", "Input",  "tournament_data.json");
            string outputPath  = Path.Combine(projectRoot, "Data", "Output", "ranking_current.json");

            var dh = new DataHandler();
            List<User> users = dh.LoadUsers(usersPath);
            TournamentData td = dh.LoadTournamentData(tourneyPath);
            Console.WriteLine($"   {users.Count} User geladen, {td.MatchResults.Count(m => m.IsFinished)} abgeschlossene Spiele.");

            var engine = new CalculationEngine();
            var report = engine.RunDailyCalculation(users, td);
            Console.WriteLine($"   Generiert: {report.GeneratedAt:dd.MM.yyyy HH:mm} UTC");
            Console.WriteLine();
            Console.WriteLine("RANKING");
            Console.WriteLine("=======================================================");
            foreach (var e in report.Leaderboard)
                Console.WriteLine($"  {e.Rank+".",-6} {e.Name,-22} {e.TotalPoints+" Pkt",-10} {e.TotalFinancialWinnings:F0} EUR");
            Console.WriteLine("=======================================================");
            Console.WriteLine($"  Ausgeschuettet: {report.FinanceSummary.DistributedAmount:F0} / {report.FinanceSummary.TotalPot:F0} EUR");
            Console.WriteLine();
            Console.WriteLine("GRUPPEN-CLUSTER ZWISCHENGEWINNE");
            foreach (var r in report.GroupClusterResults)
                Console.WriteLine($"  Gruppe {r.ClusterLabel,-5}  {r.WinnerName,-22}  {r.WinnerClusterPoints} Pkt  {r.Prize:F0} EUR");

            dh.ExportRanking(report, outputPath);
            Console.WriteLine();
            Console.WriteLine($"Ranking gespeichert: {outputPath}");
        }

        // WEB SERVER MODE
        static void RunWebServer(string[] args, string projectRoot)
        {
            string usersPath    = Path.Combine(projectRoot, "Data", "Input",  "users.json");
            string tourneyPath  = Path.Combine(projectRoot, "Data", "Input",  "tournament_data.json");
            string rankingPath  = Path.Combine(projectRoot, "Data", "Output", "ranking_current.json");
            string wwwrootPath  = Path.Combine(projectRoot, "wwwroot");

            var jsonOpts = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                PropertyNamingPolicy = null
            };

            var webArgs = args.Where(a => a != "--web").ToArray();
            var builder = WebApplication.CreateBuilder(webArgs);
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

            // GET /api/tournament
            app.MapGet("/api/tournament", () =>
                Results.Content(File.ReadAllText(tourneyPath, System.Text.Encoding.UTF8), "application/json"));

            // GET /api/users
            app.MapGet("/api/users", () =>
                Results.Content(File.ReadAllText(usersPath, System.Text.Encoding.UTF8), "application/json"));

            // GET /api/user-template
            app.MapGet("/api/user-template", () =>
            {
                var tourney = JsonSerializer.Deserialize<TournamentData>(
                    File.ReadAllText(tourneyPath, System.Text.Encoding.UTF8), jsonOpts)!;
                return Results.Json(UserTemplate.CreateEmpty(tourney), jsonOpts);
            });

            // GET /api/ranking  – liest gespeichertes Ranking
            app.MapGet("/api/ranking", () =>
            {
                if (!File.Exists(rankingPath))
                    return Results.NotFound("Noch kein Ranking berechnet. Bitte /api/ranking/recalculate aufrufen.");
                return Results.Content(File.ReadAllText(rankingPath, System.Text.Encoding.UTF8), "application/json");
            });

            // POST /api/ranking/recalculate  – berechnet neu und speichert
            app.MapPost("/api/ranking/recalculate", () =>
            {
                var dh = new DataHandler();
                var users  = dh.LoadUsers(usersPath);
                var td     = dh.LoadTournamentData(tourneyPath);
                var report = new CalculationEngine().RunDailyCalculation(users, td);
                dh.ExportRanking(report, rankingPath);
                return Results.Json(report, jsonOpts);
            });

            // POST /api/users
            app.MapPost("/api/users", async (HttpContext ctx) =>
            {
                string body = await new System.IO.StreamReader(ctx.Request.Body).ReadToEndAsync();
                var newUser = JsonSerializer.Deserialize<User>(body, jsonOpts);
                if (newUser == null) return Results.BadRequest("Ungueltige User-Daten");

                var users = JsonSerializer.Deserialize<List<User>>(
                    File.ReadAllText(usersPath, System.Text.Encoding.UTF8), jsonOpts) ?? [];

                if (string.IsNullOrWhiteSpace(newUser.UserId))
                    newUser.UserId = "USR-" + (users.Count + 1).ToString("D3");

                if (users.Any(u => u.UserId == newUser.UserId))
                    return Results.Conflict($"User-ID {newUser.UserId} existiert bereits");

                users.Add(newUser);
                File.WriteAllText(usersPath, JsonSerializer.Serialize(users, jsonOpts), System.Text.Encoding.UTF8);
                return Results.Created("/api/users/" + newUser.UserId, newUser);
            });

            // PUT /api/users/{id}
            app.MapPut("/api/users/{id}", async (string id, HttpContext ctx) =>
            {
                string body = await new System.IO.StreamReader(ctx.Request.Body).ReadToEndAsync();
                var updated = JsonSerializer.Deserialize<User>(body, jsonOpts);
                if (updated == null) return Results.BadRequest("Ungueltige User-Daten");

                var users = JsonSerializer.Deserialize<List<User>>(
                    File.ReadAllText(usersPath, System.Text.Encoding.UTF8), jsonOpts) ?? [];
                int idx = users.FindIndex(u => u.UserId == id);
                if (idx < 0) return Results.NotFound($"User {id} nicht gefunden");

                updated.UserId = id;
                users[idx] = updated;
                File.WriteAllText(usersPath, JsonSerializer.Serialize(users, jsonOpts), System.Text.Encoding.UTF8);
                return Results.Ok(updated);
            });

            // DELETE /api/users/{id}
            app.MapDelete("/api/users/{id}", (string id) =>
            {
                var users = JsonSerializer.Deserialize<List<User>>(
                    File.ReadAllText(usersPath, System.Text.Encoding.UTF8), jsonOpts) ?? [];
                int removed = users.RemoveAll(u => u.UserId == id);
                if (removed == 0) return Results.NotFound($"User {id} nicht gefunden");
                File.WriteAllText(usersPath, JsonSerializer.Serialize(users, jsonOpts), System.Text.Encoding.UTF8);
                return Results.Ok($"User {id} geloescht");
            });

            Console.WriteLine();
            Console.WriteLine("WM 2026 Tippspiel - Web-Interface");
            Console.WriteLine("  URL: http://localhost:5174");
            Console.WriteLine("  Beenden mit Strg+C");
            Console.WriteLine();
            app.Run("http://localhost:5174");
        }

        static string FindProjectRoot(string startDir)
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

    static class UserTemplate
    {
        public static User CreateEmpty(TournamentData tourney)
        {
            var bingoEventIds = tourney.BingoEventCatalog
                .Where(e => e.EventId != "FREE_SPACE")
                .Select(e => e.EventId).ToList();

            var cells = new List<BingoCell>();
            int ei = 0;
            for (int pos = 0; pos < 25; pos++)
            {
                if (pos == 12)
                    cells.Add(new BingoCell { Position = 12, EventId = "FREE_SPACE", IsFulfilled = true,
                        FulfilledAt = new DateTime(2026, 6, 11, 0, 0, 0, DateTimeKind.Utc) });
                else
                {
                    cells.Add(new BingoCell
                    {
                        Position = pos,
                        EventId  = ei < bingoEventIds.Count ? bingoEventIds[ei] : "EVT_PLACEHOLDER",
                        IsFulfilled = false
                    });
                    ei++;
                }
            }

            var matchBets = tourney.MatchResults
                .Select(m => new MatchBet { MatchId = m.MatchId, HomeGoals = 0, AwayGoals = 0 })
                .ToList();
            var allTeams = tourney.Teams.Select(t => t.TeamId).ToList();

            return new User
            {
                UserId = string.Empty,
                Name   = string.Empty,
                BetData = new UserBet
                {
                    GroupMatchBets = matchBets,
                    KnockoutBets = new Dictionary<string, List<string>>
                    {
                        ["RoundOf32"]    = allTeams.Take(32).ToList(),
                        ["RoundOf16"]    = allTeams.Take(16).ToList(),
                        ["QuarterFinal"] = allTeams.Take(8).ToList(),
                        ["SemiFinal"]    = allTeams.Take(4).ToList(),
                        ["Final"]        = allTeams.Take(2).ToList(),
                    },
                    SpecialBets = new SpecialBet { WorldChampionTeamId = string.Empty, TopScorerName = string.Empty },
                    BingoCard   = new BingoCard { Cells = cells }
                },
                CurrentScore = new ScoreBoard()
            };
        }
    }
}
