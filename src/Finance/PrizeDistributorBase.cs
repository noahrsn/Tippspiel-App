using TippspielApp.Domain;

namespace TippspielApp.Finance
{
    /// <summary>
    /// Gemeinsame Basis für alle Distributoren.
    /// AddWin() schreibt Preisgeld und den lesbaren Topf-Eintrag auf einmal,
    /// damit das nicht in jedem Distributor separat gemacht werden muss.
    /// </summary>
    public abstract class PrizeDistributorBase
    {
        // Preisgeld gutschreiben und gleichzeitig im WonPots-Log festhalten
        protected static void AddWin(User user, decimal amount, string label)
        {
            user.CurrentScore.TotalFinancialWinnings += amount;
            user.CurrentScore.WonPots.Add($"{label} ({amount:F0} EUR)");
        }
    }
}
