namespace TippspielApp.Models
{
    /// <summary>Repräsentiert einen Teilnehmer mit seinen Tipps und seinem aktuellen Punktestand.</summary>
    public class User
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public UserBet BetData { get; set; } = new();
        public ScoreBoard CurrentScore { get; set; } = new();
    }

    /// <summary>Enthält alle Tipps eines Users: Spieltipps, KO-Runden, Sondertipps und Bingo-Karte.</summary>
    public class UserBet
    {
        public List<MatchBet> GroupMatchBets { get; set; } = [];
        public Dictionary<string, string>? GroupWinnerBets { get; set; }
        public Dictionary<string, List<string>> KnockoutBets { get; set; } = [];
        public SpecialBet? SpecialBets { get; set; }
        public BingoCard? BingoCard { get; set; }
    }
}