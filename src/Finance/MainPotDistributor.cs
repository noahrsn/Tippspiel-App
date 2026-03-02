using TippspielApp.Domain;

namespace TippspielApp.Finance
{
    /// <summary>
    /// Zahlt den Haupttopf an die besten Tipper aus.
    /// Wird erst aufgerufen, wenn der WM-Sieger eingetragen ist – vorher ist der Topf gesperrt.
    /// </summary>
    public class MainPotDistributor : PrizeDistributorBase
    {
        public List<BingoPotResult> Calculate(List<User> users)
        {
            var results = new List<BingoPotResult>();
            // Gewinnbeträge für alle ausgezahlten Plätze berechnen
            var prizes  = PotMath.MainPotPrizes(users.Count);
            // Sicherheitsnetz: nicht mehr Plätze verteilen als User vorhanden sind
            int count   = Math.Min(users.Count, prizes.Count);

            // users ist an dieser Stelle bereits nach Gesamtpunkten sortiert – Index = Platzierung
            for (int i = 0; i < count; i++)
            {
                // Preisgeld gutschreiben und im WonPots-Log festhalten
                AddWin(users[i], prizes[i], $"Gesamtwertung Platz {i + 1}");
                results.Add(new BingoPotResult
                {
                    PotLabel     = $"Gesamtwertung Platz {i + 1}",
                    WinnerUserId = users[i].UserId,
                    WinnerName   = users[i].Name,
                    Prize        = prizes[i]
                });
            }

            return results;
        }
    }
}
