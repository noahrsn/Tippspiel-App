namespace TippspielApp.Domain
{
    /// <summary>
    /// Schlanker Ranglisten-Eintrag ohne Tipp-Rohdaten.
    /// Erbt alle Punktefelder direkt von ScoreSnapshot.
    /// </summary>
    public class RankingEntry : ScoreSnapshot
    {
        public int Rank { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}
