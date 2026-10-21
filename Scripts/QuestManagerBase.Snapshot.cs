using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace RAXY.Quest
{
    public abstract partial class QuestManagerBase
    {
        public event Action OnSnapshotRestored;

        public QuestSnapshot CaptureSnapshot()
        {
            var snapshot = new QuestSnapshot { trackedQuestId = trackedQuest };

            foreach (var pair in TrackedRequirementDict)
            {
                snapshot.requirements.Add(new QuestRequirementSnapshot
                {
                    requirementId = pair.Key,
                    value = CopyRequirement(pair.Value)
                });
            }

            foreach (var pair in QuestStatusDict)
            {
                if (pair.Value.CompletionState == QuestCompletionState.Completed)
                    snapshot.completedQuestIds.Add(pair.Key);
            }

            foreach (var pair in ActiveQuests)
            {
                var quest = pair.Value;
                var record = new QuestRuntimeSnapshot
                {
                    questId = pair.Key,
                    definitionSignature = DefinitionSignature(quest.QuestData),
                    currentStepIndex = quest.CurrentQuestStepIndex
                };
                foreach (var step in quest.QuestSteps_Runtime)
                {
                    var stepRecord = new QuestStepSnapshot
                    {
                        state = step.State,
                        activeMainObjectiveIndex = step.ActiveMainObjectiveIndex
                    };
                    foreach (var objective in step.MainObjectives)
                        stepRecord.mainObjectives.Add(CaptureObjective(objective));
                    foreach (var objective in step.OptionalObjectives)
                        stepRecord.optionalObjectives.Add(CaptureObjective(objective));
                    record.steps.Add(stepRecord);
                }
                snapshot.activeQuests.Add(record);
            }

            return snapshot;
        }

        public bool RestoreSnapshot(QuestSnapshot snapshot)
        {
            if (QuestDatabase?.Quests == null || snapshot == null || snapshot.version != 1 ||
                snapshot.completedQuestIds == null || snapshot.requirements == null ||
                snapshot.activeQuests == null)
            {
                Debug.LogWarning("[QuestManager] Invalid or unsupported quest snapshot.");
                return false;
            }

            var requirements = new Dictionary<string, QuestRequirementParameter>();
            foreach (var record in snapshot.requirements)
            {
                if (record == null || string.IsNullOrWhiteSpace(record.requirementId) ||
                    requirements.ContainsKey(record.requirementId) || !IsFinite(record.value.floatParam))
                    return RejectSnapshot("Invalid requirement entry.");
                requirements.Add(record.requirementId, CopyRequirement(record.value));
            }

            var completed = new HashSet<string>();
            foreach (var id in snapshot.completedQuestIds)
            {
                if (string.IsNullOrWhiteSpace(id) || !completed.Add(id))
                    return RejectSnapshot("Invalid completed quest ID.");
            }

            var statuses = new Dictionary<string, QuestStatus_Runtime>();
            foreach (var questData in QuestDatabase.Quests)
            {
                if (questData == null)
                    continue;
                if (statuses.ContainsKey(questData.QuestId))
                    return RejectSnapshot($"Duplicate quest definition '{questData.QuestId}'.");
                var status = new QuestStatus_Runtime(questData);
                status.UpdateRequirementStatus(requirements);
                statuses.Add(questData.QuestId, status);
            }

            foreach (var id in completed)
            {
                if (statuses.TryGetValue(id, out var status))
                    status.RestoreState(QuestCompletionState.Completed);
                else
                    Debug.LogWarning($"[QuestManager] Skipping unknown completed quest '{id}'.");
            }

            var active = new Dictionary<string, Quest_Runtime>();
            var seenActive = new HashSet<string>();
            foreach (var record in snapshot.activeQuests)
            {
                if (record == null || string.IsNullOrWhiteSpace(record.questId) ||
                    !seenActive.Add(record.questId) || completed.Contains(record.questId))
                    return RejectSnapshot("Invalid or duplicate active quest ID.");

                if (!statuses.TryGetValue(record.questId, out var status))
                {
                    Debug.LogWarning($"[QuestManager] Skipping unknown active quest '{record.questId}'.");
                    continue;
                }

                var questData = status.QuestSO;
                if (!string.Equals(record.definitionSignature, DefinitionSignature(questData), StringComparison.Ordinal))
                {
                    Debug.LogWarning($"[QuestManager] Skipping changed quest definition '{record.questId}'.");
                    continue;
                }

                var runtime = new Quest_Runtime(questData);
                if (!ValidRuntimeRecord(record, runtime))
                    return RejectSnapshot($"Invalid progress for quest '{record.questId}'.");

                runtime.RestoreState(record);
                active.Add(record.questId, runtime);
                status.RestoreState(QuestCompletionState.InProgress);
            }

            if (ActiveQuests != null)
                foreach (var runtime in ActiveQuests.Values)
                    runtime?.Deactivate();

            TrackedRequirementDict = requirements;
            QuestStatusDict = statuses;
            ActiveQuests = active;
            trackedQuest = !string.IsNullOrEmpty(snapshot.trackedQuestId) && active.ContainsKey(snapshot.trackedQuestId)
                ? snapshot.trackedQuestId
                : string.Empty;

            foreach (var pair in active)
            {
                WireQuestRuntime(pair.Key, pair.Value.QuestData, pair.Value);
                pair.Value.ResumeSubscriptions();
            }

            RefreshQuestObjects();
            OnSnapshotRestored?.Invoke();
            return true;
        }

        static QuestObjectiveSnapshot CaptureObjective(QuestStepObjective_Runtime objective) =>
            new QuestObjectiveSnapshot { state = objective.State, currentAmount = objective.CurrentAmount };

        static QuestRequirementParameter CopyRequirement(QuestRequirementParameter value) =>
            new QuestRequirementParameter
            {
                floatParam = value.floatParam,
                boolParam = value.boolParam,
                listStringParam = value.listStringParam == null ? null : new List<string>(value.listStringParam)
            };

        static bool RejectSnapshot(string reason)
        {
            Debug.LogWarning($"[QuestManager] Quest snapshot rejected: {reason}");
            return false;
        }

        static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        static bool ValidState(QuestCompletionState state) =>
            state == QuestCompletionState.NotStarted || state == QuestCompletionState.InProgress ||
            state == QuestCompletionState.Completed;

        static bool ValidRuntimeRecord(QuestRuntimeSnapshot record, Quest_Runtime runtime)
        {
            if (record.steps == null || record.steps.Count != runtime.QuestSteps_Runtime.Count ||
                record.currentStepIndex < -1 || record.currentStepIndex >= record.steps.Count)
                return false;
            if (record.currentStepIndex == -1 && runtime.QuestData.autoComplete)
                return false;

            for (int i = 0; i < record.steps.Count; i++)
            {
                var saved = record.steps[i];
                var current = runtime.QuestSteps_Runtime[i];
                if (saved == null || !ValidState(saved.state) || saved.mainObjectives == null ||
                    saved.optionalObjectives == null || saved.mainObjectives.Count != current.MainObjectives.Count ||
                    saved.optionalObjectives.Count != current.OptionalObjectives.Count ||
                    saved.activeMainObjectiveIndex < -1 ||
                    saved.activeMainObjectiveIndex >= current.MainObjectives.Count)
                    return false;
                if (i == record.currentStepIndex && saved.state != QuestCompletionState.InProgress)
                    return false;
                if (i < record.currentStepIndex && saved.state != QuestCompletionState.Completed)
                    return false;
                if (record.currentStepIndex == -1 && saved.state != QuestCompletionState.Completed)
                    return false;

                for (int j = 0; j < saved.mainObjectives.Count; j++)
                    if (!ValidObjective(saved.mainObjectives[j], current.MainObjectives[j])) return false;
                for (int j = 0; j < saved.optionalObjectives.Count; j++)
                    if (!ValidObjective(saved.optionalObjectives[j], current.OptionalObjectives[j])) return false;
            }
            return true;
        }

        static bool ValidObjective(QuestObjectiveSnapshot saved, QuestStepObjective_Runtime current) =>
            saved != null && ValidState(saved.state) && IsFinite(saved.currentAmount) &&
            saved.currentAmount >= 0f &&
            (current.RequiredAmount <= 0f || saved.currentAmount <= current.RequiredAmount);

        static string DefinitionSignature(QuestSO quest)
        {
            var result = new StringBuilder();
            AppendPart(result, quest.QuestId);
            AppendPart(result, quest.autoComplete ? "1" : "0");
            if (quest.questSteps == null)
                return result.ToString();
            foreach (var step in quest.questSteps)
            {
                if (step == null) continue;
                AppendPart(result, step.stepMode.ToString());
                AppendObjectives(result, step.mainObjectives);
                AppendObjectives(result, step.optionalObjectives);
            }
            return result.ToString();
        }

        static void AppendObjectives(StringBuilder result, List<QuestStepObjective> objectives)
        {
            int count = 0;
            if (objectives != null)
                foreach (var objective in objectives)
                    if (objective != null) count++;
            AppendPart(result, count.ToString(CultureInfo.InvariantCulture));
            if (objectives == null) return;
            foreach (var objective in objectives)
            {
                if (objective == null) continue;
                AppendPart(result, objective.eventSO == null ? "" : objective.eventSO.name);
                AppendPart(result, objective.objectiveParameter);
                AppendPart(result, objective.requiredAmount.ToString("R", CultureInfo.InvariantCulture));
            }
        }

        static void AppendPart(StringBuilder result, string value)
        {
            value ??= string.Empty;
            result.Append(value.Length).Append(':').Append(value);
        }

        void WireQuestRuntime(string questId, QuestSO questData, Quest_Runtime runtime)
        {
            runtime.OnStepChanged += stepIndex =>
            {
                OnQuestStepChanged?.Invoke(questId, stepIndex);
                Process_StepEnterActions(questId, questData, stepIndex);
                RefreshQuestObjects();
            };
            runtime.OnStepCompleted += stepIndex => Process_StepCompleteActions(questId, questData, stepIndex);
            runtime.OnObjectiveProgressed += objective =>
            {
                OnObjectiveProgressed?.Invoke(objective);
                RefreshQuestObjects();
            };
            runtime.OnObjectiveCompleted += objective =>
            {
                OnObjectiveCompleted?.Invoke(objective);
                RefreshQuestObjects();
            };
            runtime.OnQuestCompleted += () => QuestCompletedHandler(questId);
        }
    }
}
