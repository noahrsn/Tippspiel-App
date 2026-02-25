# 🤖 Agent Instructions: WM 2026 Tippspiel & Bingo Backend

This document provides comprehensive instructions and context for AI agents (like GitHub Copilot) working on the "WM 2026 Tippspiel & Bingo" project. It outlines the project's purpose, architecture, rules, and coding guidelines to ensure consistent and accurate contributions.

---

## 1. Project Overview

This project is a C# .NET 10 backend application designed to manage a complex prediction game (Tippspiel) combined with a Bingo component for the 2026 FIFA World Cup. It is intended for a real-world group of approximately 200 participants.

**Core Objective:** Develop a robust "Calculation Engine" that processes JSON-based user predictions and match data to calculate points, rankings, and complex financial payouts based on a custom rule set.

**Key Documents:**
*   `Documentation/RULES.md`: The definitive source of truth for all game rules, point allocations, and prize distributions. **Always consult this file when implementing scoring or finance logic.**
*   `Documentation/EXPOSÈ.md`: Outlines the project's motivation, architecture, and milestones.
*   `README.md`: General project overview and setup instructions.

---

## 2. Architecture & Design Principles

The application follows a clean, layered architecture. Each layer has exactly one responsibility. Business logic is strictly separated from I/O, routing and domain data.

### 2.1 Layer Overview

```
src/
  Program.cs             ← 4 lines – entry point only

  Domain/                ← Pure data classes (DTOs), no logic
    Tournament.cs        ← TournamentData, MatchResult, TeamInfo, BingoEventInfo
    User.cs              ← User, UserBet, MatchBet, SpecialBet
    BingoCard.cs         ← BingoCard, BingoCell
    ScoreSnapshot.cs     ← Basis-Punktestand (wird von RankingEntry geerbt)
    RankingEntry.cs      ← RankingEntry : ScoreSnapshot
    RankingReport.cs     ← RankingReport, FinanceSummary
    PotResults.cs        ← GroupClusterResult, BingoPotResult, PotOverviewEntry

  Scoring/               ← Stateless point evaluators
    IEvaluator.cs        ← interface: Evaluate(User, TournamentData)
    EvaluatorBase.cs     ← abstract: CalculateMatchPoints() (shared utility)
    ClassicEvaluator.cs  ← : EvaluatorBase – Gruppenspiel-Punkte
    KnockoutEvaluator.cs ← : EvaluatorBase – KO-Runden-Punkte
    SpecialBetEvaluator.cs ← : EvaluatorBase – Weltmeister/Torschütze
    BingoBase.cs         ← abstract: Lines[] (4×4, 10 Linien), Zeitberechnungen (public static)
    BingoEvaluator.cs    ← : BingoBase, IEvaluator – Bingo-Punkte

  Finance/               ← Prize money distributors
    PotMath.cs           ← static: alle Topf-Formeln (TotalPot, BingoPot, …)
    PrizeDistributorBase.cs ← abstract: AddWin() helper
    ClusterDistributor.cs ← : PrizeDistributorBase
    BingoDistributor.cs  ← : PrizeDistributorBase (nutzt BingoBase statisch)
    MainPotDistributor.cs ← : PrizeDistributorBase

  Application/           ← Orchestration & HTTP routing, no I/O
    RankingCalculator.cs ← Run(): Scores → sort → finance → report
    WebServer.cs         ← alle ASP.NET Minimal-API-Routen

  Infrastructure/        ← File I/O & JSON serialization
    JsonDataStore.cs     ← LoadUsers(), LoadTournamentData(), ExportRanking()
    UserTemplateBuilder.cs ← CreateEmpty(TournamentData)
```

### 2.2 Inheritance Hierarchy

| Base | Derived classes |
|---|---|
| `ScoreSnapshot` | `RankingEntry` |
| `EvaluatorBase : IEvaluator` | `ClassicEvaluator`, `KnockoutEvaluator`, `SpecialBetEvaluator` |
| `BingoBase` | `BingoEvaluator` |
| `PrizeDistributorBase` | `ClusterDistributor`, `BingoDistributor`, `MainPotDistributor` |

