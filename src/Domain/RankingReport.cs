namespace TippspielApp.Domain
{
    /// <summary>Das fertige Ergebnis einer Ranking-Berechnung – Rangliste, Cluster, Bingo und Finanzübersicht.</summary>
    public class RankingReport
    {
        public DateTime GeneratedAt { get; set; }
        // Ist der WM-Sieger bekannt? Zeigt ob der Haupttopf schon ausgespielt wurde
        public bool IsMainPotFinalized { get; set; }
        public List<RankingEntry> Leaderboard { get; set; } = [];
        public List<GroupClusterResult> GroupClusterResults { get; set; } = [];
        public List<BingoPotResult> BingoPotResults { get; set; } = [];
        // Welche Bingo-Töpfe sind vergeben, welche warten noch?
        public List<PotOverviewEntry> BingoPotOverview { get; set; } = [];
        public FinanceSummary FinanceSummary { get; set; } = new();
    }

    /// <summary>Finanzübersicht – wie viel wurde schon verteilt und was ist noch offen?</summary>
    public class FinanceSummary
    {
        public decimal TotalPot { get; set; }
        public decimal DistributedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public List<string> UnclaimedPots { get; set; } = [];
    }
}
