using System;
using System.Collections.Generic;

namespace TippspielApp.Models
{
    public class ScoreBoard
    {
        public int TotalPoints { get; set; }
        public int ClassicPoints { get; set; }
        public int KnockoutPoints { get; set; }
        public int BingoPoints { get; set; }
        public int FulfilledBingoCells { get; set; }
        public decimal TotalFinancialWinnings { get; set; }
        public List<string> WonPots { get; set; }
    }

    public class RankingReport
    {
        public DateTime GeneratedAt { get; set; }
        public List<User> Leaderboard { get; set; }
        public FinanceSummary FinanceSummary { get; set; }
    }

    public class FinanceSummary
    {
        public decimal TotalPot { get; set; }
        public decimal DistributedAmount { get; set; }
        public List<string> UnclaimedPots { get; set; }
    }
}