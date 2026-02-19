using System.Collections.Generic;

namespace TippspielApp.Models
{
    public class User
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public UserBet BetData { get; set; }
        public ScoreBoard CurrentScore { get; set; }
    }

    public class UserBet
    {
        public List<MatchBet> GroupMatchBets { get; set; }
        public Dictionary<string, string> GroupWinnerBets { get; set; }
        public Dictionary<string, List<string>> KnockoutBets { get; set; }
        public SpecialBet SpecialBets { get; set; }
        public BingoCard BingoCard { get; set; }
    }
}