using TippspielApp.Domain;

namespace TippspielApp.Infrastructure
{
    /// <summary>
    /// Erstellt eine leere Tipp-Vorlage für neue Tipper.
    /// So weiß der Client genau, welche Felder er befüllen muss.
    /// </summary>
    public static class UserTemplateBuilder
    {
        public static User CreateEmpty(TournamentData tourney)
        {
            // Alle Ereignis-IDs aus dem Katalog holen – werden den 16 Zellen zugewiesen
            var bingoEventIds = tourney.BingoEventCatalog
                .Select(e => e.EventId)
                .ToList();

            var cells = new List<BingoCell>();
            // 16 Felder aufbauen (Position 0–15)
            for (int pos = 0; pos < 16; pos++)
            {
                cells.Add(new BingoCell
                {
                    Position    = pos,
                    // Wenn weniger als 16 Ereignisse vorhanden sind, Platzhalter einsetzen
                    EventId     = pos < bingoEventIds.Count ? bingoEventIds[pos] : "EVT_PLACEHOLDER",
                    IsFulfilled = false  // bei neuen Usern immer unausgefüllt
                });
            }

            // Alle Team-IDs für die KO-Vorlagen brauchen wir später
            var allTeams = tourney.Teams.Select(t => t.TeamId).ToList();

            return new User
            {
                UserId  = string.Empty,
                Name    = string.Empty,
                BetData = new UserBet
                {
                    // Für jedes Spiel aus den Turnierdaten eine leere Tipp-Zeile erstellen
                    GroupMatchBets = tourney.MatchResults
                        .Select(m => new MatchBet { MatchId = m.MatchId })
                        .ToList(),
                    // KO-Tipps mit allen Teams vorbelegen – der User wählt dann seine Favoriten aus
                    KnockoutBets = new Dictionary<string, List<string>>
                    {
                        ["RoundOf32"]    = allTeams.Take(32).ToList(),
                        ["RoundOf16"]    = allTeams.Take(16).ToList(),
                        ["QuarterFinal"] = allTeams.Take(8).ToList(),
                        ["SemiFinal"]    = allTeams.Take(4).ToList(),
                        ["Final"]        = allTeams.Take(2).ToList(),
                    },
                    SpecialBets = new SpecialBet
                    {
                        WorldChampionTeamId = string.Empty,
                        TopScorerName       = string.Empty
                    },
                    BingoCard = new BingoCard { Cells = cells }
                },
                CurrentScore = new ScoreSnapshot()  // leer, wird beim ersten Recalculate befüllt
            };
        }
    }
}
