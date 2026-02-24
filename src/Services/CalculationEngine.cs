using TippspielApp.Models;

namespace TippspielApp.Services
{
    public class CalculationEngine
    {
        private readonly ClassicBetEvaluator _classicEvaluator = new();
        private readonly BingoEvaluator      _bingoEvaluator   = new();
        private readonly FinanceCalculator   _financeCalc      = new();

        public RankingReport RunDailyCalculation(List<User> users, TournamentData currentData)
        {
            // 1. Scores zuruecksetzen
            foreach (var user in users)
                user.CurrentScore = new ScoreBoard();

            // 2. Klassische Tipps
            foreach (var user in users)
                _classicEvaluator.UpdateClassicScores(user, currentData);

            // 3. Bingo
            _bingoEvaluator.UpdateBingoCards(users, currentData.OccurredBingoEvents);
            foreach (var user in users)
            {
                var card = user.BetData?.BingoCard;
                if (card == null) continue;
                user.CurrentScore.BingoPoints       = _bingoEvaluator.CalculateBingoPoints(card);
                user.CurrentScore.FulfilledBingoCells = _bingoEvaluator.CountFulfilledCells(card);
                user.CurrentScore.CompletedBingoLines = _bingoEvaluator.CountCompletedLines(card);
            }

            // 4. Gesamtpunkte
            foreach (var user in users)
                user.CurrentScore.TotalPoints =
                    user.CurrentScore.ClassicPoints +
                    user.CurrentScore.KnockoutPoints +
                    user.CurrentScore.BingoPoints;

            // 5. Sortieren
            SortLeaderboardWithTieBreaker(users);

            // 6. Finanzen berechnen – Ergebnislisten einsammeln
            var clusterResults = _financeCalc.CalculateGroupClusterWinnings(users, currentData);
            var bingoResults   = _financeCalc.CalculateBingoFinancialWins(users);
            var finalResults   = new List<BingoPotResult>();

            if (!string.IsNullOrEmpty(currentData.ActualWorldChampionTeamId))
                finalResults = _financeCalc.CalculateFinalTop10Winnings(users);

            // 7. Users nach Preisgeld-Update nochmals sortieren (Punkte unveraendert)
            SortLeaderboardWithTieBreaker(users);

            // 8. Schlanke RankingEntry-Liste bauen (keine Tipp-Rohdaten)
            var leaderboard = users.Select((u, i) => new RankingEntry
            {
                Rank                 = i + 1,
                UserId               = u.UserId,
                Name                 = u.Name,
                TotalPoints          = u.CurrentScore.TotalPoints,
                ClassicPoints        = u.CurrentScore.ClassicPoints,
                KnockoutPoints       = u.CurrentScore.KnockoutPoints,
                BingoPoints          = u.CurrentScore.BingoPoints,
                FulfilledBingoCells  = u.CurrentScore.FulfilledBingoCells,
                CompletedBingoLines  = u.CurrentScore.CompletedBingoLines,
                TotalFinancialWinnings = u.CurrentScore.TotalFinancialWinnings,
                WonPots              = u.CurrentScore.WonPots
            }).ToList();

            decimal distributed = leaderboard.Sum(e => e.TotalFinancialWinnings);

            return new RankingReport
            {
                GeneratedAt          = DateTime.UtcNow,
                Leaderboard          = leaderboard,
                GroupClusterResults  = clusterResults,
                BingoPotResults      = [..bingoResults, ..finalResults],
                FinanceSummary       = new FinanceSummary
                {
                    TotalPot          = 1800m,
                    DistributedAmount = distributed,
                    RemainingAmount   = 1800m - distributed,
                    UnclaimedPots     = []
                }
            };
        }

        private void SortLeaderboardWithTieBreaker(List<User> users)
        {
            users.Sort((a, b) =>
            {
                int cmp = b.CurrentScore.TotalPoints.CompareTo(a.CurrentScore.TotalPoints);
                if (cmp != 0) return cmp;
                cmp = b.CurrentScore.KnockoutPoints.CompareTo(a.CurrentScore.KnockoutPoints);
                if (cmp != 0) return cmp;
                cmp = b.CurrentScore.FulfilledBingoCells.CompareTo(a.CurrentScore.FulfilledBingoCells);
                if (cmp != 0) return cmp;
                return string.Compare(a.UserId, b.UserId, StringComparison.Ordinal);
            });
        }
    }
}
