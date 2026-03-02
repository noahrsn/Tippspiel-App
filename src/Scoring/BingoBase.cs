using TippspielApp.Domain;

namespace TippspielApp.Scoring
{
    /// <summary>
    /// Basis für den Bingo-Teil – enthält die Linien-Definition des 4×4-Rasters
    /// und Hilfsmethoden zur Zeitberechnung als static, damit der BingoDistributor
    /// sie ebenfalls nutzen kann (C# erlaubt kein Mehrfacherben).
    /// </summary>
    public abstract class BingoBase
    {
        // Alle 10 Linien: 4 Zeilen + 4 Spalten + 2 Diagonalen – Indizes beziehen sich auf Position im Raster
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

        // Gibt zurück wann die Linie abgeschlossen wurde – sprich den spätesten Zeitpunkt unter den 4 Feldern
        public static DateTime? GetLineCompletionTime(BingoCard card, int[] line)
        {
            if (card?.Cells == null) return null;
            // Dictionary für schnellen Zugriff nach Position – wird bei jede Linienkontrolle aufgebaut
            var cellMap = card.Cells.ToDictionary(c => c.Position);
            // Prüfen ob alle 4 Positionen dieser Linie erfüllt sind – wenn ein Feld fehlt sofort null
            if (!line.All(pos => cellMap.TryGetValue(pos, out var c) && c.IsFulfilled))
                return null;
            // Den spätesten Erfüllungszeitpunkt nehmen – das ist der Moment, an dem die Linie wirklich stand
            return line.Select(pos => cellMap[pos].FulfilledAt).Max();
        }

        // Wann wurde die erste Linie auf der Karte abgeschlossen? Gebraucht für den "Erste Linie"-Preis
        public static DateTime? GetEarliestLineCompletion(BingoCard? card)
        {
            if (card == null) return null;
            DateTime? earliest = null;
            // Alle 10 Linien durchgehen und das früheste Abschlussdatum finden
            foreach (var line in Lines)
            {
                var time = GetLineCompletionTime(card, line);
                // Nur aktualisieren wenn diese Linie abgeschlossen ist und früher war als die bisherige
                if (time.HasValue && (!earliest.HasValue || time < earliest))
                    earliest = time;
            }
            return earliest;
        }

        // Full House = alle 16 Felder erfüllt – gibt den Zeitpunkt des zuletzt erfüllten Feldes zurück
        public static DateTime? GetFullHouseCompletionTime(BingoCard? card)
        {
            // Karte fehlt oder hat weniger als 16 Felder – kann kein Full House sein
            if (card?.Cells == null || card.Cells.Count < 16) return null;
            // Mindestens ein Feld ist noch nicht erfüllt – kein Full House
            if (!card.Cells.All(c => c.IsFulfilled)) return null;
            // Der späteste Zeitpunkt aller Felder ist der Moment des Full House
            return card.Cells.Select(c => c.FulfilledAt).Max();
        }
    }
}
