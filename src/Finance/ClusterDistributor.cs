using TippspielApp.Domain;
using TippspielApp.Scoring;

namespace TippspielApp.Finance
{
    /// <summary>
    /// Berechnet die Gruppen-Cluster-Zwischengewinne (A+B, C+D, …, K+L).
    /// Der User mit den meisten Spielpunkten im Cluster gewinnt den Cluster-Topf.
    /// Verwendet EvaluatorBase.CalculateMatchPoints() für eine konsistente Punkteberechnung.
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
            decimal prize = PotMath.ClusterPrize(users.Count);

            foreach (var cluster in Clusters)
            {
                var groups  = new HashSet<string>(cluster, StringComparer.OrdinalIgnoreCase);
                var matches = data.MatchResults
                    .Where(m => m.IsFinished && groups.Contains(m.GroupName))
                    .ToDictionary(m => m.MatchId, StringComparer.OrdinalIgnoreCase);

                if (matches.Count == 0) continue;

                var scores = users.Select(u =>
                {
                    int pts = 0;
                    if (u.BetData?.GroupMatchBets != null)
                        foreach (var bet in u.BetData.GroupMatchBets)
                            if (matches.TryGetValue(bet.MatchId, out var result))
                                pts += EvaluatorBase.CalculateMatchPoints(bet, result);
                    return (User: u, Points: pts);
                }).ToList();

                int maxPts = scores.Max(s => s.Points);
                if (maxPts <= 0) continue;

                var winners   = scores.Where(s => s.Points == maxPts).ToList();
                decimal each  = prize / winners.Count;
                string  name  = string.Join("+", cluster);

                var cr = new GroupClusterResult
                {
                    ClusterLabel        = name,
                    Prize               = prize,
                    IsShared            = winners.Count > 1,
                    CoWinners           = winners.Select(w => w.User.Name).ToList(),
                    WinnerClusterPoints = maxPts,
                    WinnerUserId        = winners.Count == 1 ? winners[0].User.UserId : "(geteilt)",
                    WinnerName          = string.Join(", ", winners.Select(w => w.User.Name))
                };

                foreach (var (winner, _) in winners)
                    AddWin(winner, each, $"Gruppencluster {name}");

                results.Add(cr);
            }

            return results;
        }
    }
}
