namespace TippspielApp.Domain
{
    /// <summary>
    /// Die 4×4-Bingo-Karte eines Users. Kein FREE-Feld – alle 16 Felder sind Ereignisfelder.
    /// </summary>
    public class BingoCard
    {
        /// <summary>16 Felder (Positionen 0–15, zeilenweise von links oben).</summary>
        public List<BingoCell> Cells { get; set; } = [];
    }

    /// <summary>Ein einzelnes Feld auf der Bingo-Karte.</summary>
    public class BingoCell
    {
        /// <summary>Position im 4×4-Raster (0–15).</summary>
        public int Position { get; set; }
        /// <summary>ID des Ereignisses aus dem Bingo-Katalog.</summary>
        public string EventId { get; set; } = string.Empty;
        public bool IsFulfilled { get; set; }
        /// <summary>Zeitpunkt, zu dem das Ereignis eingetreten ist (UTC).</summary>
        public DateTime? FulfilledAt { get; set; }
    }
}
