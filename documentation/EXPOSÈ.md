# Projekt-Exposé: WM 2026 Prediction Engine & Bingo Backend

## 1. Einleitung und Motivation
Die Fußball-Weltmeisterschaft 2026 ist ein globales Großereignis, das traditionell von Tippspielen begleitet wird. Dieses Projekt zielt darauf ab, die klassische Tipprunde durch eine innovative "Bingo-Komponente" und komplexe Zwischenwertungen zu erweitern.

**Neben der akademischen Zielsetzung ist geplant, dieses System tatsächlich produktiv für eine reale Tippgemeinschaft von ca. 50 bis 400 Personen während der WM 2026 einzusetzen.**

Das Ziel dieses Softwareprojekts ist die Entwicklung eines robusten **Backends**, das die Verwaltung der Teilnehmer, deren Tipps sowie die automatisierte Auswertung von Spielereignissen übernimmt. Im Gegensatz zu Standard-Lösungen stehen hier komplexe, benutzerdefinierte Regeln (z. B. Bingo-Events, gruppierte Zwischengewinne) im Vordergrund, die eine maßgeschneiderte Logik erfordern.

## 2. Zielsetzung
Das Hauptziel ist die Implementierung einer **Auswertungs-Logik (Calculation Engine)**, die:
1.  Tipps und Bingo-Konfigurationen der Nutzer einliest.
2.  Spielergebnisse und Ereignisse verarbeitet.
3.  Ein detailliertes Ranking sowie Gewinnverteilungen basierend auf einem komplexen Regelwerk berechnet.

### 2.1 Abgrenzung und Fokus
Der Fokus der Entwicklung liegt auf der **Backend-Logik** und der **Datenverarbeitung**.
* **Primär:** Verarbeitung von JSON-basierten Daten (Input/Output) zur Sicherstellung der Testbarkeit, Portabilität und einfachen Abgabe des Projekts.
* **Sekundär (Ausbaustufe):** Entwicklung einer Web-Oberfläche und Anbindung an eine Azure SQL Datenbank (wird bei ausreichender Zeit umgesetzt).

---

## 3. Funktionale Anforderungen

Das System muss folgende Kernfunktionen abbilden:

### 3.1 Daten-Input (JSON-basiert)
Das System muss zwei Arten von Eingabedateien verarbeiten können:
* **User-Tipps:** Eine JSON-Struktur, die pro User alle 72 Gruppenspiele, Gruppensieger, KO-Runden-Picks, Sondertipps und das individuelle 5x5 Bingo-Feld enthält.
* **Match-Data (Simuliert):** Eine JSON-Datei, die den aktuellen Turnierstatus abbildet (Ergebnisse, Torschützen, Karten, etc.). Diese Datei simuliert die Antwort der *API-Football*, um eine konsistente Testumgebung für die Abgabe zu gewährleisten.

### 3.2 Die "Calculation Engine" (Kernlogik)
Die Engine berechnet täglich den Punktestand neu.

**A. Klassische Tipp-Auswertung:**
* Punktevergabe nach Ergebnis (3), Tordifferenz (2), Tendenz (1).
* Validierung der KO-Phasen-Tipps (Punkte pro Runde, Team darf nur 1x pro Runde werten).
* Sondertipps (Weltmeister, Torschützenkönig inkl. Tie-Breaker Regel "zuerst genannter Spieler").

**B. Bingo-Logik:**
* Überprüfung der 50 möglichen Ereignisse gegen die realen Match-Daten (z. B. "Rote Karte in Gruppe A").
* Status-Update der 5x5 Matrix jedes Spielers (Mitte = Free).
* Erkennung von Mustern: Einzelne Felder (3 Pkt), Linien (8 Pkt), Full House (20 Pkt).
* Tracking der zeitlichen Abfolge für Geldgewinne (z. B. "Wer hat die *erste* Linie?").

**C. Finanz-Logik & Zwischenwertungen:**
* Berechnung der 6 Gruppen-Cluster (z. B. A+B) für die dynamischen Zwischengewinne (ca. 16-17 % des Gesamttopfs).
* Verteilung der Bingo-Geldpreise (First Line, etc.) oder Fallback-Verteilung (Top-Plätze), falls kein Bingo erreicht wird (ca. 20-22 % des Gesamttopfs).
* Endabrechnung des Haupttopfes für die Gesamtwertung (ca. 60-62 % des Gesamttopfs, dynamisch auf die Top-Plätze verteilt).

### 3.3 Output
Das System generiert eine `ranking_current.json` und stellt die Daten über eine REST-API bereit. Die API liefert das aktuelle Leaderboard, die Finanzstände und den Bingo-Fortschritt als JSON.

---

## 4. Technische Architektur und Klassendesign

Die Architektur ist in **fünf klar abgegrenzte Schichten** aufgeteilt. Jede Schicht hat genau eine Verantwortung. Business-Logik ist strikt von I/O, Routing und Domänendaten getrennt. Es gibt keine Konsolenausgabe – die Anwendung startet direkt als Web-Server.

### 4.1 Schichtenübersicht

| Schicht | Namespace | Zweck |
|---|---|---|
| `Domain/` | `TippspielApp.Domain` | Reine Datenobjekte (DTOs), keinerlei Logik |
| `Scoring/` | `TippspielApp.Scoring` | Zustandslose Punkt-Evaluatoren |
| `Finance/` | `TippspielApp.Finance` | Preisgeld-Distributoren |
| `Application/` | `TippspielApp.Application` | Orchestrierung & HTTP-Routen |
| `Infrastructure/` | `TippspielApp.Infrastructure` | Datei-I/O & JSON-Serialisierung |

