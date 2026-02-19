using System.Collections.Generic;
using TippspielApp.Models;
using TippspielApp.Services;

namespace TippspielApp.Services
{
    public class CalculationEngine
    {
        private readonly ClassicBetEvaluator _classicEvaluator;
        private readonly BingoEvaluator _bingoEvaluator;
        private readonly FinanceCalculator _financeCalculator;

        public CalculationEngine()
        {
            _classicEvaluator = new ClassicBetEvaluator();
            _bingoEvaluator = new BingoEvaluator();
            _financeCalculator = new FinanceCalculator();
        }

        public RankingReport RunDailyCalculation(List<User> users, TournamentData currentData) 
        { 
            return null; 
        }

        private void SortLeaderboardWithTieBreaker(List<User> users) 
        { 
        }
    }
}