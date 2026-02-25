using TippspielApp.Domain;

namespace TippspielApp.Scoring
{
    /// <summary>Berechnet KO-Runden-Punkte. Pro korrekt getipptem Team Punkte gemäß Runde.</summary>
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
            if (bets == null || actuals == null) return 0;

            int total = 0;
            foreach (var (round, pointsPerTeam) in PointsPerRound)
            {
                if (!bets.TryGetValue(round, out var bettedTeams)) continue;
                if (!actuals.TryGetValue(round, out var actualTeams)) continue;

                var actualSet = new HashSet<string>(actualTeams, StringComparer.OrdinalIgnoreCase);
                foreach (var team in bettedTeams.Distinct(StringComparer.OrdinalIgnoreCase))
                    if (actualSet.Contains(team))
                        total += pointsPerTeam;
            }
            return total;
        }
    }
}
