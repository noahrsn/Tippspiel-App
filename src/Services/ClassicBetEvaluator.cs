using System.Collections.Generic;
using TippspielApp.Models;

namespace TippspielApp.Services
{
    public class ClassicBetEvaluator
    {
        public int CalculateMatchPoints(MatchBet bet, MatchResult result) 
        { 
            return 0; 
        }
        
        public int CalculateGroupWinnerPoints(Dictionary<string, string> bets, Dictionary<string, string> actuals) 
        { 
            return 0; 
        }
        
        public int CalculateKnockoutPoints(Dictionary<string, List<string>> bets, Dictionary<string, List<string>> actuals) 
        { 
            return 0; 
        }
        
        public int CalculateSpecialBetPoints(SpecialBet bet, TournamentData data) 
        { 
            return 0; 
        }
        
        public void UpdateClassicScores(User user, TournamentData data) 
        { 
        }
    }
}