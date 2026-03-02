namespace TippspielApp.Domain
{
    /// <summary>
    /// Was im Ranking nach außen sichtbar ist – Platz, Name und alle Punktwerte, aber keine Tipp-Rohdaten.
    /// Erbt den Punktestand direkt von ScoreSnapshot.
    /// </summary>
    public class RankingEntry : ScoreSnapshot
    {
        public int Rank { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
