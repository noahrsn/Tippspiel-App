namespace TippspielApp.Models
{
    /// <summary>
    /// Repräsentiert die 5×5-Bingo-Karte eines Users.
    /// Position 12 (Mitte) ist das FREE-Feld und immer erfüllt.
    /// </summary>
    public class BingoCard
    {
        /// <summary>25 Felder (Positionen 0–24, zeilenweise von links oben).</summary>
        public List<BingoCell> Cells { get; set; } = [];
    }

    /// <summary>Ein einzelnes Feld auf der Bingo-Karte.</summary>
    public class BingoCell
    {
        /// <summary>Position im 5×5-Raster (0–24, zeilenweise). Mitte = 12 (FREE).</summary>
        public int Position { get; set; }
        /// <summary>ID des Ereignisses aus dem Bingo-Katalog (z. B. "EVT_RED_CARD_GRP_A"). FREE-Feld = "FREE_SPACE".</summary>
        public string EventId { get; set; } = string.Empty;
        /// <summary>Gibt an, ob das Ereignis im Turnier eingetreten ist.</summary>
        public bool IsFulfilled { get; set; }
        /// <summary>Zeitpunkt, zu dem das Ereignis eingetreten ist (UTC). Null = noch nicht erfüllt.</summary>
        public DateTime? FulfilledAt { get; set; }
    }
}