using TippspielApp.Domain;

namespace TippspielApp.Scoring
{
    /// <summary>Wertet alle abgeschlossenen Gruppenspiele aus und trägt die Punkte als ClassicPoints ein.</summary>
    public class ClassicEvaluator : EvaluatorBase
    {
        public override void Evaluate(User user, TournamentData data)
        {
            // Kein Absturz wenn der User noch keine Tipps eingetragen hat
            if (user.BetData?.GroupMatchBets == null) return;

            // Dictionary für O(1)-Lookup statt linearer Suche – MatchId als Key
            // OrdinalIgnoreCase damit Groß-/Kleinschreibung im JSON egal ist
            var finished = data.MatchResults
                .Where(m => m.IsFinished)         // nur abgeschlossene Spiele bewerten
                .ToDictionary(m => m.MatchId, StringComparer.OrdinalIgnoreCase);

            int pts = 0;
            foreach (var bet in user.BetData.GroupMatchBets)
            {
                // TryGetValue gibt false zurück wenn das Spiel noch nicht gespielt wurde → überspringen
                if (finished.TryGetValue(bet.MatchId, out var result))
                    pts += CalculateMatchPoints(bet, result);
            }

            user.CurrentScore.ClassicPoints = pts;
        }
    }
}
