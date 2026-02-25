namespace TippspielApp.Domain
{
    /// <summary>Repräsentiert einen Teilnehmer mit seinen Tipps und seinem aktuellen Punktestand.</summary>
    public class User
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public UserBet BetData { get; set; } = new();
        public ScoreSnapshot CurrentScore { get; set; } = new();
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

    /// <summary>Tipp für ein einzelnes Gruppenspiel.</summary>
    public class MatchBet
    {
        public string MatchId { get; set; } = string.Empty;
        public int HomeGoals { get; set; }
        public int AwayGoals { get; set; }
    }

    /// <summary>Sondertipps: Weltmeister-Team (20 Pkt) und Torschützenkönig (20 Pkt).</summary>
    public class SpecialBet
    {
        public string? WorldChampionTeamId { get; set; }
        public string? TopScorerName { get; set; }
    }
}
