namespace TippspielApp.Domain
{
    /// <summary>Ein Teilnehmer – enthält Name/ID, seine Tipps und den laufenden Punktestand.</summary>
    public class User
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public UserBet BetData { get; set; } = new();
        public ScoreSnapshot CurrentScore { get; set; } = new();
    }

    /// <summary>Alle Tippdaten eines Users zusammengefasst – Gruppenspiele, KO-Runden, Sondertipps und Bingo-Karte.</summary>
    public class UserBet
    {
        public List<MatchBet> GroupMatchBets { get; set; } = [];
        public Dictionary<string, string>? GroupWinnerBets { get; set; }
        public Dictionary<string, List<string>> KnockoutBets { get; set; } = [];
        public SpecialBet? SpecialBets { get; set; }
        public BingoCard? BingoCard { get; set; }
    }

    /// <summary>Tipp für ein einzelnes Spiel – Heim- und Auswärtstore reichen aus.</summary>
    public class MatchBet
    {
        public string MatchId { get; set; } = string.Empty;
        public int HomeGoals { get; set; }
        public int AwayGoals { get; set; }
    }

    /// <summary>Die zwei Sondertipps – Weltmeister und Torschützenkönig. Jeder richtige Tipp bringt 20 Punkte.</summary>
    public class SpecialBet
    {
        public string? WorldChampionTeamId { get; set; }
        public string? TopScorerName { get; set; }
    }
}
