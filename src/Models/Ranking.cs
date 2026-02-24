namespace TippspielApp.Models
{
    /// <summary>Enthaelt den aktuellen Punktestand und die gewonnenen Preisgelder eines Users (interner Zwischenspeicher).</summary>
    public class ScoreBoard
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

    /// <summary>Schlanker Ranglisten-Eintrag – enthaelt KEINE Tipp-Rohdaten.</summary>
    public class RankingEntry
    {
        public int Rank { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int TotalPoints { get; set; }
        public int ClassicPoints { get; set; }
        public int KnockoutPoints { get; set; }
        public int BingoPoints { get; set; }
        public int FulfilledBingoCells { get; set; }
        public int CompletedBingoLines { get; set; }
        public decimal TotalFinancialWinnings { get; set; }
        public List<string> WonPots { get; set; } = [];
    }

    /// <summary>Ergebnis eines Gruppen-Cluster-Zwischengewinns (zwei Gruppen, 50 EUR).</summary>
    public class GroupClusterResult
    {
        public string ClusterLabel { get; set; } = string.Empty;   // "A+B"
        public string WinnerUserId { get; set; } = string.Empty;
        public string WinnerName { get; set; } = string.Empty;
        public int WinnerClusterPoints { get; set; }
        public decimal Prize { get; set; }
        public bool IsShared { get; set; }
        public List<string> CoWinners { get; set; } = [];
    }

    /// <summary>Ergebnis eines Bingo-Geldpreises.</summary>
    public class BingoPotResult
    {
        public string PotLabel { get; set; } = string.Empty;   // "Erste Linie", "Bester Bingospieler" ...
        public string WinnerUserId { get; set; } = string.Empty;
        public string WinnerName { get; set; } = string.Empty;
        public decimal Prize { get; set; }
    }

    /// <summary>Vollstaendiger Auswertungsbericht ohne Tipp-Rohdaten.</summary>
    public class RankingReport
    {
        public DateTime GeneratedAt { get; set; }
        public List<RankingEntry> Leaderboard { get; set; } = [];
        public List<GroupClusterResult> GroupClusterResults { get; set; } = [];
        public List<BingoPotResult> BingoPotResults { get; set; } = [];
        public FinanceSummary FinanceSummary { get; set; } = new();
    }

    /// <summary>Finanzielle Zusammenfassung des Gesamttopfes (1.800 EUR).</summary>
    public class FinanceSummary
    {
        public decimal TotalPot { get; set; }
        public decimal DistributedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public List<string> UnclaimedPots { get; set; } = [];
    }
}
