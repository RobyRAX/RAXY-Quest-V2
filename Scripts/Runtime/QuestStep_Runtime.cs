using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    [Serializable]
    public class QuestStep_Runtime
    {
        //[ShowInInspector]
        public QuestStep QuestStepData { get; }

        [ShowInInspector]
        public string StepName => QuestStepData?.StepName;

        [ShowInInspector]
        public QuestStepMode StepMode => QuestStepData.stepMode;

        //[ShowInInspector]
        public int ActiveMainObjectiveIndex { get; private set; } = -1;

        [ShowInInspector]
        public QuestCompletionState State { get; private set; }

        [ListDrawerSettings(
            ShowIndexLabels = true,
            ListElementLabelName = "ObjectiveDescription",
            ShowFoldout = true,
            Expanded = true,
            HideAddButton = true,
            HideRemoveButton = true,
            ElementColor = "GetMainObjectiveListElementColor")]
        [ShowInInspector]
        public List<QuestStepObjective_Runtime> MainObjectives { get; set; }

        [ListDrawerSettings(
            ShowIndexLabels = true,
            ListElementLabelName = "ObjectiveDescription",
            ShowFoldout = true,
            Expanded = true,
            HideAddButton = true,
            HideRemoveButton = true,
            ElementColor = "GetOptionalObjectiveListElementColor")]
        [ShowInInspector]
        public List<QuestStepObjective_Runtime> OptionalObjectives { get; set; }

        public bool AllMainObjectivesDone =>
            MainObjectives.Count > 0 && MainObjectives.All(o => o.IsCompleted);

        public event Action OnStepCompleted;
        public event Action<QuestStepObjective_Runtime> OnObjectiveProgressed;
        public event Action<QuestStepObjective_Runtime> OnObjectiveCompleted;

        public QuestStep_Runtime(QuestStep data)
        {
            QuestStepData = data;
            MainObjectives = BuildObjectives(data?.mainObjectives);
            OptionalObjectives = BuildObjectives(data?.optionalObjectives);
            State = QuestCompletionState.NotStarted;
        }

        static List<QuestStepObjective_Runtime> BuildObjectives(List<QuestStepObjective> objectives)
        {
            var result = new List<QuestStepObjective_Runtime>();
            if (objectives == null)
                return result;

            foreach (var objective in objectives)
            {
                if (objective == null)
                    continue;

                result.Add(new QuestStepObjective_Runtime(objective));
            }

            return result;
        }

        public void Activate()
        {
            if (State == QuestCompletionState.Completed)
                return;

            State = QuestCompletionState.InProgress;

            if (QuestStepData.stepMode == QuestStepMode.Sequential)
                ActivateMainObjective(0);
            else
            {
                for (int i = 0; i < MainObjectives.Count; i++)
                    ActivateMainObjective(i);
            }

            foreach (var objective in OptionalObjectives)
                ActivateObjective(objective);

            if (MainObjectives.Count == 0)
                CompleteStep();
        }

        public void Deactivate()
        {
            foreach (var objective in AllObjectives())
                objective.Deactivate();
        }

        public void ActivateMainObjective(int index)
        {
            if (index < 0 || index >= MainObjectives.Count)
                return;

            var selected = MainObjectives[index];

            if (QuestStepData.stepMode == QuestStepMode.Sequential)
            {
                if (index > 0)
                {
                    var previous = MainObjectives[index - 1];
                    if (!previous.IsCompleted)
                        return;
                }

                if (selected.IsCompleted)
                    return;
            }
            else if (selected.IsCompleted)
            {
                return;
            }

            ActiveMainObjectiveIndex = index;
            ActivateObjective(selected, isMainObjective: true);
        }

        void ActivateObjective(QuestStepObjective_Runtime objective, bool isMainObjective = false)
        {
            objective.OnProgressed -= ObjectiveProgressedHandler;
            objective.OnCompleted -= ObjectiveCompletedHandler;

            objective.OnProgressed += ObjectiveProgressedHandler;
            objective.OnCompleted += ObjectiveCompletedHandler;

            if (isMainObjective)
                objective.OnCompleted += _ => MainObjectiveCompletedHandler(objective);

            objective.Activate();
        }

        void ObjectiveProgressedHandler(QuestStepObjective_Runtime objective)
        {
            OnObjectiveProgressed?.Invoke(objective);
        }

        void ObjectiveCompletedHandler(QuestStepObjective_Runtime objective)
        {
            OnObjectiveCompleted?.Invoke(objective);
        }

        void MainObjectiveCompletedHandler(QuestStepObjective_Runtime objective)
        {
            if (QuestStepData.stepMode == QuestStepMode.Sequential)
            {
                int index = MainObjectives.IndexOf(objective);
                if (index >= 0 && index + 1 < MainObjectives.Count)
                    ActivateMainObjective(index + 1);
            }

            if (AllMainObjectivesDone)
                CompleteStep();
        }

        void CompleteStep()
        {
            if (State == QuestCompletionState.Completed)
                return;

            State = QuestCompletionState.Completed;
            ActiveMainObjectiveIndex = -1;
            Deactivate();
            OnStepCompleted?.Invoke();
        }

        IEnumerable<QuestStepObjective_Runtime> AllObjectives()
        {
            foreach (var objective in MainObjectives)
                yield return objective;

            foreach (var objective in OptionalObjectives)
                yield return objective;
        }

#if UNITY_EDITOR
        Color GetMainObjectiveListElementColor(int index, Color defaultColor) =>
            QuestRuntimeInspectorColors.GetMainObjectiveElementColor(this, index, defaultColor);

        Color GetOptionalObjectiveListElementColor(int index, Color defaultColor) =>
            QuestRuntimeInspectorColors.GetOptionalObjectiveElementColor(this, index, defaultColor);
#endif
    }
}
