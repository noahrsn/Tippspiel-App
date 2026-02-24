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

The application follows a clean, modular architecture separating data models, business logic, and data access.

### 2.1 Core Components

*   **Models (`src/Models/`):** Pure data transfer objects (DTOs). They should contain minimal to no business logic.
    *   `User.cs`: Represents a participant, their bets, bingo card, current score, and winnings.
    *   `Tournament.cs`: Represents the real-world tournament data (matches, results, events).
    *   `Bets.cs`: Represents the user's predictions (match scores, KO stage teams, special bets).
    *   `Bingo.cs`: Represents the 5x5 Bingo card and the catalog of possible events.
    *   `Ranking.cs`: Represents the calculated leaderboard and financial status.
*   **Services (`src/Services/`):** The core business logic.
    *   `CalculationEngine.cs`: The orchestrator. It coordinates the evaluation of bets, bingo, and finances.
    *   `ClassicBetEvaluator.cs`: Calculates points for match predictions, KO stages, and special bets.
    *   `BingoEvaluator.cs`: Checks real-world events against user bingo cards, updates card status, and calculates bingo points.
    *   `FinanceCalculator.cs`: Manages the prize pool (1.800 €), calculates group-stage intermediate winnings, bingo prizes, and the final leaderboard payouts.
    *   `DataHandler.cs`: Handles reading from and writing to JSON files.
*   **Data (`Data/`):**
    *   `Input/users.json`: The source of truth for user predictions.
    *   `Input/tournament_data.json`: The source of truth for real-world tournament results and events.
    *   `Output/ranking_current.json`: The generated output file containing the current standings.

### 2.2 Design Guidelines

*   **Stateless Services:** Services should ideally be stateless. Pass the necessary data (Users, Tournament data) into the evaluation methods.
*   **Immutability (where practical):** Treat input data (`users.json`, `tournament_data.json`) as read-only during the calculation phase. Generate a new `Ranking` object as output.
*   **Testability:** The logic must be highly testable. The separation of concerns (Evaluators vs. DataHandler) is crucial for unit testing the calculation logic with mock data.
*   **JSON First:** The system relies heavily on JSON for input and output. Ensure robust serialization and deserialization using `System.Text.Json`.

---

## 3. Critical Business Rules (The "Gotchas")

When implementing logic, pay special attention to these specific rules defined in `RULES.md`:

### 3.1 Classic Betting (`ClassicBetEvaluator`)
*   **Match Points:** Exact (4 pts), Goal Difference/Draw (3 pts), Tendency (2 pts), Wrong (0 pts).
*   **KO Stage:** Users predict teams reaching specific rounds *independently* of the actual tournament tree. A team can only score *once* per round.
*   **Special Bets:** World Champion (20 pts), Top Scorer (20 pts). *Tie-breaker for Top Scorer: The first named player counts.*

### 3.2 Bingo (`BingoEvaluator`)
*   **The Free Space:** The center square (index 12 in a 0-indexed 1D array, or [2,2] in a 2D array) is a "FREE" space and is *always* considered fulfilled.
*   **Event Triggering:** An event is fulfilled if it happens *at least once* during the tournament.
*   **Points:** 2 pts per fulfilled field, 8 pts per completed line (horizontal, vertical, diagonal). *Note: The 20 pts for a full board were removed.*

### 3.3 Finance & Payouts (`FinanceCalculator`)
*   **Total Pool:** 1.800 € (1.100 € Overall, 300 € Group Stages, 400 € Bingo).
*   **Group Stage Clusters:** 6 clusters (A+B, C+D, E+F, G+H, I+J, K+L). 50 € to the user with the most points *from match predictions only* within that specific cluster.
*   **Bingo Prizes:** 1st Line (100 €), Next 4 Lines (50 € each), Best Bingo Player / Most Fields (100 €). *Note: The prize for a full bingo board was replaced by "Best Bingo Player".*
*   **Overall Ranking:** Payouts for Top 20 (1st: 300 €, 2nd: 200 €, 3rd: 100 €, 4th-10th: 50 €, 11th-20th: 15 €).
*   **Tie-Breakers:** If points are equal, the order is: 1. More KO stage points -> 2. More fulfilled Bingo fields -> 3. Random draw (Losentscheid).

---

## 4. Development Workflow & Tasks

When asked to implement a feature or fix a bug, follow this general workflow:

1.  **Understand the Goal:** Read the user prompt carefully. Identify which component (Models, Evaluators, DataHandler) needs modification.
2.  **Consult the Rules:** Verify the requested change against `Documentation/RULES.md`. If there's a discrepancy, ask the user for clarification or follow the explicit instructions in the prompt if they override the rules.
3.  **Locate the Code:** Use file search or semantic search to find the relevant classes and methods.
4.  **Implement the Change:**
    *   Write clean, idiomatic C# 10 code.
    *   Use LINQ where appropriate for data manipulation.
    *   Ensure proper error handling (e.g., missing JSON files, malformed data).
5.  **Verify:** Mentally trace the logic or suggest writing a unit test to ensure the calculation is correct according to the rules.

### Common Agent Tasks:
*   **Scaffolding:** Creating the initial class structures based on the `EXPOSÈ.md` and `RULES.md`.
*   **Implementing Evaluators:** Writing the complex logic in `ClassicBetEvaluator`, `BingoEvaluator`, and `FinanceCalculator`.
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