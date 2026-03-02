namespace TippspielApp.Domain
{
    /// <summary>Ergebnis für einen der sechs Gruppen-Cluster (z.B. A+B) – wer hat gewonnen und wie viel bekommt er?</summary>
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

    /// <summary>Wird für Bingo-Gewinne und Gesamtwertungs-Plätze gleichermaßen genutzt – einfach Label, Gewinner und Betrag.</summary>
    public class BingoPotResult
    {
        public string PotLabel { get; set; } = string.Empty;
        public string WinnerUserId { get; set; } = string.Empty;
        public string WinnerName { get; set; } = string.Empty;
        public decimal Prize { get; set; }
    }

    /// <summary>Zeigt an ob ein Preistopf schon vergeben wurde oder noch aussteht.</summary>
    public class PotOverviewEntry
    {
        public string PotLabel { get; set; } = string.Empty;
        public decimal Prize { get; set; }
        public bool IsAwarded { get; set; }
        public string WinnerUserId { get; set; } = string.Empty;
        public string WinnerName { get; set; } = string.Empty;
    }
}
