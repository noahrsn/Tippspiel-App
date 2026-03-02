# Struktur und Skript für das Referat: "Programmieren mit C#" (Tippspiel-App)

**Modul:** Programmieren mit C# (Prof. Dr. Reinhold Kloos)  
**Zielvorgabe:** 12 Minuten Präsentationszeit  
**Fokus:** C# Kernkonzepte, Klassenstruktur, SOLID, Collections, JSON-Persistenz  
**Prüfungsanforderungen referenziert (BANF0 - BANF4):** Erfüllt.

---

## Zeitplan und Agenda (12 Minuten Gesamt)

1. **Einleitung & Projektanforderungen (ca. 2 Min)** - *Reales Projekt (200-400 Tipper), Web vs. WPF*
2. **Entwicklungsprozess & Architektur (ca. 2,5 Min)** - *Von den Requirements zur 5-Schichten-Struktur*
3. **Fokusthema 1: Objektorientierung & SOLID (ca. 3,5 Min)** - *Schnittstellen & Vererbung*
4. **Fokusthema 2: Collections & LINQ in C# (ca. 2 Min)** - *Listen, HashSets & Datenverarbeitung*
5. **Fokusthema 3: Persistenz mit JSON (ca. 1 Min)** - *System.Text.Json (BANF3)*
6. **Fazit & Ausblick (ca. 1 Min)** - *Azure Deployment, Datenbank, User Experience*

---

## 1. Einleitung & Anforderungen des Projekts (ca. 2 Minuten)

**Begrüßung und Einstieg:**
* "Hallo zusammen, ich präsentiere euch heute mein Projekt für dieses Modul, welches nicht nur für die Prüfung entwickelt wurde, sondern für den realen produktiven Einsatz zur Fußball Weltmeisterschaft 2026."
* **Das reale Projekt & Anforderungen:** Ein komplexes Backend für ein kombiniertes Tippspiel mit einem Umfang von **200 bis 400 Tippern**. Es vereint klassisches Ergebnis-Tippen mit einer völlig neuen Bingo-Komponente (z. B. Ereignisse wie "Rote Karte in Gruppe A" füllen eine persönliche 5x5 Bingo-Karte).
* **Die Herausforderung:** Da hier mit echten Einsätzen (insg. ca. 1.800€) und komplexen Gewinnverteilungen gearbeitet wird, war die Anforderung an eine exakte, performante und saubere Berechnungsschicht (*Calculation Engine*) oberste Priorität. Gleichzeitig mussten die Tipper ihre Tipps unkompliziert abgeben und einsehen können.

**Exkurs: Warum Web-App (HTML) und nicht WPF / MAUI? (Abweichung von BANF1)**
* Die Vorgabe (BANF1) spezifiziert oft eine klassische WPF- oder MAUI-Oberfläche. Da dieses System jedoch real von hunderten Usern auf ihren Smartphones, Tablets und Laptops genutzt werden soll, war eine klassische Desktop-App nicht machbar.
* **Die Lösung:** Eine Web-Anwendung ist zwingend erforderlich, damit jeder Nutzer einfach über den Browser partizipieren kann. Das Projekt ist ein reines C#-Backend (ASP.NET Minimal API), das auf Azure als Web App gehostet wird. Das Frontend (HTML/JS) konsumiert die Backend-Routen. Die Backend-Logik ist exakt dieselbe OOP-Welt wie bei WPF, erfüllt dieselben Kriterien, ist aber für diesen konkreten Use Case praxistauglich.

---

## 2. Entwicklungsprozess & Architektur (ca. 2,5 Minuten)

**Entwicklungsprozess:**
* Der Prozess begann klassisch mit der Anforderungserhebung: Welche Regeln gelten für das Bingo? Wie müssen bei Punktgleichstand in K.O.-Runden Gelder aufgeteilt werden? 
* Das führte zur Erstellung des Regelwerks (Requirements) und anschließend zu UML-Diagrammen, um die Domänenmodelle (`User`, `BingoCard`, `ScoreSnapshot`) von der Berechnungslogik strikt zu trennen.

