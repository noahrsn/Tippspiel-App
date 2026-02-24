namespace TippspielApp.Models
{
    /// <summary>Repräsentiert den Tipp eines Users für ein einzelnes Gruppenspiel.</summary>
    public class MatchBet
    {
        public string MatchId { get; set; } = string.Empty;
        /// <summary>Getippte Tore der Heimmannschaft.</summary>
        public int HomeGoals { get; set; }
        /// <summary>Getippte Tore der Gastmannschaft.</summary>
        public int AwayGoals { get; set; }
    }

    /// <summary>Sondertipps: Weltmeister-Team und Torschützenkönig.</summary>
    public class SpecialBet
    {
        /// <summary>Team-ID des getippten Weltmeisters. Korrekt = 20 Punkte.</summary>
        public string? WorldChampionTeamId { get; set; }
        /// <summary>Vollständiger Name des getippten Torschützenkönigs. Korrekt = 20 Punkte.</summary>
        public string? TopScorerName { get; set; }
    }
}