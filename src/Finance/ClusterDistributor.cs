using TippspielApp.Domain;
using TippspielApp.Scoring;

namespace TippspielApp.Finance
{
    /// <summary>
    /// Wertet die sechs Gruppen-Cluster aus (A+B, C+D, …, K+L) und verteilt den Cluster-Preis.
    /// Wichtig: Nur Spielpunkte aus den jeweiligen Gruppen zählen, keine KO- oder Bingo-Punkte.
    /// CalculateMatchPoints() aus EvaluatorBase wird hier direkt wiederverwendet.
    /// </summary>
    public class ClusterDistributor : PrizeDistributorBase
    {
        private static readonly string[][] Clusters =
        [
            ["A", "B"], ["C", "D"], ["E", "F"],
            ["G", "H"], ["I", "J"], ["K", "L"]
        ];

        public List<GroupClusterResult> Calculate(List<User> users, TournamentData data)
        {
            var results = new List<GroupClusterResult>();
            // Preis pro Cluster ist von der aktuellen Teilnehmerzahl abhängig
            decimal prize = PotMath.ClusterPrize(users.Count);

            foreach (var cluster in Clusters)
            {
                // Die zwei Gruppen des Clusters als Set – z.B. {"A", "B"}
                var groups  = new HashSet<string>(cluster, StringComparer.OrdinalIgnoreCase);
                // Nur abgeschlossene Spiele aus genau diesen Gruppen einsammeln
                var matches = data.MatchResults
                    .Where(m => m.IsFinished && groups.Contains(m.GroupName))
                    .ToDictionary(m => m.MatchId, StringComparer.OrdinalIgnoreCase);

                // Cluster noch nicht gespielt – überspringen
                if (matches.Count == 0) continue;

                // Für jeden Tipper die Punkte aus genau diesem Cluster ausrechnen
                var scores = users.Select(u =>
                {
                    int pts = 0;
                    if (u.BetData?.GroupMatchBets != null)
                        foreach (var bet in u.BetData.GroupMatchBets)
                            // TryGetValue liefert false für Tipps aus anderen Gruppen → werden ignoriert
                            if (matches.TryGetValue(bet.MatchId, out var result))
                                pts += EvaluatorBase.CalculateMatchPoints(bet, result);
                    return (User: u, Points: pts);
                }).ToList();

                int maxPts = scores.Max(s => s.Points);
                // Niemand hat einen gültigen Tipp für diesen Cluster – kein Gewinner
                if (maxPts <= 0) continue;

                // Alle Tipper mit Maximalpunktzahl sind Gewinner (Gleichstand = Teilen)
                var winners   = scores.Where(s => s.Points == maxPts).ToList();
                // Bei mehreren Gewinnern wird der Preis gleichmäßig aufgeteilt
                decimal each  = prize / winners.Count;
                string  name  = string.Join("+", cluster);  // z.B. "A+B"

                var cr = new GroupClusterResult
                {
                    ClusterLabel        = name,
                    Prize               = prize,
                    IsShared            = winners.Count > 1,
                    CoWinners           = winners.Select(w => w.User.Name).ToList(),
                    WinnerClusterPoints = maxPts,
                    // Bei Gleichstand UserId auf "(geteilt)" setzen – dadurch sieht man es im Report
                    WinnerUserId        = winners.Count == 1 ? winners[0].User.UserId : "(geteilt)",
                    WinnerName          = string.Join(", ", winners.Select(w => w.User.Name))
                };

                // Preisgeld auf jeden Gewinner einzeln verbuchen
                foreach (var (winner, _) in winners)
                    AddWin(winner, each, $"Gruppencluster {name}");

                results.Add(cr);
            }

            return results;
        }
    }
}
