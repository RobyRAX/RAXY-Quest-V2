# RAXY Quest 2

RAXY Quest 2 provides an event-driven quest foundation for Unity projects: quest data, requirement tracking, step/objective runtime progression, and a quest manager base.

## Features

- **QuestSO / QuestDatabaseSO** — quest definitions with requirements, steps, and localized text
- **QuestStep / QuestStepObjective** — step data with Sequential/Parallel modes, main and optional objectives
- **QuestRequirementEventSO / QuestStepObjectiveEventSO** — `EventSO`-based channels for requirement updates and objective progress
- **Quest_Runtime / QuestStep_Runtime / QuestStepObjective_Runtime** — live play-mode wrappers that track progression and completion
- **QuestStatus_Runtime** — per-quest availability state (`NotStarted` → `CanBeTaken` → `InProgress` → `Completed`)
- **QuestManagerBase** — abstract orchestrator: requirement tracking, take/complete quests, quest events

## Setup

1. Create a concrete manager extending `QuestManagerBase` (see `Scripts/Example/QuestManager.cs`).
2. Create a `QuestDatabaseSO` asset and fill it with `QuestSO` assets.
3. Create `QuestRequirementEventSO` and `QuestStepObjectiveEventSO` assets and wire them into quest data.
4. Call `InitAllQuest()` at bootstrap, then `TakeQuest(questId)` when starting quests.
5. Raise objective events from gameplay code (kill, collect, talk, etc.) to advance progress.

## Dependencies

- **RAXY Event** (`com.raxy.event`) — `EventSO<T>` channels for requirements and objectives
- **RAXY Localization** (`com.raxy.utility.localization`) — localized quest and objective text via `StringProvider`
- **UniTask** (`com.cysharp.unitask`) — async localization cache refresh
- **Odin Inspector** (project plugin) — editor attributes and runtime inspector coloring

## Notes

Game-specific objective raising, quest UI, save/load, and bootstrap wiring should live in your project, not in this package. `Scripts/Example/` contains a concrete manager and a smoke-test component for manual testing.
