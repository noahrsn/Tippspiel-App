using TippspielApp.Domain;

namespace TippspielApp.Scoring
{
    /// <summary>Prüft Weltmeister- und Torschützentipp – jeder richtige Tipp bringt 20 Punkte.</summary>
    public class SpecialBetEvaluator : EvaluatorBase
    {
        public override void Evaluate(User user, TournamentData data)
        {
            // Werden zu den ClassicPoints addiert, weil Sondertipps inhaltlich zur Gruppe gehören
            user.CurrentScore.ClassicPoints += Calculate(user.BetData?.SpecialBets, data);
        }

        private static int Calculate(SpecialBet? bet, TournamentData data)
        {
            // Kein Sondertipp vorhanden – nichts zu prüfen
            if (bet == null) return 0;
            int pts = 0;

            // Weltmeister nur prüfen wenn das Turnier entschieden ist (Feld ist dann gesetzt)
            // OrdinalIgnoreCase damit z.B. "germany" und "Germany" beide akzeptiert werden
            if (!string.IsNullOrEmpty(data.ActualWorldChampionTeamId) &&
                string.Equals(bet.WorldChampionTeamId, data.ActualWorldChampionTeamId, StringComparison.OrdinalIgnoreCase))
                pts += 20;  // richtiger Weltmeister-Tipp = 20 Punkte

            // Torschützenkönig analog – erst prüfen wenn der Sieger feststeht
            if (!string.IsNullOrEmpty(data.ActualTopScorerName) &&
                string.Equals(bet.TopScorerName, data.ActualTopScorerName, StringComparison.OrdinalIgnoreCase))
                pts += 20;  // richtiger Torschütze-Tipp = 20 Punkte

            return pts;
        }
    }
}
