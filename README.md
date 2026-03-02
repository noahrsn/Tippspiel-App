# WM 2026 – Kombiniertes Tippspiel & Bingo

Webbasierte Auswertungsanwendung für ein kombiniertes Fußball-Tippspiel mit Bingo-Komponente zur FIFA Weltmeisterschaft 2026. Das System verwaltet Tipps und Bingo-Karten für eine Tippgemeinschaft und berechnet Punkte sowie Preisgeldverteilungen vollautomatisch.

---

## Projektübersicht

Das Projekt besteht aus einem **ASP.NET Minimal-API-Backend** (C# / .NET 10) und einem **statischen Web-Frontend** (HTML, CSS, JavaScript), das direkt vom Server ausgeliefert wird. Es wird kein separater Frontend-Build-Schritt benötigt.

Kernfunktionalität:
- User-Tipps und Turnierdaten aus JSON-Dateien einlesen
- Punkte für alle Kategorien berechnen (Gruppenspiele, K.O.-Phase, Sondertipps, Bingo)
- Preisgeld dynamisch auf Basis der Teilnehmerzahl verteilen
- Ergebnisse als Rangliste im Browser darstellen

---

## Funktionsumfang

### Tippspiel
- **Gruppenspiele:** Exaktes Ergebnis (4 Pkt), Tordifferenz (3 Pkt), Tendenz (2 Pkt)
- **K.O.-Phase:** Pro korrekt getipptem Team, je nach Runde 2–10 Punkte
- **Sondertipps:** Weltmeister und Torschützenkönig (je 20 Punkte)

### Bingo
- Individuelle **4×4-Bingo-Karten** (16 Felder, kein Freifeld)
- Felder werden durch vordefinierte WM-Ereignisse erfüllt (z. B. „Rote Karte in der Gruppenphase")
- 3 Punkte pro erfülltem Feld
- Bonus-Punkte für vollständige Linien (1. Linie: 10 Pkt, 2. Linie: +6 Pkt, 3. Linie: +4 Pkt)
- 10 mögliche Linien: 4 Zeilen + 4 Spalten + 2 Diagonalen

### Finanzen
- Gesamttopf: **Teilnehmerzahl × 9 €** (dynamisch)
- **6 Gruppen-Cluster** (A+B, C+D, …): je ~16,6 % des Cluster-Anteils, nach Spielpunkten im Cluster
- **Bingo-Topf** (~22 %): aufgeteilt nach erster Linie und Full House (zeitbasiert)
- **Haupttopf** (Rest): geometrisch abgestufte Auszahlung an die Topplatzierten, wird erst nach Turnierende freigegeben

---

## Projektstruktur

```
src/
  Program.cs                  – Einstiegspunkt
  Application/
    RankingCalculator.cs      – Orchestrierung der Gesamtauswertung
    WebServer.cs              – API-Routen und Webserver-Konfiguration
  Domain/
    Tournament.cs             – Turnierdaten-Modelle (Teams, Spiele, Ereignisse)
    User.cs                   – User- und Tipp-Modelle
    BingoCard.cs              – Bingo-Karten-Modell
    ScoreSnapshot.cs          – Punktestand-Modell
    RankingEntry.cs           – Ranglisten-Eintrag
    RankingReport.cs          – Vollständiger Auswertungsbericht
    PotResults.cs             – Preisgeld-Ergebnis-Modelle
  Scoring/
    ClassicEvaluator.cs       – Gruppenspiel-Auswertung
    KnockoutEvaluator.cs      – K.O.-Runden-Auswertung
    SpecialBetEvaluator.cs    – Sondertipp-Auswertung
    BingoEvaluator.cs         – Bingo-Auswertung
    BingoBase.cs              – Linien-Definitionen und Zeitberechnungen
    EvaluatorBase.cs          – Gemeinsame Basis, enthält CalculateMatchPoints()
  Finance/
    PotMath.cs                – Topf-Berechnungsformeln
    ClusterDistributor.cs     – Gruppen-Cluster-Gewinne
    BingoDistributor.cs       – Bingo-Preisgeldverteilung
    MainPotDistributor.cs     – Haupttopf-Verteilung
    PrizeDistributorBase.cs   – Gemeinsame Basis für alle Distributoren
  Infrastructure/
    JsonDataStore.cs          – JSON-Lese- und Schreiboperationen
    UserTemplateBuilder.cs    – Leere Tipp-Vorlage für neue User

Data/
  Input/
    users.json                          – Alle Tipper mit ihren Tipps
    tournament_data_vor_turnier.json    – Datensatz: vor dem Turnier
    tournament_data_nach_gruppe.json    – Datensatz: nach der Gruppenphase
    tournament_data_nach_turnier.json   – Datensatz: nach dem Turnier
  Output/
    ranking_current.json      – Zuletzt berechnetes Ranking

wwwroot/
  index.html                  – Web-Oberfläche
  css/app.css
  js/app.js

RULES.md                    – Vollständiges Regelwerk
```

---

## Ausführen mit Visual Studio 2022

### Voraussetzungen

- **Visual Studio 2022** (Version 17.x oder neuer)
- Workload **„ASP.NET und Webentwicklung"** muss installiert sein
  _(Visual Studio Installer öffnen → Visual Studio 2022 → Ändern → Workload auswählen)_
- **.NET 10 SDK** – Download: https://dotnet.microsoft.com/download/dotnet/10.0

### Schritt-für-Schritt

**1. Projektmappe öffnen**

- Visual Studio 2022 starten
- `Datei` → `Öffnen` → `Projekt/Projektmappe...`
- Datei `Tippspiel-App.sln` im Projektordner auswählen und auf `Öffnen` klicken

**2. Startprojekt prüfen**

- Im Projektmappen-Explorer (rechts) sollte `Tippspiel-App` **fett** hervorgehoben sein
- Falls nicht: Rechtsklick auf `Tippspiel-App` → `Als Startprojekt festlegen`

**3. Anwendung starten**

- `F5` drücken (startet mit Debugger) oder `Strg+F5` (startet ohne Debugger)
- Visual Studio baut das Projekt automatisch und startet den Webserver
- Im Ausgabe-Fenster erscheint: `URL: http://localhost:5174`

**4. Browser öffnen**

- Browser öffnen und `http://localhost:5174` aufrufen
- Die Web-Oberfläche wird geladen

**5. Einloggen**

- **Admin-Zugang:** Im Login-Feld `admin` eingeben, Dataset auswählen (z. B. `nach_turnier`) und bestätigen
- **Tipper-Zugang:** Namen oder Tipper-ID eines vorhandenen Tippers eingeben (z. B. `USR-001`)

**6. Ranking berechnen**

- Im Admin-Bereich auf **„Ranking neu berechnen"** klicken
- Das Ergebnis wird in `Data/Output/ranking_current.json` gespeichert und direkt angezeigt

### Anwendung beenden

- In Visual Studio auf das rote **Stop-Symbol** klicken oder `Shift+F5` drücken
- Alternativ im Konsolenfenster `Strg+C` drücken

---

## Datasets

Das System liefert drei vorgefertigte Turnier-Datensätze zum Testen:

| Dataset-Key | Datei | Beschreibung |
|-------------|-------|--------------|
| `vor_turnier` | `tournament_data_vor_turnier.json` | Keine Ergebnisse – Tipps können eingetragen werden |
| `nach_gruppe` | `tournament_data_nach_gruppe.json` | Gruppenphase abgeschlossen |
| `nach_turnier` | `tournament_data_nach_turnier.json` | Turnier vollständig – Haupttopf freigegeben |

---

## API-Endpunkte (Übersicht)

| Methode | Pfad | Beschreibung |
|---------|------|--------------|
| GET | `/api/tournament?dataset=...` | Turnierdaten laden |
| GET | `/api/users` | Alle Tipper abrufen |
| GET | `/api/users/lookup?q=...` | Tipper nach Name oder ID suchen |
| POST | `/api/users` | Neuen Tipper anlegen |
| PUT | `/api/users/{id}` | Tipper-Daten aktualisieren |
| DELETE | `/api/users/{id}` | Tipper löschen |
| GET | `/api/ranking` | Aktuelles Ranking abrufen |
| POST | `/api/ranking/recalculate?dataset=...` | Ranking neu berechnen |
| GET | `/api/tournament-datasets` | Verfügbare Datensätze auflisten |
| GET | `/api/user-template` | Leere Tipp-Vorlage abrufen |