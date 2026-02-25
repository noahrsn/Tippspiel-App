using TippspielApp.Domain;

namespace TippspielApp.Scoring
{
    /// <summary>
    /// Wertet die Bingo-Karte eines Users aus und berechnet Bingo-Punkte.
    /// Erbt Linien-Definitionen und Zeitberechnungen von BingoBase.
    /// Implementiert IEvaluator: Evaluate() markiert Felder und schreibt den Punktestand.
    /// </summary>
    public class BingoEvaluator : BingoBase, IEvaluator
    {
        public void Evaluate(User user, TournamentData data)
        {
            var card = user.BetData?.BingoCard;
            if (card == null) return;

            MarkFulfilledCells(card, data.OccurredBingoEvents);

            user.CurrentScore.FulfilledBingoCells = CountFulfilledCells(card);
            user.CurrentScore.CompletedBingoLines = CountCompletedLines(card);
            user.CurrentScore.BingoPoints         = user.CurrentScore.FulfilledBingoCells * 3
                                                  + CalcLinePoints(user.CurrentScore.CompletedBingoLines);
        }

        private static void MarkFulfilledCells(BingoCard card, List<string> occurredEvents)
        {
            var eventSet = new HashSet<string>(occurredEvents, StringComparer.OrdinalIgnoreCase);
            var now = DateTime.UtcNow;

            foreach (var cell in card.Cells)
            {
                if (!cell.IsFulfilled && eventSet.Contains(cell.EventId))
                {
                    cell.IsFulfilled = true;
                    cell.FulfilledAt = now;
                }
            }
        }

        /// <summary>Zählt alle erfüllten Felder (kein FREE-Feld mehr im 4×4-Raster).</summary>
        private static int CountFulfilledCells(BingoCard card)
            => card.Cells.Count(c => c.IsFulfilled);

        /// <summary>Berechnet Linienpunkte: 1. Linie 10 Pkt, 2. Linie 6 Pkt, 3. Linie 4 Pkt, danach 0.</summary>
        private static int CalcLinePoints(int completedLines) => completedLines switch
        {
            0 => 0,
            1 => 10,
            2 => 16,
            _ => 20
        };

        private static int CountCompletedLines(BingoCard card)
        {
            var fulfilled = new HashSet<int>(card.Cells.Where(c => c.IsFulfilled).Select(c => c.Position));
            return Lines.Count(line => line.All(pos => fulfilled.Contains(pos)));
        }
    }
}
