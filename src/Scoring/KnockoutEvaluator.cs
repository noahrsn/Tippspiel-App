using TippspielApp.Domain;

namespace TippspielApp.Scoring
{
    /// <summary>Punkte für KO-Tipps – je weiter die Runde, desto mehr Punkte pro richtig getippter Mannschaft.</summary>
    public class KnockoutEvaluator : EvaluatorBase
    {
        private static readonly Dictionary<string, int> PointsPerRound = new(StringComparer.OrdinalIgnoreCase)
        {
            { "RoundOf32",    2 },
            { "RoundOf16",    4 },
            { "QuarterFinal", 6 },
            { "SemiFinal",    8 },
            { "Final",       10 }
        };

        public override void Evaluate(User user, TournamentData data)
        {
            user.CurrentScore.KnockoutPoints = Calculate(
                user.BetData?.KnockoutBets, data.ActualKnockoutTeams);
        }

        private static int Calculate(
            Dictionary<string, List<string>>? bets,
            Dictionary<string, List<string>>? actuals)
        {
            // Wenn einer der beiden Teile fehlt, gibt es nichts zu berechnen
            if (bets == null || actuals == null) return 0;

            int total = 0;
            // PointsPerRound enthält alle 5 Runden – wir gehen sie der Reihe nach durch
            foreach (var (round, pointsPerTeam) in PointsPerRound)
            {
                // Wenn der User für diese Runde keinen Tipp hat, Runde überspringen
                if (!bets.TryGetValue(round, out var bettedTeams)) continue;
                // Wenn die Runde noch nicht gespielt wurde, gibt es auch nichts zu vergleichen
                if (!actuals.TryGetValue(round, out var actualTeams)) continue;

                // HashSet für schnellen Enthält-Check – sonst wäre es O(n*m) statt O(n)
                var actualSet = new HashSet<string>(actualTeams, StringComparer.OrdinalIgnoreCase);
                // Distinct verhindert, dass ein Team doppelt in der Tipp-Liste landet und doppelt punktet
                foreach (var team in bettedTeams.Distinct(StringComparer.OrdinalIgnoreCase))
                    if (actualSet.Contains(team))
                        total += pointsPerTeam;  // Punkte abhängig von der Runde (2 bis 10)
            }
            return total;
        }
    }
}
