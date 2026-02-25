using TippspielApp.Domain;

namespace TippspielApp.Scoring
{
    /// <summary>Berechnet Sondertipp-Punkte: Weltmeister (20 Pkt) und Torschützenkönig (20 Pkt).</summary>
    public class SpecialBetEvaluator : EvaluatorBase
    {
        public override void Evaluate(User user, TournamentData data)
        {
            // Addiert auf ClassicPoints, die zuvor von ClassicEvaluator gesetzt wurden.
            user.CurrentScore.ClassicPoints += Calculate(user.BetData?.SpecialBets, data);
        }

        private static int Calculate(SpecialBet? bet, TournamentData data)
        {
            if (bet == null) return 0;
            int pts = 0;

            if (!string.IsNullOrEmpty(data.ActualWorldChampionTeamId) &&
                string.Equals(bet.WorldChampionTeamId, data.ActualWorldChampionTeamId, StringComparison.OrdinalIgnoreCase))
                pts += 20;

            if (!string.IsNullOrEmpty(data.ActualTopScorerName) &&
                string.Equals(bet.TopScorerName, data.ActualTopScorerName, StringComparison.OrdinalIgnoreCase))
                pts += 20;

            return pts;
        }
    }
}