`BingoDistributor` does **not** inherit `BingoBase` (C# single inheritance). Instead, `BingoBase` exposes its time-helper methods as `public static`, allowing `BingoDistributor` to call them directly.

### 2.3 Design Guidelines

*   **Stateless evaluators:** every `IEvaluator` receives data through parameters and writes results to `user.CurrentScore`. No instance state.
*   **Single Responsibility:** `ClassicEvaluator` only calculates match points; `SpecialBetEvaluator` only calculates special bet points. `RankingCalculator` runs them in sequence.
*   **Namespace = Layer:** `TippspielApp.Domain`, `TippspielApp.Scoring`, `TippspielApp.Finance`, `TippspielApp.Application`, `TippspielApp.Infrastructure`.
*   **JSON First:** `System.Text.Json` config lives only in `JsonDataStore`. Property names in Domain classes must not change (they define the JSON contract).
*   **Console mode removed:** The application starts as a web server only (`WebServer.Run(args)`).

---

## 3. Critical Business Rules (The "Gotchas")

When implementing logic, pay special attention to these specific rules defined in `RULES.md`:

### 3.1 Classic Betting (`ClassicEvaluator`, `KnockoutEvaluator`, `SpecialBetEvaluator`)
*   **Match Points:** Exact (4 pts), Goal Difference/Draw (3 pts), Tendency (2 pts), Wrong (0 pts).
*   **KO Stage:** Users predict teams reaching specific rounds *independently* of the actual tournament tree. A team can only score *once* per round.
*   **Special Bets:** World Champion (20 pts), Top Scorer (20 pts). *Tie-breaker for Top Scorer: The first named player counts.*

### 3.2 Bingo (`BingoEvaluator`, `BingoBase`)
*   **Grid:** 4×4 (16 cells, positions 0–15). There is **no FREE field** – all 16 cells are event fields.
    *   **Event Triggering:** An event is fulfilled if it happens *at least once* during the tournament.
    *   **Points:** 3 pts per fulfilled field. Line points are tiered: 1st completed line = 10 pts, 2nd = 6 pts, 3rd = 4 pts, further lines = 0 pts. *Note: The 20 pts for a full board were removed.*

### 3.3 Finance & Payouts (`PotMath`, `ClusterDistributor`, `BingoDistributor`, `MainPotDistributor`)
*   **Total Pool:** 1.800 € (1.100 € Overall, 300 € Group Stages, 400 € Bingo).
*   **Group Stage Clusters:** 6 clusters (A+B, C+D, E+F, G+H, I+J, K+L). 50 € to the user with the most points *from match predictions only* within that specific cluster.
*   **Bingo Prizes:** 1st Line (100 €), Next 4 Lines (50 € each), Best Bingo Player / Most Fields (100 €). *Note: The prize for a full bingo board was replaced by "Best Bingo Player".*
*   **Overall Ranking:** Payouts for Top 20 (1st: 300 €, 2nd: 200 €, 3rd: 100 €, 4th-10th: 50 €, 11th-20th: 15 €).
*   **Tie-Breakers:** If points are equal, the order is: 1. More KO stage points -> 2. More fulfilled Bingo fields -> 3. Random draw (Losentscheid).

---

## 4. Development Workflow & Tasks

When asked to implement a feature or fix a bug, follow this general workflow:

1.  **Understand the Goal:** Read the user prompt carefully. Identify which layer (`Domain`, `Scoring`, `Finance`, `Application`, `Infrastructure`) needs modification.
2.  **Consult the Rules:** Verify the requested change against `Documentation/RULES.md`. If there's a discrepancy, ask the user for clarification or follow the explicit instructions in the prompt if they override the rules.
3.  **Locate the Code:** Use file search or semantic search to find the relevant classes and methods.
4.  **Implement the Change:**
    *   Write clean, idiomatic C# 10 code.
    *   Use LINQ where appropriate for data manipulation.
    *   Ensure proper error handling (e.g., missing JSON files, malformed data).
5.  **Verify:** Mentally trace the logic or suggest writing a unit test to ensure the calculation is correct according to the rules.

### Common Agent Tasks:
*   **Scaffolding:** Creating the initial class structures based on the `EXPOSÈ.md` and `RULES.md`.
*   **Implementing Evaluators:** Writing the complex logic in `ClassicEvaluator`, `KnockoutEvaluator`, `SpecialBetEvaluator`, `BingoEvaluator`, and the Finance distributors.
*   **JSON Serialization:** Setting up the `System.Text.Json` attributes and configurations to correctly parse the input files.
*   **Debugging:** Finding logical errors in the point calculation or prize distribution.

---

## 5. Code Style & Conventions

*   **Language:** C# 10 (.NET 10).
*   **Naming:** PascalCase for classes, methods, and properties. camelCase for local variables and parameters.
*   **Comments:** Use XML documentation comments (`///`) for public classes and methods, especially in the Services layer to explain the calculation logic.
*   **File Encoding:** UTF-8.
*   **Language (Text):** The project documentation and domain terms are in German (e.g., `Torschützenkönig`, `Gruppenphase`). Use English for code elements (classes, variables, methods) but German for user-facing strings or specific domain concepts if translation is ambiguous.

---
*End of Agent Instructions*