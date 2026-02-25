using TippspielApp.Domain;

namespace TippspielApp.Infrastructure
{
    /// <summary>
    /// Erzeugt leere User-Vorlagen auf Basis der aktuellen Turnierdaten.
    /// Ermöglicht Clients, eine befüllbare Tipp-Struktur abzurufen.
    /// </summary>
    public static class UserTemplateBuilder
    {
        public static User CreateEmpty(TournamentData tourney)
        {
            var bingoEventIds = tourney.BingoEventCatalog
                .Select(e => e.EventId)
                .ToList();

            var cells = new List<BingoCell>();
            for (int pos = 0; pos < 16; pos++)
            {
                cells.Add(new BingoCell
                {
                    Position    = pos,
                    EventId     = pos < bingoEventIds.Count ? bingoEventIds[pos] : "EVT_PLACEHOLDER",
                    IsFulfilled = false
                });
            }

            var allTeams = tourney.Teams.Select(t => t.TeamId).ToList();

            return new User
            {
                UserId  = string.Empty,
                Name    = string.Empty,
                BetData = new UserBet
                {
                    GroupMatchBets = tourney.MatchResults
                        .Select(m => new MatchBet { MatchId = m.MatchId })
                        .ToList(),
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
                CurrentScore = new ScoreSnapshot()
            };
        }
    }
}
