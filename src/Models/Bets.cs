namespace TippspielApp.Models
{
    public class MatchBet
    {
        public string MatchId { get; set; }
        public int HomeGoals { get; set; }
        public int AwayGoals { get; set; }
    }

    public class SpecialBet
    {
        public string WorldChampionTeamId { get; set; }
        public string TopScorerName { get; set; }
    }
}