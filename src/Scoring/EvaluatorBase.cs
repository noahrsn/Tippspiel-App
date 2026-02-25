using TippspielApp.Domain;

namespace TippspielApp.Scoring
{
    /// <summary>
    /// Abstrakte Basis für alle klassischen Tipp-Evaluatoren.
    /// Stellt CalculateMatchPoints() als gemeinsame, statische Hilfsmethode bereit,
    /// damit auch der ClusterDistributor sie aufrufen kann.
    /// </summary>
    public abstract class EvaluatorBase : IEvaluator
    {
        public abstract void Evaluate(User user, TournamentData data);

        /// <summary>Exakt: 4 Pkt | Tordifferenz/Unentschieden: 3 Pkt | Tendenz: 2 Pkt | Falsch: 0 Pkt.</summary>
        public static int CalculateMatchPoints(MatchBet bet, MatchResult result)
        {
            int betDiff    = bet.HomeGoals    - bet.AwayGoals;
            int actualDiff = result.HomeGoals - result.AwayGoals;

            if (bet.HomeGoals == result.HomeGoals && bet.AwayGoals == result.AwayGoals) return 4;
            if (betDiff == actualDiff) return 3;
            if (Math.Sign(betDiff) == Math.Sign(actualDiff)) return 2;
            return 0;
        }
    }
}
