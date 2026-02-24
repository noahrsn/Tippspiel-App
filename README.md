# 🏆 WM 2026 – Kombiniertes Tippspiel & Bingo

Dieses Projekt ist das Backend für ein kombiniertes Tippspiel und Bingo zur Fußball-Weltmeisterschaft 2026. Es wurde entwickelt, um die Tipps, Bingo-Karten und Punkteauswertungen für eine Tippgemeinschaft von ca. 200 Personen zu verwalten.

## 📖 Projektübersicht

Das System kombiniert ein klassisches Fußball-Tippspiel mit einer innovativen Bingo-Komponente. Neben der Vorhersage von Spielergebnissen und Turnierverläufen können die Teilnehmer durch das Eintreten spezifischer Spielereignisse (z. B. "Rote Karte in Gruppe A") auf ihrer individuellen 5x5-Bingo-Karte punkten.

Die Kernaufgabe dieses Projekts ist die **Calculation Engine**, die:
1. User-Tipps und Bingo-Konfigurationen einliest.
2. Reale (oder simulierte) Spielergebnisse und Ereignisse verarbeitet.
3. Ein detailliertes Ranking sowie komplexe Gewinnverteilungen (inkl. Zwischengewinnen) berechnet.

## ⚙️ Funktionen

* **Klassisches Tippspiel:**
  * Auswertung von 72 Gruppenspielen (Exaktes Ergebnis, Tordifferenz, Tendenz).
  * Tipps für die K.O.-Phase (unabhängig vom Turnierbaum).
  * Sondertipps (Weltmeister, Torschützenkönig).
* **Bingo-Komponente:**
  * Individuelle 5x5-Bingo-Karten pro User (mit "Free"-Feld in der Mitte).
  * Auswertung von 50 vordefinierten Ereignissen.
  * Punkte für erfüllte Felder und vollständige Linien.
* **Finanz- & Gewinnlogik:**
  * Verwaltung eines Gesamttopfes von 1.800 €.
  * Berechnung von Zwischengewinnen für Gruppen-Cluster (z. B. Gruppe A+B).
  * Verteilung von reinen Bingo-Gewinnen (z. B. Erste vollständige Linie, Bester Bingospieler).
  * Endabrechnung für die Top 20 der Gesamtwertung.
* **Datenverarbeitung:**
  * JSON-basierter Input für User-Tipps und Match-Daten.
  * JSON-basierter Output für das aktuelle Ranking und die Gewinnverteilung.

## 📂 Projektstruktur

* `src/`: Enthält den C#-Quellcode der Anwendung.
  * `Models/`: Datenmodelle (User, Bets, Bingo, Tournament, Ranking).
  * `Services/`: Geschäftslogik (CalculationEngine, BingoEvaluator, ClassicBetEvaluator, FinanceCalculator, DataHandler).
* `Data/`: Enthält die JSON-Dateien für Input und Output.
  * `Input/`: `users.json` (Tipps) und `tournament_data.json` (Spielergebnisse).
  * `Output/`: `ranking_current.json` (Berechnetes Ranking).
* `Documentation/`: Enthält detaillierte Dokumentationen.
  * `RULES.md`: Das vollständige Regelwerk des Tippspiels.
  * `EXPOSÈ.md`: Das Projekt-Exposé mit Architektur- und Designentscheidungen.

## 🚀 Erste Schritte

### Voraussetzungen
* .NET 10.0 SDK (oder kompatibel)

### Ausführen der Anwendung
1. Klonen Sie das Repository.
2. Navigieren Sie in das Projektverzeichnis: `cd Tippspiel-App`
3. Stellen Sie sicher, dass die Eingabedateien (`users.json` und `tournament_data.json`) im Ordner `Data/Input/` vorhanden sind.
4. Führen Sie das Projekt aus:
   ```bash
   dotnet run
   ```
5. Die Ergebnisse werden in der Konsole ausgegeben und in der Datei `Data/Output/ranking_current.json` gespeichert.

## 📄 Dokumentation

Weitere Details zu den Regeln und der Architektur finden Sie in den Dateien im Ordner `Documentation/`:
* [Regelwerk (RULES.md)](Documentation/RULES.md)
* [Projekt-Exposé (EXPOSÈ.md)](Documentation/EXPOSÈ.md)
