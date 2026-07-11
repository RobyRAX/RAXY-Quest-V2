using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    [HideReferenceObjectPicker]
    [Serializable]
    public class Quest_Runtime
    {
        const int AllStepsCompletedIndex = -1;

        [ShowInInspector]
        public QuestSO QuestData { get; }

        [ListDrawerSettings(
            ShowIndexLabels = true,
            ListElementLabelName = "StepName",
            ShowFoldout = true,
            Expanded = true,
            HideAddButton = true,
            HideRemoveButton = true,
            ElementColor = "GetQuestStepListElementColor")]
        [ShowInInspector]
        public List<QuestStep_Runtime> QuestSteps_Runtime { get; }

        [ShowInInspector]
        public int CurrentQuestStepIndex { get; private set; } = -1;

        [ShowInInspector]
        public QuestCompletionState State { get; private set; }

        public QuestStep_Runtime ActiveStep =>
            CurrentQuestStepIndex >= 0 && CurrentQuestStepIndex < QuestSteps_Runtime.Count
                ? QuestSteps_Runtime[CurrentQuestStepIndex]
                : null;

        public event Action<int> OnStepChanged;
        public event Action<QuestStepObjective_Runtime> OnObjectiveProgressed;
        public event Action<QuestStepObjective_Runtime> OnObjectiveCompleted;
        public event Action OnQuestCompleted;

        public Quest_Runtime(QuestSO data)
        {
            QuestData = data;
            QuestSteps_Runtime = new List<QuestStep_Runtime>();

            if (data?.questSteps == null)
                return;

            foreach (var step in data.questSteps)
            {
                if (step == null)
                    continue;

                QuestSteps_Runtime.Add(new QuestStep_Runtime(step));
            }

            State = QuestCompletionState.NotStarted;
        }

        public void ActivateStep(int index)
        {
            if (index < 0 || index >= QuestSteps_Runtime.Count)
            {
                Debug.LogWarning($"[Quest_Runtime] Invalid step index {index} for quest '{QuestData?.QuestId}'.");
                return;
            }

            ActiveStep?.Deactivate();

            CurrentQuestStepIndex = index;
            State = QuestCompletionState.InProgress;

            var step = QuestSteps_Runtime[index];
            step.OnStepCompleted -= StepCompletedHandler;
            step.OnObjectiveProgressed -= ObjectiveProgressedHandler;
            step.OnObjectiveCompleted -= ObjectiveCompletedHandler;

            step.OnStepCompleted += StepCompletedHandler;
            step.OnObjectiveProgressed += ObjectiveProgressedHandler;
            step.OnObjectiveCompleted += ObjectiveCompletedHandler;

            step.Activate();
            OnStepChanged?.Invoke(index);
        }

        public void CompleteActiveStep()
        {
            if (ActiveStep == null)
                return;

            if (ActiveStep.State != QuestCompletionState.Completed)
                ActiveStep.Deactivate();

            StepCompletedHandler();
        }

        public void Deactivate()
        {
            ActiveStep?.Deactivate();
        }

        void StepCompletedHandler()
        {
            int nextIndex = CurrentQuestStepIndex + 1;
            if (nextIndex < QuestSteps_Runtime.Count)
            {
                ActivateStep(nextIndex);
                return;
            }

            CompleteQuest();
        }

        void CompleteQuest()
        {
            if (State == QuestCompletionState.Completed)
                return;

            Deactivate();
            CurrentQuestStepIndex = AllStepsCompletedIndex;
            State = QuestCompletionState.Completed;
            OnQuestCompleted?.Invoke();
        }

        void ObjectiveProgressedHandler(QuestStepObjective_Runtime objective)
        {
            OnObjectiveProgressed?.Invoke(objective);
        }

        void ObjectiveCompletedHandler(QuestStepObjective_Runtime objective)
        {
            OnObjectiveCompleted?.Invoke(objective);
        }

#if UNITY_EDITOR
        Color GetQuestStepListElementColor(int index, Color defaultColor) =>
            QuestRuntimeInspectorColors.GetQuestStepElementColor(this, index, defaultColor);
#endif
    }
}
