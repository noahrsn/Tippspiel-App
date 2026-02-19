using System.Collections.Generic;

namespace TippspielApp.Models
{
    public class TournamentData
    {
        public List<MatchResult> MatchResults { get; set; }
        public Dictionary<string, string> ActualGroupWinners { get; set; }
        public Dictionary<string, List<string>> ActualKnockoutTeams { get; set; }
        public List<string> OccurredBingoEvents { get; set; }
        public string ActualWorldChampionTeamId { get; set; }
        public string ActualTopScorerName { get; set; }
    }

    public class MatchResult
    {
        public string MatchId { get; set; }
        public string GroupName { get; set; }
        public int HomeGoals { get; set; }
        public int AwayGoals { get; set; }
        public bool IsFinished { get; set; }
    }
}