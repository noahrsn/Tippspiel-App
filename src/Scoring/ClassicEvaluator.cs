using TippspielApp.Domain;

namespace TippspielApp.Scoring
{
    /// <summary>Berechnet Punkte für Gruppenspiel-Tipps und schreibt sie als ClassicPoints.</summary>
    public class ClassicEvaluator : EvaluatorBase
    {
        public override void Evaluate(User user, TournamentData data)
        {
            if (user.BetData?.GroupMatchBets == null) return;

            var finished = data.MatchResults
                .Where(m => m.IsFinished)
                .ToDictionary(m => m.MatchId, StringComparer.OrdinalIgnoreCase);

            int pts = 0;
            foreach (var bet in user.BetData.GroupMatchBets)
                if (finished.TryGetValue(bet.MatchId, out var result))
                    pts += CalculateMatchPoints(bet, result);

            user.CurrentScore.ClassicPoints = pts;
        }
    }
}
