using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    [HideReferenceObjectPicker]
    [Serializable]
    public class QuestStatus_Runtime
    {
        [ShowInInspector]
        public QuestSO QuestSO { get; }

        [ShowInInspector]
        public QuestCompletionState CompletionState { get; private set; }

        [ShowInInspector]
        public List<QuestRequirementEntry> Requirements => QuestSO.questRequirements;

        public QuestStatus_Runtime(QuestSO questSO)
        {
            QuestSO = questSO;
            CompletionState = HasRequirements(questSO)
                ? QuestCompletionState.NotStarted
                : QuestCompletionState.CanBeTaken;
        }

        public void UpdateRequirementStatus(IReadOnlyDictionary<string, QuestRequirementParameter> trackedRequirements)
        {
            if (CompletionState == QuestCompletionState.InProgress ||
                CompletionState == QuestCompletionState.Completed)
                return;

            if (!HasRequirements(QuestSO))
            {
                CompletionState = QuestCompletionState.CanBeTaken;
                return;
            }

            foreach (var entry in QuestSO.questRequirements)
            {
                if (entry?.requirementEventSO == null)
                    continue;

                var requirementId = entry.requirementEventSO.RequirementId;
                if (string.IsNullOrEmpty(requirementId) ||
                    !trackedRequirements.TryGetValue(requirementId, out var current))
                {
                    CompletionState = QuestCompletionState.NotStarted;
                    return;
                }

                if (current.floatParam < entry.requiredFloat ||
                    current.boolParam != entry.requiredBool)
                {
                    CompletionState = QuestCompletionState.NotStarted;
                    return;
                }
            }

            CompletionState = QuestCompletionState.CanBeTaken;
        }

        public void SetInProgress()
        {
            CompletionState = QuestCompletionState.InProgress;
        }

        public void SetCompleted()
        {
            CompletionState = QuestCompletionState.Completed;
        }

        static bool HasRequirements(QuestSO questSO)
        {
            if (questSO?.questRequirements == null || questSO.questRequirements.Count == 0)
                return false;

            foreach (var entry in questSO.questRequirements)
            {
                if (entry?.requirementEventSO != null)
                    return true;
            }

            return false;
        }
    }
}
