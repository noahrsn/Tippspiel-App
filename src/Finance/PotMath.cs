namespace TippspielApp.Finance
{
    /// <summary>
    /// Statische Formeln für alle Topf-Berechnungen.
    /// Alle Beträge sind dynamisch abhängig von der Teilnehmerzahl (Teilnehmer × 9 €).
    /// </summary>
    public static class PotMath
    {
        /// <summary>Rundet einen Betrag auf den nächsten 5-€-Schritt.</summary>
        public static decimal RoundToFive(decimal value)
            => Math.Round(value / 5m, MidpointRounding.AwayFromZero) * 5m;

        /// <summary>Gesamttopf = Teilnehmer × 9 €.</summary>
        public static decimal TotalPot(int n) => n * 9m;

        /// <summary>Gewinn pro Gruppen-Cluster (ca. 16,6 % / 6 Blöcke), auf 5 € gerundet.</summary>
        public static decimal ClusterPrize(int n) => RoundToFive(n * 9m * 0.166m / 6m);

        /// <summary>Bingo-Topf (ca. 22,2 % des Gesamttopfs), auf 5 € gerundet.</summary>
        public static decimal BingoPot(int n) => RoundToFive(n * 9m * 0.222m);

        /// <summary>Haupttopf = Gesamttopf − 6 × Cluster − Bingo-Topf.</summary>
        public static decimal MainPot(int n) => TotalPot(n) - ClusterPrize(n) * 6m - BingoPot(n);

        /// <summary>Anzahl der bezahlten Plätze (ca. 7 % der Tipper, mindestens 5).</summary>
        public static int NumberOfWinners(int n) => Math.Max(5, (int)Math.Round(n * 0.07));

        /// <summary>
        /// Gewinnliste für die Gesamtwertung: geometrisch abnehmend (Faktor ~0,68), auf 5 € gerundet.
        /// Platz 1 erhält den Restbetrag, damit die Summe exakt dem Topf entspricht.
        /// </summary>
        public static List<decimal> MainPotPrizes(int n)
        {
            int    count  = NumberOfWinners(n);
            decimal pot   = MainPot(n);
            const double r = 0.68;

            double[] weights    = Enumerable.Range(0, count).Select(i => Math.Pow(r, i)).ToArray();
            double   totalW     = weights.Sum();
            var      prizes     = new decimal[count];
            decimal  assigned   = 0m;

            for (int i = 1; i < count; i++)
            {
                prizes[i] = Math.Max(15m, RoundToFive((decimal)(weights[i] / totalW) * pot));
                assigned += prizes[i];
            }
            prizes[0] = pot - assigned;
            return [.. prizes];
        }

        /// <summary>
        /// Verteilt einen Topf anhand von Quoten-Anteilen, auf 5 € gerundet.
        /// Der letzte Preis erhält den Restbetrag (rundungssicher).
        /// </summary>
        public static List<decimal> Split(decimal pot, decimal[] ratios)
        {
            var prizes   = new decimal[ratios.Length];
            decimal used = 0m;
            for (int i = 0; i < ratios.Length - 1; i++)
            {
                prizes[i] = RoundToFive(pot * ratios[i]);
                used      += prizes[i];
            }
            prizes[^1] = pot - used;
            return [.. prizes];
        }
    }
}