### 4.2 Klassen im Detail

**Domain-Schicht** – Datencontainer ohne Logik:
* **`TournamentData`**: Turnierdaten (Spiele, Ergebnisse, Events).
* **`User`** mit `UserBet`, `MatchBet`, `SpecialBet`: Teilnehmer inkl. aller Tipps.
* **`BingoCard`** / **`BingoCell`**: Das 5×5-Bingo-Feld des Users.
* **`ScoreSnapshot`**: Basis-Punktestand (TotalPoints, ClassicPoints, BingoPoints, …).
* **`RankingEntry : ScoreSnapshot`**: Ranglisten-Zeile (erbt Punkte + ergänzt Rank, UserId, Name).
* **`RankingReport`**, **`FinanceSummary`**, **`PotOverviewEntry`**: Ausgabe-Strukturen.

**Scoring-Schicht** – Zustandslose Evaluatoren mit Vererbung:
* **`IEvaluator`**: Interface `Evaluate(User, TournamentData)`.
* **`EvaluatorBase : IEvaluator`**: Abstrakte Basis; stellt `public static CalculateMatchPoints()` bereit.
* **`ClassicEvaluator`**, **`KnockoutEvaluator`**, **`SpecialBetEvaluator`**: Je ein konkreter Evaluator, alle von `EvaluatorBase`.
* **`BingoBase`**: Abstrakte Basis mit `public static` Zeitberechnungen und `Lines[]`-Definition.
* **`BingoEvaluator : BingoBase, IEvaluator`**: Bingo-Karten-Auswertung.

**Finance-Schicht** – Preisgeldverteilung mit Vererbung:
* **`PotMath`**: Statische Topf-Formeln (`TotalPot`, `BingoPot`, `MainPot`, `Split`, …).
* **`PrizeDistributorBase`**: Abstrakte Basis mit `AddWin()` Helper.
* **`ClusterDistributor`**, **`BingoDistributor`**, **`MainPotDistributor`**: Je ein konkreter Distributor.

**Application-Schicht** – Koordination ohne I/O:
* **`RankingCalculator`**: Startet die Evaluatoren in der richtigen Reihenfolge, sortiert und führt die Finanzberechnung durch.
* **`WebServer`**: Alle ASP.NET Minimal-API-Routen (`/api/ranking`, `/api/users`, …).

**Infrastructure-Schicht** – Datei-I/O:
* **`JsonDataStore`**: `LoadUsers()`, `LoadTournamentData()`, `ExportRanking()`.
* **`UserTemplateBuilder`**: Erzeugt leere User-Vorlage aus Turnierdaten.

### 4.3 Programmablauf

`Program.cs` (4 Zeilen) ruft nur `WebServer.Run(args)` auf. Die eigentliche Pipeline läuft in `RankingCalculator.Run()`:

1. **Reset**: Alle Scores werden auf 0 zurückgesetzt.
2. **Evaluate**: Die Evaluatoren laufen in Reihenfolge: `ClassicEvaluator → KnockoutEvaluator → SpecialBetEvaluator → BingoEvaluator`.
3. **Sort**: User-Liste wird nach `TotalPoints` absteigend sortiert.
4. **Finance**: `ClusterDistributor`, `BingoDistributor`, `MainPotDistributor` berechnen Geldgewinne.
5. **Export**: `JsonDataStore` speichert das Ergebnis als `ranking_current.json`.
6. **Response**: Der Web-Server gibt `RankingReport` als JSON zurück.

### 4.4 Vererbungshierarchie

```
ScoreSnapshot
  └── RankingEntry

IEvaluator
  └── EvaluatorBase
        ├── ClassicEvaluator
        ├── KnockoutEvaluator
        └── SpecialBetEvaluator

BingoBase
  └── BingoEvaluator  (implementiert auch IEvaluator)

PrizeDistributorBase
  ├── ClusterDistributor
  ├── BingoDistributor   (nutzt BingoBase.static – kein Mehrfach-Erben nötig)
  └── MainPotDistributor
```

---

## 5. Projektablauf & Meilensteine

### Phase 1: Datenmodellierung & MVP (Pflichtteil)
* Definition der JSON-Schemata für Tipps und Match-Daten.
* Implementierung der Punkte-Logik (Gruppenphase & KO-System).
* Implementierung der Bingo-Matrix-Auswertung.
* **Ziel:** Erfolgreicher Run eines Test-Cases via Web-API mit JSON-Input und -Output.

### Phase 2: Erweiterte Logik (Pflichtteil)
* Implementierung der Geld-Gewinnverteilung (Zwischengewinne & Haupttopf).
* Handling von Sonderfällen (z. B. Torschützenkönig-Gleichstand).
* Simulation eines kompletten Turnierverlaufs durch Einspeisen verschiedener Tages-Dateien.

### Phase 3: Cloud & UI (Kür/Optional)
* Bereitstellung einer einfachen Web-Oberfläche zur Visualisierung der JSON-Daten.
* Deployment auf Azure Students.
* Anbindung der echten API-Football Schnittstelle für Live-Daten.

---

## 6. Teststrategie für die Abgabe
Um die Korrektheit der komplexen Regeln zu beweisen, werden **Szenario-Dateien** erstellt:
1.  `users.json`: Enthält Test-User mit unterschiedlichen Tipp-Strategien.
2.  `tournament_data.json`: Simuliert verschiedene Turnier-Tage (z. B. "Ende Gruppenphase", "Finale").
3.  **Erwartetes Ergebnis:** Das System muss für diese Inputs deterministisch die korrekten Punkte und Geldbeträge ausgeben (Vergleich Soll/Ist).