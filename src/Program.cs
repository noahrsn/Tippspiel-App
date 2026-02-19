using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using TippspielApp.Models;

namespace TippspielApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Pfade zu den JSON-Dateien (relativ zum Ausgabeverzeichnis der Anwendung)
            string baseDir = AppContext.BaseDirectory; // z. B. bin/Debug/net10.0/
            string usersPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Data", "Input", "users.json"));
            string tournamentPath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Data", "Input", "tournament_data.json"));

            // Falls die Dateien noch nicht da sind, brechen wir ab
            if (!File.Exists(usersPath) || !File.Exists(tournamentPath))
            {
                Console.WriteLine($"JSON-Dateien nicht gefunden. Erwartete Pfade:\n - {usersPath}\n - {tournamentPath}");
                return;
            }

            // 1. JSON einlesen und deserialisieren
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var users = JsonSerializer.Deserialize<List<User>>(File.ReadAllText(usersPath), jsonOptions);
            var tournamentData = JsonSerializer.Deserialize<TournamentData>(File.ReadAllText(tournamentPath), jsonOptions);

            if (users == null || tournamentData == null || users.Count == 0) return;

            // 2. Ersten User für den Basic-Test auswählen
            var testUser = users[0];
            Console.WriteLine($"--- Auswertung für: {testUser.Name} ---");
            
            int totalMatchPoints = 0;

            // 3. Spiele auswerten (3-2-1 Regel)
            foreach (var bet in testUser.BetData.GroupMatchBets)
            {
                // Reales Ergebnis zum Tipp suchen
                var actualResult = tournamentData.MatchResults.FirstOrDefault(m => m.MatchId == bet.MatchId);

                // Nur fertige Spiele auswerten
                if (actualResult != null && actualResult.IsFinished)
                {
                    int points = 0;
                    int betDiff = bet.HomeGoals - bet.AwayGoals;
                    int actualDiff = actualResult.HomeGoals - actualResult.AwayGoals;

                    if (bet.HomeGoals == actualResult.HomeGoals && bet.AwayGoals == actualResult.AwayGoals)
                    {
                        points = 3; // Exaktes Ergebnis
                    }
                    else if (betDiff == actualDiff)
                    {
                        points = 2; // Richtige Tordifferenz (gilt auch für Unentschieden)
                    }
                    else if (Math.Sign(betDiff) == Math.Sign(actualDiff))
                    {
                        points = 1; // Richtige Tendenz (Gewinner stimmt)
                    }

                    Console.WriteLine($"[{bet.MatchId}] Tipp: {bet.HomeGoals}:{bet.AwayGoals} | Ergebnis: {actualResult.HomeGoals}:{actualResult.AwayGoals} => {points} Punkte");
                    totalMatchPoints += points;
                }
            }

            Console.WriteLine($"\nGesamtpunkte (Spiele): {totalMatchPoints}");
        }
    }
}