using System.Collections.Generic;
using TippspielApp.Models;

namespace TippspielApp.Services
{
    public class BingoEvaluator
    {
        public void UpdateBingoCards(List<User> users, List<string> occurredEvents) 
        { 
        }
        
        public int CalculateBingoPoints(BingoCard card) 
        { 
            return 0; 
        }
        
        private int CountFulfilledCells(BingoCard card) 
        { 
            return 0; 
        }
        
        private int CountCompletedLines(BingoCard card) 
        { 
            return 0; 
        }
        
        private bool IsFullHouse(BingoCard card) 
        { 
            return false; 
        }
    }
}