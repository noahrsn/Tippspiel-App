namespace TippspielApp.Domain
{
    /// <summary>Vollständiger Auswertungsbericht – enthält keine Tipp-Rohdaten.</summary>
    public class RankingReport
    {
        public DateTime GeneratedAt { get; set; }
        /// <summary>True sobald der WM-Sieger feststeht und der Haupttopf ausgezahlt wurde.</summary>
        public bool IsMainPotFinalized { get; set; }
        public List<RankingEntry> Leaderboard { get; set; } = [];
        public List<GroupClusterResult> GroupClusterResults { get; set; } = [];
        public List<BingoPotResult> BingoPotResults { get; set; } = [];
        /// <summary>Alle Preistöpfe mit Status (vergeben / offen).</summary>
        public List<PotOverviewEntry> BingoPotOverview { get; set; } = [];
        public FinanceSummary FinanceSummary { get; set; } = new();
    }

    /// <summary>Finanzielle Zusammenfassung (dynamisch: Teilnehmer × 9 €).</summary>
    public class FinanceSummary
    {
        public decimal TotalPot { get; set; }
        public decimal DistributedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public List<string> UnclaimedPots { get; set; } = [];
    }
}
