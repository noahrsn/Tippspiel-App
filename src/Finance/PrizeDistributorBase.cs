using TippspielApp.Domain;

namespace TippspielApp.Finance
{
    /// <summary>
    /// Abstrakte Basis für alle Preisgeld-Distributoren.
    /// Stellt AddWin() als gemeinsame Hilfsmethode bereit, die Preisgeld und
    /// WonPots-Eintrag in einem Schritt schreibt.
    /// </summary>
    public abstract class PrizeDistributorBase
    {
        /// <summary>Bucht einen Gewinn auf den User und fügt einen lesbaren Eintrag in WonPots ein.</summary>
        protected static void AddWin(User user, decimal amount, string label)
        {
            user.CurrentScore.TotalFinancialWinnings += amount;
            user.CurrentScore.WonPots.Add($"{label} ({amount:F0} EUR)");
        }
    }
}
