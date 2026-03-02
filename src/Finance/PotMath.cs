namespace TippspielApp.Finance
{
    /// <summary>
    /// Alle Berechnungsformeln für die Preistöpfe an einem Ort gesammelt.
    /// Da der Topf von der Teilnehmerzahl abhängt (n × 9 €), werden alle Beträge live berechnet.
    /// </summary>
    public static class PotMath
    {
        // Auf 5 € runden – damit die Preise "runde" Beträge bleiben
        // Trick: durch 5 teilen, runden, wieder mit 5 multiplizieren
        public static decimal RoundToFive(decimal value)
            => Math.Round(value / 5m, MidpointRounding.AwayFromZero) * 5m;

        // Jeder zahlt 9 € – daraus entsteht der gesamte Preistopf
        public static decimal TotalPot(int n) => n * 9m;

        // Preis für jeden der 6 Cluster – ca. 16,6 % des Topfs aufgeteilt auf 6 Blöcke
        public static decimal ClusterPrize(int n) => RoundToFive(n * 9m * 0.166m / 6m);

        // ~22 % des Gesamttopfs gehen in den Bingo-Bereich
        public static decimal BingoPot(int n) => RoundToFive(n * 9m * 0.222m);

        // Was übrig bleibt nach Cluster und Bingo – geht in die Gesamtwertung
        public static decimal MainPot(int n) => TotalPot(n) - ClusterPrize(n) * 6m - BingoPot(n);

        // Wieviele Plätze werden ausgezahlt? Mindestens 5, maximal ~7 % der Teilnehmer
        public static int NumberOfWinners(int n) => Math.Max(5, (int)Math.Round(n * 0.07));

        /// <summary>
        /// Berechnet die Gewinnbeträge für Platz 1 bis n.
        /// Die Beträge nehmen geometrisch ab (Faktor ~0,68) und werden auf 5 € gerundet.
        /// Platz 1 bekommt den Rest, damit die Summe exakt aufgeht.
        /// </summary>
        public static List<decimal> MainPotPrizes(int n)
        {
            int    count  = NumberOfWinners(n);
            decimal pot   = MainPot(n);
            // r = Abklingfaktor: Platz i+1 bekommt ~68 % von dem was Platz i bekommt
            const double r = 0.68;

            // Gewichtungs-Array: weights[0]=1, weights[1]=0.68, weights[2]=0.68², usw.
            double[] weights    = Enumerable.Range(0, count).Select(i => Math.Pow(r, i)).ToArray();
            double   totalW     = weights.Sum();  // Summe aller Gewichte für spätere Normierung
            var      prizes     = new decimal[count];
            decimal  assigned   = 0m;  // aufsummierte Beträge für Plätze 2 bis n

            // Plätze 2..n berechnen und runden; Mindestbetrag 15 € damit keiner leer ausgeht
            for (int i = 1; i < count; i++)
            {
                prizes[i] = Math.Max(15m, RoundToFive((decimal)(weights[i] / totalW) * pot));
                assigned += prizes[i];
            }
            // Platz 1 bekommt den Rest – so geht die Summe garantiert exakt auf
            prizes[0] = pot - assigned;
            return [.. prizes];
        }

        /// <summary>
        /// Hilfsmethode: teilt einen Topf nach prozentualen Anteilen auf.
        /// Letzter Preis bekommt den Rest, damit keine Rundungsdifferenzen entstehen.
        /// </summary>
        public static List<decimal> Split(decimal pot, decimal[] ratios)
        {
            var prizes   = new decimal[ratios.Length];
            decimal used = 0m;
            // Alle außer dem letzten Anteil normal berechnen und aufsummieren
            for (int i = 0; i < ratios.Length - 1; i++)
            {
                prizes[i] = RoundToFive(pot * ratios[i]);
                used      += prizes[i];
            }
            // Letzter Preis = Restbetrag, damit die Summe nie vom Topf abweicht
            prizes[^1] = pot - used;
            return [.. prizes];
        }
    }
}
