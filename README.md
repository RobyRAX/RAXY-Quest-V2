# RAXY Quest 2

RAXY Quest 2 provides an event-driven quest foundation for Unity projects: quest data, requirement tracking, step/objective runtime progression, and a quest manager base.

## Features

- **QuestSO / QuestDatabaseExampleSO** — quest definitions with requirements, steps, localized text, and `autoComplete` (complete after all steps, default on)
- **QuestAction** — `TakeQuest`, `CompleteQuest`, `TriggerEventSO` for OnTaken / OnEnter / OnComplete / OnCompleted hooks
- **QuestStep / QuestStepObjective** — step data with Sequential/Parallel modes, `actions_OnEnter` / `actions_OnComplete`, main and optional objectives
- **QuestRequirementEventSO / QuestStepObjectiveEventSO** — `EventSO`-based channels for requirement updates and objective progress
- **Quest_Runtime / QuestStep_Runtime / QuestStepObjective_Runtime** — live play-mode wrappers that track progression and completion
- **QuestStatus_Runtime** — per-quest availability state (`NotStarted` → `CanBeTaken` → `InProgress` → `Completed`)
- **QuestManagerBase** — abstract orchestrator: requirement tracking, take/complete quests, quest events

## Setup

1. Create a concrete manager extending `QuestManagerBase` (see `Scripts/Example/QuestManagerExample.cs`).
2. Create a `QuestDatabaseExampleSO` asset and fill it with `QuestSO` assets.
3. Create `QuestRequirementEventSO` and `QuestStepObjectiveEventSO` assets and wire them into quest data.
4. Call `InitQuestManager()` at bootstrap, then `TakeQuest(questId)` when starting quests.
5. Raise objective events from gameplay code (kill, collect, talk, etc.) to advance progress.

## Samples

Starter manager, demo quests, EventSOs, smoke test, and a minimal tracker UI ship as a Package Manager sample (not auto-imported).

1. Open **Window → Package Manager**.
2. Select **RAXY Quest 2**.
3. Under **Samples**, click **Import** on **Basic Setup**.
4. Unity copies the sample into `Assets/Samples/RAXY Quest 2/<version>/Basic Setup/`.
5. Drop **Quest Manager** and **Quest Tracker** into a scene, enter Play Mode, then use Inspector buttons:
   - `TakeQuest("Demo Quest A")` on the manager
   - `RaiseObjectiveEvent` on `QuestSmokeTest` (Talk / Guide) to finish A
   - Swap smoke-test objective to **Progress Objective** (`Evidence`, amount `1`) and raise three times for B — or take B after A unlocks via the completed-requirement raise

Sample scripts (`SampleQuestManager`, tracker UI) live under the imported Samples folder (`Assembly-CSharp`), not in the package runtime assembly.

## Dependencies

- **RAXY Event** (`com.raxy.event`) — `EventSO<T>` channels for requirements and objectives
- **RAXY Localization** (`com.raxy.utility.localization`) — localized quest and objective text via `StringProvider`
- **UniTask** (`com.cysharp.unitask`) — async localization cache refresh
- **Odin Inspector** (project plugin) — editor attributes and runtime inspector coloring
- **TextMeshPro** — required by the sample tracker UI after Import

## Notes

Game-specific objective raising, save/load, and bootstrap wiring should live in your project. `Scripts/Example/` contains a concrete manager and a smoke-test component for manual testing. Optional starter content is available via **Samples → Basic Setup**.
