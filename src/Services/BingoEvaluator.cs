using TippspielApp.Models;

namespace TippspielApp.Services
{
    public class BingoEvaluator
    {
        // Alle 12 möglichen Linien (5x5-Feld): 5 Zeilen, 5 Spalten, 2 Diagonalen
        private static readonly int[][] Lines =
        [
            [0, 1, 2, 3, 4],         // Zeile 1
            [5, 6, 7, 8, 9],         // Zeile 2
            [10, 11, 12, 13, 14],    // Zeile 3 (enthält FREE-Feld)
            [15, 16, 17, 18, 19],    // Zeile 4
            [20, 21, 22, 23, 24],    // Zeile 5
            [0, 5, 10, 15, 20],      // Spalte 1
            [1, 6, 11, 16, 21],      // Spalte 2
            [2, 7, 12, 17, 22],      // Spalte 3
            [3, 8, 13, 18, 23],      // Spalte 4
            [4, 9, 14, 19, 24],      // Spalte 5
            [0, 6, 12, 18, 24],      // Diagonale \
            [4, 8, 12, 16, 20]       // Diagonale /
        ];

        /// <summary>Aktualisiert alle Bingo-Karten der User anhand der eingetretenen Ereignisse.</summary>
        public void UpdateBingoCards(List<User> users, List<string> occurredEvents)
        {
            var eventSet = new HashSet<string>(occurredEvents, StringComparer.OrdinalIgnoreCase);
            var now = DateTime.UtcNow;

            foreach (var user in users)
            {
                if (user.BetData?.BingoCard?.Cells == null) continue;

                foreach (var cell in user.BetData.BingoCard.Cells)
                {
                    // FREE-Feld ist immer erfüllt
                    if (cell.Position == 12)
                    {
                        cell.IsFulfilled = true;
                        cell.FulfilledAt ??= DateTime.Parse("2026-06-01T00:00:00Z").ToUniversalTime();
                        continue;
                    }

                    if (!cell.IsFulfilled && eventSet.Contains(cell.EventId))
                    {
                        cell.IsFulfilled = true;
                        cell.FulfilledAt = now;
                    }
                }
            }
        }

        /// <summary>Berechnet die Bingo-Punkte für die Gesamtwertung: 2 Pkt/Feld, 8 Pkt/Linie.</summary>
        public int CalculateBingoPoints(BingoCard card)
        {
            int cellPoints = CountFulfilledCells(card) * 2;
            int linePoints = CountCompletedLines(card) * 8;
            return cellPoints + linePoints;
        }

        public int CountFulfilledCells(BingoCard card)
        {
            if (card?.Cells == null) return 0;
            return card.Cells.Count(c => c.IsFulfilled);
        }

        public int CountCompletedLines(BingoCard card)
        {
            if (card?.Cells == null) return 0;

            var fulfilled = new HashSet<int>(card.Cells
                .Where(c => c.IsFulfilled)
                .Select(c => c.Position));

            return Lines.Count(line => line.All(pos => fulfilled.Contains(pos)));
        }

        /// <summary>Gibt die Abschlusszeit einer Linie zurück (Zeitpunkt des letzten erfüllten Feldes).</summary>
        public DateTime? GetLineCompletionTime(BingoCard card, int[] line)
        {
            if (card?.Cells == null) return null;

            var cellMap = card.Cells.ToDictionary(c => c.Position);
            if (!line.All(pos => cellMap.TryGetValue(pos, out var c) && c.IsFulfilled))
                return null;

            return line
                .Select(pos => cellMap[pos].FulfilledAt)
                .Max();
        }
    }
}