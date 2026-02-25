namespace TippspielApp.Domain
{
    /// <summary>Ergebnis eines Gruppen-Cluster-Zwischengewinns (je zwei Gruppen).</summary>
    public class GroupClusterResult
    {
        public string ClusterLabel { get; set; } = string.Empty;
        public string WinnerUserId { get; set; } = string.Empty;
        public string WinnerName { get; set; } = string.Empty;
        public int WinnerClusterPoints { get; set; }
        public decimal Prize { get; set; }
        public bool IsShared { get; set; }
        public List<string> CoWinners { get; set; } = [];
    }

    /// <summary>Ergebnis eines ausgezahlten Preisgelds (Bingo-Topf oder Gesamtwertungsplatz).</summary>
    public class BingoPotResult
    {
        public string PotLabel { get; set; } = string.Empty;
        public string WinnerUserId { get; set; } = string.Empty;
        public string WinnerName { get; set; } = string.Empty;
        public decimal Prize { get; set; }
    }

    /// <summary>Übersichts-Eintrag für einen Topf: vergeben oder noch offen.</summary>
    public class PotOverviewEntry
    {
        public string PotLabel { get; set; } = string.Empty;
        public decimal Prize { get; set; }
        public bool IsAwarded { get; set; }
        public string WinnerUserId { get; set; } = string.Empty;
        public string WinnerName { get; set; } = string.Empty;
    }
}
