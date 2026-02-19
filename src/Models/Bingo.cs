using System;
using System.Collections.Generic;

namespace TippspielApp.Models
{
    public class BingoCard
    {
        public List<BingoCell> Cells { get; set; }
    }

    public class BingoCell
    {
        public int Position { get; set; }
        public string EventId { get; set; }
        public bool IsFulfilled { get; set; }
        public DateTime? FulfilledAt { get; set; }
    }
}