**Die 5-Schichten-Architektur:**
* Um die aus dem Prozess resultierende hohe Komplexität abzufedern, habe ich das Projekt extrem modular nach Clean Architecture Ansätzen aufgeteilt in fünf klare Namespaces:
  1. `Domain`: Die rein datenhaltenden Klassen (ohne Logik).
  2. `Scoring`: Die Evaluatoren für die Punkteberechnung.
  3. `Finance`: Die Verteilung der Preisgelder.
  4. `Application`: Dirigiert das Zusammenspiel und betreibt die API.
  5. `Infrastructure`: Kümmert sich um Datei- und Datenzugriffe.

**Die wichtigsten Klassen zum Verständnis (BANF4 - Begründeter Aufbau):**
* **`User` / `TournamentData`**: Entkoppelter Zustand der Spieler und der Spiele.
* **`RankingCalculator`**: Der "Dirigent" (Orchestrator). Diese Klasse enthält die Pipeline: Erst Punkte zurücksetzen, dann alle Evaluatoren über das Turnier laufen lassen, sortieren, dann Finanzen ausrechnen, erneut sortieren, und als `RankingReport` bereitstellen.
* **Die Evaluatoren (`ClassicEvaluator`, `BingoEvaluator`)**: Diese implementieren die tatsächliche Rechnungslogik zustandslos und sind voneinander unabhängig.

---

## 3. Fokusthema 1: Objektorientierte Prinzipien & SOLID in C# (ca. 3,5 Minuten)

*(Bezug zu BANF2: OOM und SOLID-Kriterien)*

"Ein wesentlicher Teil meiner Entwicklungsarbeit lag in der Einhaltung der SOLID-Prinzipien, um eine testbare und wartbare C#-Anwendung zu schreiben."

* **S – Single Responsibility Principle (SRP):** 
  * Ich trenne strikt zwischen Punkteberechnung und Geldverteilung. Die Evaluatoren (im Namespace `Scoring`) aktualisieren nur die Punkte eines Users. 
  * Für die finanziellen Gewinne sind ausschließlich die Distributoren (z.B. `MainPotDistributor` in `Finance`) zuständig. So geraten Domänen und Logiken nicht durcheinander.
  
* **O – Open/Closed Principle (OCP)** & **D – Dependency Inversion:**
  * Wenn zur WM 2026 eine neue Regel für Sonderpunkte eingeführt wird, muss ich die Klasse `RankingCalculator` eigentlich nicht mehr anfassen. 
  * Der Calculator hält eine Liste von Evaluatoren über das gemeinsames **Interface** `IEvaluator`. 
  * *Beispielcode auf Folie / Handout:* `private readonly List<IEvaluator> _evaluators = [ new ClassicEvaluator(), new BingoEvaluator() ... ];`. Für eine neue Logik muss man einfach nur eine neue Klasse anhängen, die `IEvaluator` implementiert.

* **L – Liskov Substitution Principle (LSP):**
  * Subklassen müssen sich verhalten wie ihre Basis. Ich habe eine abstrakte Basisklasse `EvaluatorBase` definiert, von der z.B. der `KnockoutEvaluator` ableitet. Der Code in der Schleife des `RankingCalculators` iteriert einfach über alle Objekte desselben Interfaces oder derselben Basisklasse. 

* **I – Interface Segregation Principle (ISP):**
  * Das genutzte Interface `IEvaluator` ist absichtlich sehr klein und schlank gehalten. Es enthält nur genau eine Methode: `void Evaluate(User user, TournamentData data)`. Es erzwingt keinen unnötigen Methodenballast für die implementierenden Klassen.

---

## 4. Fokusthema 2: Collections & LINQ in C# (ca. 2 Minuten)

