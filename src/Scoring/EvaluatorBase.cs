using TippspielApp.Domain;

namespace TippspielApp.Scoring
{
    /// <summary>
    /// Gemeinsame Basis für alle Tipp-Evaluatoren.
    /// CalculateMatchPoints() ist static, damit der ClusterDistributor sie ebenfalls nutzen kann.
    /// </summary>
    public abstract class EvaluatorBase : IEvaluator
    {
        public abstract void Evaluate(User user, TournamentData data);

        // Punkteverteilung: exakter Tipp = 4, richtige Differenz = 3, richtige Tendenz = 2, sonst 0
        public static int CalculateMatchPoints(MatchBet bet, MatchResult result)
        {
            // Toresdifferenz des Tipps (positiv = Heimsieg, 0 = Unentschieden, negativ = Auswärtssieg)
            int betDiff    = bet.HomeGoals    - bet.AwayGoals;
            // Tatsächliche Differenz aus dem Ergebnis
            int actualDiff = result.HomeGoals - result.AwayGoals;

            // Beide Tore stimmen exakt – volle 4 Punkte
            if (bet.HomeGoals == result.HomeGoals && bet.AwayGoals == result.AwayGoals) return 4;
            // Differenz stimmt – z.B. 2:0 getippt, 3:1 gespielt → beide Mal +2
            if (betDiff == actualDiff) return 3;
            // Gleiche Richtung (beide positiv oder beide negativ) → Tendenz richtig, aber Tore falsch
            if (Math.Sign(betDiff) == Math.Sign(actualDiff)) return 2;
            // Falsche Tendenz – z.B. Heimsieg getippt, aber Auswärtssieg
            return 0;
        }
    }
}
