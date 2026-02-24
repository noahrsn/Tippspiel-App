using TippspielApp.Models;

namespace TippspielApp.Services
{
    public class ClassicBetEvaluator
    {
        // Punktevergabe je Runde (Key = Rundenname in JSON)
        private static readonly Dictionary<string, int> KnockoutPoints = new(StringComparer.OrdinalIgnoreCase)
        {
            { "RoundOf32",    2 },
            { "RoundOf16",    4 },
            { "QuarterFinal", 6 },
            { "SemiFinal",    8 },
            { "Final",       10 }
        };

        /// <summary>
        /// Berechnet die Punkte für ein einzelnes Gruppenspiel.
        /// Exakt: 4 | Tordifferenz/Unentschieden: 3 | Tendenz: 2 | Falsch: 0
        /// </summary>
        public int CalculateMatchPoints(MatchBet bet, MatchResult result)
        {
            int betDiff    = bet.HomeGoals    - bet.AwayGoals;
            int actualDiff = result.HomeGoals - result.AwayGoals;

            // Exaktes Ergebnis
            if (bet.HomeGoals == result.HomeGoals && bet.AwayGoals == result.AwayGoals)
                return 4;

            // Richtige Tordifferenz (deckt damit auch jedes Unentschieden ab)
            if (betDiff == actualDiff)
                return 3;

            // Richtige Tendenz (Heimsieg / Auswärtssieg)
            if (Math.Sign(betDiff) == Math.Sign(actualDiff))
                return 2;

            return 0;
        }

        /// <summary>
        /// Gruppensieger-Punkte wurden aus dem Regelwerk entfernt – gibt immer 0 zurück.
        /// </summary>
        public int CalculateGroupWinnerPoints(Dictionary<string, string>? bets, Dictionary<string, string>? actuals)
            => 0;

        /// <summary>
        /// Berechnet KO-Phasen-Punkte. Pro korrekt platziertem Team gibt es Punkte gemäß Runde.
        /// Ein Team kann pro Runde nur einmal punkten.
        /// </summary>
        public int CalculateKnockoutPoints(Dictionary<string, List<string>>? bets, Dictionary<string, List<string>>? actuals)
        {
            if (bets == null || actuals == null) return 0;

            int total = 0;
            foreach (var (round, pointsPerTeam) in KnockoutPoints)
            {
                if (!bets.TryGetValue(round, out var bettedTeams)) continue;
                if (!actuals.TryGetValue(round, out var actualTeams)) continue;

                var actualSet = new HashSet<string>(actualTeams, StringComparer.OrdinalIgnoreCase);
                foreach (var team in bettedTeams.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (actualSet.Contains(team))
                        total += pointsPerTeam;
                }
            }
            return total;
        }

        /// <summary>
        /// Berechnet Punkte für Weltmeister (20 Pkt) und Torschützenkönig (20 Pkt).
        /// </summary>
        public int CalculateSpecialBetPoints(SpecialBet? bet, TournamentData data)
        {
            if (bet == null) return 0;
            int total = 0;

            if (!string.IsNullOrEmpty(data.ActualWorldChampionTeamId) &&
                string.Equals(bet.WorldChampionTeamId, data.ActualWorldChampionTeamId, StringComparison.OrdinalIgnoreCase))
                total += 20;

            if (!string.IsNullOrEmpty(data.ActualTopScorerName) &&
                string.Equals(bet.TopScorerName, data.ActualTopScorerName, StringComparison.OrdinalIgnoreCase))
                total += 20;

            return total;
        }

        /// <summary>
        /// Aktualisiert alle Punktefelder eines Users basierend auf den aktuellen Turnierdaten.
        /// </summary>
        public void UpdateClassicScores(User user, TournamentData data)
        {
            if (user.BetData == null) return;

            var finishedMatches = data.MatchResults
                .Where(m => m.IsFinished)
                .ToDictionary(m => m.MatchId, m => m, StringComparer.OrdinalIgnoreCase);

            int matchPoints = 0;
            if (user.BetData.GroupMatchBets != null)
            {
                foreach (var bet in user.BetData.GroupMatchBets)
                {
                    if (finishedMatches.TryGetValue(bet.MatchId, out var result))
                        matchPoints += CalculateMatchPoints(bet, result);
                }
            }

            int koPoints = CalculateKnockoutPoints(user.BetData.KnockoutBets, data.ActualKnockoutTeams);
            int specialPoints = CalculateSpecialBetPoints(user.BetData.SpecialBets, data);

            user.CurrentScore.ClassicPoints  = matchPoints + specialPoints;
            user.CurrentScore.KnockoutPoints = koPoints;
        }
    }
}