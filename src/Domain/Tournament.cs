namespace TippspielApp.Domain
{
    public class TournamentData
    {
        public List<TeamInfo> Teams { get; set; } = [];
        public List<BingoEventInfo> BingoEventCatalog { get; set; } = [];
        public List<MatchResult> MatchResults { get; set; } = [];
        public Dictionary<string, string> ActualGroupWinners { get; set; } = [];
        public Dictionary<string, List<string>> ActualKnockoutTeams { get; set; } = [];
        public List<string> OccurredBingoEvents { get; set; } = [];
        public string? ActualWorldChampionTeamId { get; set; }
        public string? ActualTopScorerName { get; set; }
        /// <summary>ISO-Datum des Eröffnungsspiels (z.B. "2026-06-11"). Tipps gesperrt ab einem Tag davor.</summary>
        public string? TournamentStartDate { get; set; }
    }

    public class TeamInfo
    {
        public string TeamId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public string FlagCode { get; set; } = string.Empty;
    }

    public class BingoEventInfo
    {
        public string EventId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Occurred { get; set; }
    }

    public class MatchResult
    {
        public string MatchId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string HomeTeamId { get; set; } = string.Empty;
        public string AwayTeamId { get; set; } = string.Empty;
        public int HomeGoals { get; set; }
        public int AwayGoals { get; set; }
        public bool IsFinished { get; set; }
    }
}
