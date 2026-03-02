using TippspielApp.Domain;
using TippspielApp.Finance;
using TippspielApp.Scoring;

namespace TippspielApp.Application
{
    /// <summary>
    /// Führt die komplette Ranglisten-Berechnung durch: erst Punkte, dann sortieren, dann Finanzen.
    /// Neue Evaluatoren können einfach zur Liste hinzugefügt werden ohne den Rest zu verändern.
    /// </summary>
    public class RankingCalculator
    {
        // Wichtig: ClassicEvaluator muss vor SpecialBetEvaluator laufen, weil der daraufaddiert
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
            // Alten Punktestand wegwerfen, frisch anfangen
            foreach (var user in users)
                user.CurrentScore = new ScoreSnapshot();

            // Jeden Evaluator auf jeden Tipper anwenden
            foreach (var evaluator in _evaluators)
                foreach (var user in users)
                    evaluator.Evaluate(user, data);

            // Gesamtpunkte = Classic + Knockout + Bingo
            foreach (var user in users)
                user.CurrentScore.TotalPoints =
                    user.CurrentScore.ClassicPoints +
                    user.CurrentScore.KnockoutPoints +
                    user.CurrentScore.BingoPoints;

            // Erst nach Punkten sortieren
            Sort(users);

            // Preisgeld verteilen – erst Cluster, dann Bingo, ggf. Haupttopf
            var clusterResults = _cluster.Calculate(users, data);
            var bingoResults   = _bingo.Calculate(users);
            var finalResults   = new List<BingoPotResult>();

            if (!string.IsNullOrEmpty(data.ActualWorldChampionTeamId))
                finalResults = _mainPot.Calculate(users);

            // Nochmal sortieren (Punkte haben sich nicht geändert, aber sauber so)
            Sort(users);

            // RankingEntry ohne Tipp-Rohdaten erzeugen
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

            // Welche Töpfe sind schon vergeben, welche stehen noch aus?
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

            // Für die Finanzübersicht: alle noch offenen Töpfe auflisten
            var unclaimed = potOverview
                .Where(e => !e.IsAwarded)             // nur noch nicht vergebene Töpfe
                .Select(e => $"{e.PotLabel} ({e.Prize:F0} EUR)")
                .ToList();

            // Wenn Turnier noch läuft, Haupttopf explizit als ausstehend markieren
            if (!isFinalized)
                unclaimed.Add($"Gesamtwertung Haupttopf – {PotMath.MainPot(users.Count):F0} EUR (erst nach Turnierende)");

            decimal total       = PotMath.TotalPot(users.Count);
            // ausgeschüttetes Preisgeld – über alle Tipper im Leaderboard aufsummieren
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
                // 1. Kriterium: wer hat mehr Gesamtpunkte?
                int cmp = b.CurrentScore.TotalPoints.CompareTo(a.CurrentScore.TotalPoints);
                if (cmp != 0) return cmp;
                // 2. Kriterium bei Gleichstand: wer hat mehr KO-Punkte?
                cmp = b.CurrentScore.KnockoutPoints.CompareTo(a.CurrentScore.KnockoutPoints);
                if (cmp != 0) return cmp;
                // 3. Kriterium: wer hat mehr Bingo-Felder erfüllt?
                cmp = b.CurrentScore.FulfilledBingoCells.CompareTo(a.CurrentScore.FulfilledBingoCells);
                if (cmp != 0) return cmp;
                // 4. Kriterium: alphabetische UserId – deterministisch, damit Tests nicht flúckern
                return string.Compare(a.UserId, b.UserId, StringComparison.Ordinal);
            });
        }
    }
}
