using TippspielApp.Domain;
using TippspielApp.Finance;
using TippspielApp.Scoring;

namespace TippspielApp.Application
{
    /// <summary>
    /// Orchestriert die vollständige Auswertung: Punkte → Sortierung → Finanzen → Report.
    /// Iteriert die IEvaluator-Liste, damit neue Bewertungsregeln einfach ergänzt werden können.
    /// </summary>
    public class RankingCalculator
    {
        // Reihenfolge ist wichtig: ClassicEvaluator setzt ClassicPoints,
        // SpecialBetEvaluator addiert dann darauf.
        private readonly List<IEvaluator> _evaluators =
        [
            new ClassicEvaluator(),
            new KnockoutEvaluator(),
            new SpecialBetEvaluator(),
            new BingoEvaluator()
        ];

        private readonly ClusterDistributor  _cluster  = new();
        private readonly BingoDistributor    _bingo    = new();
        private readonly MainPotDistributor  _mainPot  = new();

        public RankingReport Run(List<User> users, TournamentData data)
        {
            // 1. Punktestand zurücksetzen
            foreach (var user in users)
                user.CurrentScore = new ScoreSnapshot();

            // 2. Punkte berechnen (alle Kategorien)
            foreach (var evaluator in _evaluators)
                foreach (var user in users)
                    evaluator.Evaluate(user, data);

            // 3. Gesamtpunkte aggregieren
            foreach (var user in users)
                user.CurrentScore.TotalPoints =
                    user.CurrentScore.ClassicPoints +
                    user.CurrentScore.KnockoutPoints +
                    user.CurrentScore.BingoPoints;

            // 4. Sortieren
            Sort(users);

            // 5. Finanzen berechnen
            var clusterResults = _cluster.Calculate(users, data);
            var bingoResults   = _bingo.Calculate(users);
            var finalResults   = new List<BingoPotResult>();

            if (!string.IsNullOrEmpty(data.ActualWorldChampionTeamId))
                finalResults = _mainPot.Calculate(users);

            // 6. Nach Preisgeld-Update nochmals sortieren (Punkte unverändert)
            Sort(users);

            // 7. Rangliste aufbauen (RankingEntry erbt alle Felder von ScoreSnapshot)
            var leaderboard = users.Select((u, i) => new RankingEntry
            {
                Rank                   = i + 1,
                UserId                 = u.UserId,
                Name                   = u.Name,
                TotalPoints            = u.CurrentScore.TotalPoints,
                ClassicPoints          = u.CurrentScore.ClassicPoints,
                KnockoutPoints         = u.CurrentScore.KnockoutPoints,
                BingoPoints            = u.CurrentScore.BingoPoints,
                FulfilledBingoCells    = u.CurrentScore.FulfilledBingoCells,
                CompletedBingoLines    = u.CurrentScore.CompletedBingoLines,
                TotalFinancialWinnings = u.CurrentScore.TotalFinancialWinnings,
                WonPots                = u.CurrentScore.WonPots
            }).ToList();

            // 8. Topf-Übersicht (vergeben / offen)
            bool isFinalized = !string.IsNullOrEmpty(data.ActualWorldChampionTeamId);
            var  allAwarded  = bingoResults.Concat(finalResults).ToList();
            var  prizeSlots  = BingoDistributor.GetPrizeSlots(users.Count);

            var potOverview = prizeSlots.Select(slot =>
            {
                var won = allAwarded.FirstOrDefault(r =>
                    string.Equals(r.PotLabel, slot.Label, StringComparison.OrdinalIgnoreCase));
                return new PotOverviewEntry
                {
                    PotLabel     = slot.Label,
                    Prize        = slot.Amount,
                    IsAwarded    = won != null,
                    WinnerUserId = won?.WinnerUserId ?? string.Empty,
                    WinnerName   = won?.WinnerName   ?? string.Empty
                };
            }).ToList();

            // 9. Nicht vergebene Töpfe für FinanceSummary
            var unclaimed = potOverview
                .Where(e => !e.IsAwarded)
                .Select(e => $"{e.PotLabel} ({e.Prize:F0} EUR)")
                .ToList();

            if (!isFinalized)
                unclaimed.Add($"Gesamtwertung Haupttopf – {PotMath.MainPot(users.Count):F0} EUR (erst nach Turnierende)");

            decimal total       = PotMath.TotalPot(users.Count);
            decimal distributed = leaderboard.Sum(e => e.TotalFinancialWinnings);

            return new RankingReport
            {
                GeneratedAt         = DateTime.UtcNow,
                IsMainPotFinalized  = isFinalized,
                Leaderboard         = leaderboard,
                GroupClusterResults = clusterResults,
                BingoPotResults     = [..bingoResults, ..finalResults],
                BingoPotOverview    = potOverview,
                FinanceSummary      = new FinanceSummary
                {
                    TotalPot          = total,
                    DistributedAmount = distributed,
                    RemainingAmount   = total - distributed,
                    UnclaimedPots     = unclaimed
                }
            };
        }

        private static void Sort(List<User> users)
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
