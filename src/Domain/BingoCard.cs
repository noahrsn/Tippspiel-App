namespace TippspielApp.Domain
{
    /// <summary>
    /// Bingo-Karte mit 4×4 Feldern. Kein Freifeld – alle 16 müssen durch echte WM-Ereignisse abgedeckt werden.
    /// </summary>
    public class BingoCard
    {
        // Positionen 0–15 von links oben, zeilenweise
        public List<BingoCell> Cells { get; set; } = [];
    }

    /// <summary>Ein einzelnes Bingo-Feld – Position, zugehöriges Ereignis und ob es schon eingetreten ist.</summary>
    public class BingoCell
    {
        // 0 = oben links, 15 = unten rechts
        public int Position { get; set; }
        // Verweist auf ein Ereignis im BingoEventCatalog der TournamentData
        public string EventId { get; set; } = string.Empty;
        public bool IsFulfilled { get; set; }
        // Wann das Feld erfüllt wurde – null bedeutet, es ist noch offen
        public DateTime? FulfilledAt { get; set; }
    }
}
