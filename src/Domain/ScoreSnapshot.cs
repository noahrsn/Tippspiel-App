namespace TippspielApp.Domain
{
    /// <summary>
    /// Speichert den aktuellen Punktestand eines Teilnehmers.
    /// RankingEntry erbt diese Klasse, damit man im Ranking nicht extra alles mappen muss.
    /// </summary>
    public class ScoreSnapshot
    {
        public int TotalPoints { get; set; }
        public int ClassicPoints { get; set; }
        public int KnockoutPoints { get; set; }
        public int BingoPoints { get; set; }
        public int FulfilledBingoCells { get; set; }
        public int CompletedBingoLines { get; set; }
        public decimal TotalFinancialWinnings { get; set; }
        public List<string> WonPots { get; set; } = [];
    }
}
