using TippspielApp.Domain;
using TippspielApp.Scoring;

namespace TippspielApp.Finance
{
    /// <summary>
    /// Verteilt die Bingo-Preise – wer hat zuerst eine Linie, wer zuerst Full House?
    /// Zeiten werden über BingoBase verglichen (statische Methoden, weil kein Erben möglich).
    /// </summary>
    public class BingoDistributor : PrizeDistributorBase
    {
        public List<BingoPotResult> Calculate(List<User> users)
        {
            var results      = new List<BingoPotResult>();
            decimal bingoPot = PotMath.BingoPot(users.Count);

            // 45 % des Bingo-Topfs gehen an Linien-Gewinner, der Rest an Full-House-Gewinner
            decimal linePot = PotMath.RoundToFive(bingoPot * 0.45m);
            decimal fhPot   = bingoPot - linePot;

            // Anzahl der ausgezahlten Linien-Plätze skaliert mit der Teilnehmerzahl
            int nLine = users.Count < 75  ? 2 : 3;
            // Full-House-Plätze analog – bei weniger Leuten reicht ein Gewinner
            int nFH   = users.Count < 100 ? 1 : users.Count < 200 ? 2 : 3;

            // Aufteilung der Linien-Preise je nach Anzahl der Gewinner-Slots
            var linePrizes = nLine == 3
                ? PotMath.Split(linePot, [0.50m, 0.30m, 0.20m])  // 50/30/20 bei 3 Plätzen
                : PotMath.Split(linePot, [0.60m, 0.40m]);         // 60/40 bei 2 Plätzen

            // Full-House-Aufteilung analog
            var fhPrizes = nFH == 3
                ? PotMath.Split(fhPot, [0.51m, 0.29m, 0.20m])
                : nFH == 2
                    ? PotMath.Split(fhPot, [0.55m, 0.45m])
                    : PotMath.Split(fhPot, [1.00m]);  // alles an einen Gewinner

            // Jeden User nach dem frühesten Zeitpunkt seiner ersten vollständigen Linie sortieren
            var byLine = users
                .Select(u => (User: u, Time: BingoBase.GetEarliestLineCompletion(u.BetData?.BingoCard)))
                .Where(x => x.Time.HasValue)     // nur User die überhaupt eine Linie haben
                .OrderBy(x => x.Time)            // frühester Zeitpunkt = Rang 1
                .ToList();

            // Die ersten nLine (oder weniger) Einträge bekommen Preisgeld
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

            // Dasselbe für Full House – wer hat zuerst alle 16 Felder?
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

        // Liefert alle Bingo-Töpfe mit ihren Beträgen – wird für die Übersicht im Report gebraucht
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
