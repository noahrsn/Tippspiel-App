namespace TippspielApp.Domain
{
    /// <summary>
    /// Basisklasse für den Punktestand eines Users.
    /// Wird von RankingEntry geerbt, sodass alle Felder direkt im Ranglisten-Eintrag verfügbar sind.
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
