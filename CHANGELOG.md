# Changelog

## [1.1.0] - CompleteQuest action + autoComplete

- Added `CompleteQuest` action (optional `QuestSO`; falls back to context quest id).
- Added `QuestSO.autoComplete` (default `true`) to control whether the quest completes automatically after all steps finish.
- Added per-step `actions_OnComplete` (runs when a step finishes, before advancing / quest complete).

## [1.0.0] - Initial release

- Event-driven quest system: quest data (`QuestSO`, `QuestDatabaseSO`), requirement and objective event channels, runtime progression wrappers, and `QuestManagerBase` orchestrator.
