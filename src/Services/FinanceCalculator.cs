using TippspielApp.Models;

namespace TippspielApp.Services
{
    public class FinanceCalculator
    {
        private static readonly string[][] GroupClusters =
        [
            ["A", "B"], ["C", "D"], ["E", "F"],
            ["G", "H"], ["I", "J"], ["K", "L"]
        ];

        private readonly BingoEvaluator _bingoEvaluator = new();

        public List<GroupClusterResult> CalculateGroupClusterWinnings(List<User> users, TournamentData data)
        {
            var results = new List<GroupClusterResult>();

            foreach (var cluster in GroupClusters)
            {
                var clusterGroups = new HashSet<string>(cluster, StringComparer.OrdinalIgnoreCase);
                var relevantMatches = data.MatchResults
                    .Where(m => m.IsFinished && clusterGroups.Contains(m.GroupName))
                    .ToDictionary(m => m.MatchId, m => m, StringComparer.OrdinalIgnoreCase);

                if (relevantMatches.Count == 0) continue;

                var evaluator = new ClassicBetEvaluator();
                var scores = users.Select(u =>
                {
                    int pts = 0;
                    if (u.BetData?.GroupMatchBets != null)
                        foreach (var bet in u.BetData.GroupMatchBets)
                            if (relevantMatches.TryGetValue(bet.MatchId, out var result))
                                pts += evaluator.CalculateMatchPoints(bet, result);
                    return (User: u, Points: pts);
                }).ToList();

                int maxPts = scores.Max(s => s.Points);
                if (maxPts <= 0) continue;

                var winners = scores.Where(s => s.Points == maxPts).ToList();
                decimal prizePerWinner = 50m / winners.Count;
                string clusterName = string.Join("+", cluster);

                var clusterResult = new GroupClusterResult
                {
                    ClusterLabel = clusterName,
                    Prize = 50m,
                    IsShared = winners.Count > 1,
                    CoWinners = winners.Select(w => w.User.Name).ToList()
                };

                if (winners.Count == 1)
                {
                    clusterResult.WinnerUserId = winners[0].User.UserId;
                    clusterResult.WinnerName   = winners[0].User.Name;
                    clusterResult.WinnerClusterPoints = maxPts;
                }
                else
                {
                    clusterResult.WinnerUserId = "(geteilt)";
                    clusterResult.WinnerName   = string.Join(", ", winners.Select(w => w.User.Name));
                    clusterResult.WinnerClusterPoints = maxPts;
                }

                foreach (var (winner, _) in winners)
                {
                    winner.CurrentScore.TotalFinancialWinnings += prizePerWinner;
                    winner.CurrentScore.WonPots.Add($"Gruppencluster {clusterName} ({prizePerWinner:F0} EUR)");
                }

                results.Add(clusterResult);
            }

            return results;
        }

        public List<BingoPotResult> CalculateBingoFinancialWins(List<User> users)
        {
            var results = new List<BingoPotResult>();

            var usersWithLines = new List<(User Player, DateTime EarliestLine)>();
            foreach (var u in users)
            {
                var card = u.BetData?.BingoCard;
                if (card == null) continue;
                var earliest = GetEarliestLineCompletion(card);
                if (earliest.HasValue)
                    usersWithLines.Add((u, earliest.Value));
            }
            usersWithLines = usersWithLines.OrderBy(x => x.EarliestLine).ToList();

            if (usersWithLines.Count >= 1)
            {
                var w = usersWithLines[0].Player;
                w.CurrentScore.TotalFinancialWinnings += 100m;
                w.CurrentScore.WonPots.Add("Bingo: Erste vollstaendige Linie (100 EUR)");
                results.Add(new BingoPotResult { PotLabel = "Erste vollstaendige Linie", WinnerUserId = w.UserId, WinnerName = w.Name, Prize = 100m });
            }

            for (int i = 1; i < Math.Min(5, usersWithLines.Count); i++)
            {
                var w = usersWithLines[i].Player;
                w.CurrentScore.TotalFinancialWinnings += 50m;
                w.CurrentScore.WonPots.Add($"Bingo: Linie #{i + 1} (50 EUR)");
                results.Add(new BingoPotResult { PotLabel = $"Linie #{i + 1}", WinnerUserId = w.UserId, WinnerName = w.Name, Prize = 50m });
            }

            var bestBingo = users
                .OrderByDescending(u => _bingoEvaluator.CountFulfilledCells(u.BetData?.BingoCard ?? new BingoCard { Cells = [] }))
                .ThenBy(u => u.UserId)
                .FirstOrDefault();

            if (bestBingo != null)
            {
                bestBingo.CurrentScore.TotalFinancialWinnings += 100m;
                bestBingo.CurrentScore.WonPots.Add("Bingo: Bester Bingospieler - meiste Felder (100 EUR)");
                results.Add(new BingoPotResult { PotLabel = "Bester Bingospieler (meiste Felder)", WinnerUserId = bestBingo.UserId, WinnerName = bestBingo.Name, Prize = 100m });
            }

            return results;
        }

        public List<BingoPotResult> CalculateFinalTop10Winnings(List<User> users)
        {
            var results = new List<BingoPotResult>();
            for (int i = 0; i < Math.Min(users.Count, 20); i++)
            {
                decimal prize = i switch
                {
                    0    => 300m,
                    1    => 200m,
                    2    => 100m,
                    <= 9 =>  50m,
                    _    =>  15m
                };
                users[i].CurrentScore.TotalFinancialWinnings += prize;
                users[i].CurrentScore.WonPots.Add($"Gesamtwertung Platz {i + 1} ({prize:F0} EUR)");
                results.Add(new BingoPotResult { PotLabel = $"Gesamtwertung Platz {i + 1}", WinnerUserId = users[i].UserId, WinnerName = users[i].Name, Prize = prize });
            }
            return results;
        }

        public void DistributeFallbackBingoWins(List<User> users) { }

        private DateTime? GetEarliestLineCompletion(BingoCard card)
        {
            DateTime? earliest = null;
            foreach (var line in BingoLines)
            {
                var time = GetLineCompletionTime(card, line);
                if (time.HasValue && (!earliest.HasValue || time < earliest))
                    earliest = time;
            }
            return earliest;
        }

        private static DateTime? GetLineCompletionTime(BingoCard card, int[] line)
        {
            if (card?.Cells == null) return null;
            var cellMap = card.Cells.ToDictionary(c => c.Position);
            if (!line.All(pos => cellMap.TryGetValue(pos, out var c) && c.IsFulfilled))
                return null;
            return line.Select(pos => cellMap[pos].FulfilledAt).Max();
        }

        private static readonly int[][] BingoLines =
        [
            [0, 1, 2, 3, 4], [5, 6, 7, 8, 9], [10, 11, 12, 13, 14],
            [15, 16, 17, 18, 19], [20, 21, 22, 23, 24],
            [0, 5, 10, 15, 20], [1, 6, 11, 16, 21], [2, 7, 12, 17, 22],
            [3, 8, 13, 18, 23], [4, 9, 14, 19, 24],
            [0, 6, 12, 18, 24], [4, 8, 12, 16, 20]
        ];
    }
}
