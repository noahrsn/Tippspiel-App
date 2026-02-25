using TippspielApp.Domain;
using TippspielApp.Scoring;

namespace TippspielApp.Finance
{
    /// <summary>
    /// Berechnet die finanziellen Bingo-Gewinne (Linie und Full House).
    /// Greift auf BingoBase für Linien-Definitionen und Zeitberechnungen zu (statische Methoden).
    /// </summary>
    public class BingoDistributor : PrizeDistributorBase
    {
        public List<BingoPotResult> Calculate(List<User> users)
        {
            var results      = new List<BingoPotResult>();
            decimal bingoPot = PotMath.BingoPot(users.Count);

            decimal linePot = PotMath.RoundToFive(bingoPot * 0.45m);
            decimal fhPot   = bingoPot - linePot;

            int nLine = users.Count < 75  ? 2 : 3;
            int nFH   = users.Count < 100 ? 1 : users.Count < 200 ? 2 : 3;

            var linePrizes = nLine == 3
                ? PotMath.Split(linePot, [0.50m, 0.30m, 0.20m])
                : PotMath.Split(linePot, [0.60m, 0.40m]);

            var fhPrizes = nFH == 3
                ? PotMath.Split(fhPot, [0.51m, 0.29m, 0.20m])
                : nFH == 2
                    ? PotMath.Split(fhPot, [0.55m, 0.45m])
                    : PotMath.Split(fhPot, [1.00m]);

            var byLine = users
                .Select(u => (User: u, Time: BingoBase.GetEarliestLineCompletion(u.BetData?.BingoCard)))
                .Where(x => x.Time.HasValue)
                .OrderBy(x => x.Time)
                .ToList();

            for (int i = 0; i < Math.Min(nLine, byLine.Count); i++)
            {
                var w = byLine[i].User;
                AddWin(w, linePrizes[i], $"Bingo Linie #{i + 1}");
                results.Add(new BingoPotResult
                {
                    PotLabel     = $"Bingo: {i + 1}. Linie",
                    WinnerUserId = w.UserId,
                    WinnerName   = w.Name,
                    Prize        = linePrizes[i]
                });
            }

            var byFH = users
                .Select(u => (User: u, Time: BingoBase.GetFullHouseCompletionTime(u.BetData?.BingoCard)))
                .Where(x => x.Time.HasValue)
                .OrderBy(x => x.Time)
                .ToList();

            for (int i = 0; i < Math.Min(nFH, byFH.Count); i++)
            {
                var w = byFH[i].User;
                AddWin(w, fhPrizes[i], $"Bingo Full House #{i + 1}");
                results.Add(new BingoPotResult
                {
                    PotLabel     = $"Bingo: {i + 1}. Full House",
                    WinnerUserId = w.UserId,
                    WinnerName   = w.Name,
                    Prize        = fhPrizes[i]
                });
            }

            return results;
        }

        /// <summary>Gibt alle erwarteten Bingo-Preisslots zurück (für Topf-Übersicht).</summary>
        public static List<(string Label, decimal Amount)> GetPrizeSlots(int n)
        {
            decimal bingoPot = PotMath.BingoPot(n);
            decimal linePot  = PotMath.RoundToFive(bingoPot * 0.45m);
            decimal fhPot    = bingoPot - linePot;

            int nLine = n < 75  ? 2 : 3;
            int nFH   = n < 100 ? 1 : n < 200 ? 2 : 3;

            var lineP = PotMath.Split(linePot, nLine == 3 ? [0.50m, 0.30m, 0.20m] : [0.60m, 0.40m]);
            var fhP   = PotMath.Split(fhPot,   nFH == 3  ? [0.51m, 0.29m, 0.20m]
                                              : nFH == 2  ? [0.55m, 0.45m] : [1.00m]);

            var slots = new List<(string Label, decimal Amount)>();
            for (int i = 0; i < nLine; i++) slots.Add(($"Bingo: {i + 1}. Linie",      lineP[i]));
            for (int i = 0; i < nFH;   i++) slots.Add(($"Bingo: {i + 1}. Full House", fhP[i]));
            return slots;
        }
    }
}
