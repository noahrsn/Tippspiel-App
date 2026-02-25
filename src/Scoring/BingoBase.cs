using TippspielApp.Domain;

namespace TippspielApp.Scoring
{
    /// <summary>
    /// Abstrakte Basisklasse für alle Bingo-bezogenen Services.
    /// Stellt das 4×4-Linien-Raster und zeitbasierte Hilfsmethoden als public static bereit,
    /// sodass BingoEvaluator (erbt davon) und BingoDistributor (erbt nicht davon) beide Zugriff haben.
    /// </summary>
    public abstract class BingoBase
    {
        /// <summary>Alle 10 möglichen Linien im 4×4-Raster (4 Zeilen, 4 Spalten, 2 Diagonalen).</summary>
        public static readonly int[][] Lines =
        [
            [0,  1,  2,  3 ],   // Zeile 1
            [4,  5,  6,  7 ],   // Zeile 2
            [8,  9,  10, 11],   // Zeile 3
            [12, 13, 14, 15],   // Zeile 4
            [0,  4,  8,  12],   // Spalte 1
            [1,  5,  9,  13],   // Spalte 2
            [2,  6,  10, 14],   // Spalte 3
            [3,  7,  11, 15],   // Spalte 4
            [0,  5,  10, 15],   // Diagonale \
            [3,  6,  9,  12]    // Diagonale /
        ];

        /// <summary>Zeitpunkt der letzten Linie (zuletzt erfülltes Feld). Null wenn Linie unvollständig.</summary>
        public static DateTime? GetLineCompletionTime(BingoCard card, int[] line)
        {
            if (card?.Cells == null) return null;
            var cellMap = card.Cells.ToDictionary(c => c.Position);
            if (!line.All(pos => cellMap.TryGetValue(pos, out var c) && c.IsFulfilled))
                return null;
            return line.Select(pos => cellMap[pos].FulfilledAt).Max();
        }

        /// <summary>Frühester Zeitpunkt irgendeiner vollständigen Linie auf der Karte.</summary>
        public static DateTime? GetEarliestLineCompletion(BingoCard? card)
        {
            if (card == null) return null;
            DateTime? earliest = null;
            foreach (var line in Lines)
            {
                var time = GetLineCompletionTime(card, line);
                if (time.HasValue && (!earliest.HasValue || time < earliest))
                    earliest = time;
            }
            return earliest;
        }

        /// <summary>Zeitpunkt des Full House (letztes erfülltes Feld). Null wenn nicht alle 16 erfüllt.</summary>
        public static DateTime? GetFullHouseCompletionTime(BingoCard? card)
        {
            if (card?.Cells == null || card.Cells.Count < 16) return null;
            if (!card.Cells.All(c => c.IsFulfilled)) return null;
            return card.Cells.Select(c => c.FulfilledAt).Max();
        }
    }
}
