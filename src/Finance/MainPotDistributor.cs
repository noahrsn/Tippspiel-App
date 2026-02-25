using TippspielApp.Domain;

namespace TippspielApp.Finance
{
    /// <summary>
    /// Verteilt den Haupttopf der Gesamtwertung.
    /// Wird nur aufgerufen, wenn der WM-Sieger feststeht.
    /// </summary>
    public class MainPotDistributor : PrizeDistributorBase
    {
        public List<BingoPotResult> Calculate(List<User> users)
        {
            var results = new List<BingoPotResult>();
            var prizes  = PotMath.MainPotPrizes(users.Count);
            int count   = Math.Min(users.Count, prizes.Count);

            for (int i = 0; i < count; i++)
            {
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
