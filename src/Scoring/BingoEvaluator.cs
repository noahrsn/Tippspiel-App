using TippspielApp.Domain;

namespace TippspielApp.Scoring
{
    /// <summary>
    /// Berechnet die Bingo-Punkte eines Users: zuerst Felder markieren,
    /// dann Felder und Linien zählen und daraus die Punkte zusammenrechnen.
    /// Linienpunkte sind abgestuft: 1. Linie = 10 Pkt, 2. = 6 Pkt, 3. = 4 Pkt, danach 0.
    /// </summary>
    public class BingoEvaluator : EvaluatorBase
    {
        public override void Evaluate(User user, TournamentData data)
        {
            var card = user.BetData?.BingoCard;
            // Kein Bingo-Karte getippt – 0 Punkte, nichts weiter tun
            if (card == null) return;

            // Zuerst Felder als erfüllt markieren, die durch eingetretene Ereignisse abgedeckt sind
            MarkFulfilledCells(card, data.OccurredBingoEvents);

            // Einzelne Felder zählen – jedes gibt 3 Punkte
            user.CurrentScore.FulfilledBingoCells = CountFulfilledCells(card);
            // Komplette Linien zählen – bringen Bonuspunkte
            user.CurrentScore.CompletedBingoLines = CountCompletedLines(card);
            // Gesamtpunkte: 3 pro Feld + abgestufte Linienpunkte
            user.CurrentScore.BingoPoints         = user.CurrentScore.FulfilledBingoCells * 3
                                                  + CalcLinePoints(user.CurrentScore.CompletedBingoLines);
        }

        private static void MarkFulfilledCells(BingoCard card, List<string> occurredEvents)
        {
            // HashSet für schnellen Lookup – bei ~200 Spielern wird das oft aufgerufen
            var eventSet = new HashSet<string>(occurredEvents, StringComparer.OrdinalIgnoreCase);
            var now = DateTime.UtcNow;

            foreach (var cell in card.Cells)
            {
                // Nur Felder anpacken die noch nicht erfüllt sind – sonst würde FulfilledAt überschrieben
                if (!cell.IsFulfilled && eventSet.Contains(cell.EventId))
                {
                    cell.IsFulfilled = true;
                    cell.FulfilledAt = now;  // Zeitstempel für spätere Zeitvergleiche im Distributor
                }
            }
        }

        // Kein Freifeld mehr – alle 16 müssen aktiv durch WM-Ereignisse abgedeckt werden
        private static int CountFulfilledCells(BingoCard card)
            => card.Cells.Count(c => c.IsFulfilled);

        // Abstufung nach Regelwerk: erste Linie 10 Pkt, zweite 6, dritte 4, alles danach nichts mehr
        // Die Werte im switch sind kumulativ – bei 2 Linien also 10+6=16 insgesamt
        private static int CalcLinePoints(int completedLines) => completedLines switch
        {
            0 => 0,
            1 => 10,
            2 => 16,  // 10 + 6
            _ => 20   // 10 + 6 + 4, ab der 4. Linie kommen keine weiteren hinzu
        };

        private static int CountCompletedLines(BingoCard card)
        {
            // Positionen aller erfüllten Felder als HashSet – Linienkontrolle dann per All()
            var fulfilled = new HashSet<int>(card.Cells.Where(c => c.IsFulfilled).Select(c => c.Position));
            // Eine Linie gilt als abgeschlossen, wenn alle 4 ihrer Positionen im Set enthalten sind
            return BingoBase.Lines.Count(line => line.All(pos => fulfilled.Contains(pos)));
        }
    }
}