*(Wichtiges C# Kernthema - hier zeigst du Sprachbeherrschung!)*

Ein großer Vorteil von C# gegenüber anderen Sprachen ist das mächtige Handling von Collections durch Generics und LINQ (Language Integrated Query).

* **Typisierte Listen (`List<T>`) und Mengenstrukturen (`HashSet<T>`):**
  * In der Auswertung müssen teilweise riesige Mengen an Arrays (z. B. Bingo-Events vs. geschehene Spiel-Ereignisse) abgeglichen werden.
  * Im `BingoEvaluator` konvertiere ich deshalb die aktuell aufgetretenen Events bewusst in ein `HashSet<string>`. Das reduziert die Zeitkomplexität (O(1) anstelle von O(n)), da intern ein Hash-Table genutzt wird, was die Abfrage `eventSet.Contains(eventId)` massiv beschleunigt.
  * String-Vergleiche geschehen performant und fehlerfrei über `StringComparer.OrdinalIgnoreCase`.

* **Power von LINQ:**
  * Collections werden bei mir selten über simple for-Schleifen verarbeitet. Die Semantik von LINQ macht den Code viel lesbarer.
  * *Beispiel:* Um im Bingo-Feld zu prüfen, ob der Spieler eine vollständige Linie hat, wird elegant verkettet:
    `card.Cells.Where(c => c.IsFulfilled).Select(c => c.Position)`. 
  * Ich nutze das methodenbasierte LINQ massiv für Aggregationen (z.B. `.Sum()`), Filterungen (`.Where()`) und als Tie-Breaker-Sortierung. Im `RankingCalculator` wird im Fall eines Punktegleichstandes automatisch nach KO-Punkten, dann Bingo-Linienerfolgen bewertet.

---

## 5. Fokusthema 3: Persistenz mit JSON (ca. 1 Minute)

*(Bezug zu BANF3: Datei-Speicherung)*

* Anstatt die Daten fest im Code ("hartcodiert") zu halten, verfügt das Modul über eine vollständige Persistenzschicht im `Infrastructure` Namespace (`JsonDataStore`).
* Ich nutze den hochperformanten, nativen C#-Namensraum `System.Text.Json`. 
* Die JSON-Serializer-Einstellungen sind so definiert, dass sie eine gewisse Fehlertoleranz aufweisen (`PropertyNameCaseInsensitive = true`).
* Dadurch liest das Programm den kompletten User-Wettstand (der offline z.B. aus einem Formular generiert wurde) und die sich täglich verändernden Turnier-Daten als Dateien aus dem Dateisystem ein und generiert am Ende eine neue Export-Datei `ranking_current.json`.

---

## 6. Fazit & Ausblick (ca. 1 Minute)

* Abschließend lässt sich sagen, dass das Modul die Prüfungsanforderungen durch sauberes C#, strikte SOLID-Prinzipien und eine Dateipersistenz erfüllt, wobei der Architekturansatz durch die Auslegung auf echte Benutzer und Web-Technologien den funktionalen Anforderungen der Praxis gerecht wird.
* **Weitere Schritte bis zum Start der WM 2026:** 
  1. **Deployment & Datenbank:** Die vollständige Bereitstellung (Webapp auf Azure deployen) und der Umzug der Persistenz von `JsonDataStore` auf eine echte Datenbank auf Azure (über Entity Framework Core).
  2. **Feature für Ältere:** Die Entwicklung einer Funktion zum einfachen Ausdrucken der Tipps und Bingokarten als PDF speziell für ältere Mitspieler, die Offline teilnehmen möchten.
  3. **Frontend:** Ein möglichst benutzerfreundliches, interaktives Frontend (React/Vue/HTML), um die Komplexität der Punkteberechnung leicht verständlich als Dashboard für die Tipper darzustellen.
  
* "Vielen Dank für Ihre Aufmerksamkeit."