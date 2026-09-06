using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RAXY.Quest
{
    public class QuestObject : MonoBehaviour
    {
#if UNITY_EDITOR
        void OnValidate()
        {
            if (toggleConditions == null)
                return;

            for (int i = 0; i < toggleConditions.Count; i++)
            {
                var condition = toggleConditions[i];
                if (condition == null)
                    continue;

                condition.EditorDb = QuestDatabase;
                condition.SyncStepsFromQuest();
            }
        }

        [TitleGroup("Editor Data")]
        [SerializeField]
        Object editor_QuestDb;

        [TitleGroup("Editor Data")]
        [Button]
        void Find_QuestDatabaseSO()
        {
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so is IQuestDatabase)
                {
                    editor_QuestDb = so;
                    return;
                }
            }

            editor_QuestDb = null;
        }

        IQuestDatabase QuestDatabase
        {
            get
            {
                if (editor_QuestDb is null)
                    return null;
                
                if (editor_QuestDb is IQuestDatabase questDb)
                    return questDb;
                
                return null;
            }
        }
#endif

        [TitleGroup("Settings")]
        public bool defaultVisibilityState;

        [TitleGroup("Settings")]
        public List<QuestObjectToggleCondition> toggleConditions;

        [TitleGroup("Runtime")]
        [ShowInInspector]
        QuestManagerBase questManager;

        public void Set_QuestManager(QuestManagerBase questMan)
        {
            UnsubscribeManagerEvents();
            questManager = questMan;
            SubscribeManagerEvents();
            Refresh();
        }

        [TitleGroup("Debug Functions")]
        [Button]
        public void Refresh()
        {
            if (questManager == null)
                return;

            bool flip = false;
            if (toggleConditions != null)
            {
                for (int i = 0; i < toggleConditions.Count; i++)
                {
                    var condition = toggleConditions[i];
                    if (condition != null && condition.IsMet(questManager))
                    {
                        flip = true;
                        break;
                    }
                }
            }

            bool shouldBeActive = flip ? !defaultVisibilityState : defaultVisibilityState;
            if (gameObject.activeSelf != shouldBeActive)
                gameObject.SetActive(shouldBeActive);
        }

        void OnDestroy()
        {
            UnsubscribeManagerEvents();
        }

        void SubscribeManagerEvents()
        {
            if (questManager == null)
                return;

            questManager.OnQuestTaken += OnManagerQuestChanged;
            questManager.OnQuestCompleted += OnManagerQuestChanged;
            questManager.OnQuestStepChanged += OnManagerQuestStepChanged;
            questManager.OnObjectiveProgressed += OnManagerObjectiveChanged;
            questManager.OnObjectiveCompleted += OnManagerObjectiveChanged;
        }

        void UnsubscribeManagerEvents()
        {
            if (questManager == null)
                return;

            questManager.OnQuestTaken -= OnManagerQuestChanged;
            questManager.OnQuestCompleted -= OnManagerQuestChanged;
            questManager.OnQuestStepChanged -= OnManagerQuestStepChanged;
            questManager.OnObjectiveProgressed -= OnManagerObjectiveChanged;
            questManager.OnObjectiveCompleted -= OnManagerObjectiveChanged;
        }

        void OnManagerQuestChanged(string _) => Refresh();

        void OnManagerQuestStepChanged(string _, int __) => Refresh();

        void OnManagerObjectiveChanged(QuestStepObjective_Runtime _) => Refresh();
    }

    [Serializable]
    public class QuestObjectToggleCondition
    {
        [NonSerialized]
        public IQuestDatabase EditorDb;

#if UNITY_EDITOR
        [ValueDropdown(nameof(EditorQuestIds), AppendNextDrawer = true)]
        [OnValueChanged(nameof(SyncStepsFromQuest))]
#endif
        public string questId;

#if UNITY_EDITOR
        [OnValueChanged(nameof(SyncStepsFromQuest))]
#endif
        public QuestCompletionState questCompletionState;

        [ShowIf("@IsInProgress")]
        [ListDrawerSettings(
            ShowIndexLabels = true,
            ListElementLabelName = "Label",
            Expanded = true)]
        public List<QuestObjectStepToggle> steps;

        bool IsInProgress => questCompletionState == QuestCompletionState.InProgress;

        public bool IsMet(QuestManagerBase manager)
        {
            if (manager == null || string.IsNullOrEmpty(questId))
                return false;

            var status = manager.GetQuestStatus(questId);
            if (status == null || status.CompletionState != questCompletionState)
                return false;

            if (questCompletionState != QuestCompletionState.InProgress)
                return true;

            if (steps == null || steps.Count == 0)
                return false;

            var activeQuest = manager.GetActiveQuest(questId);
            if (activeQuest == null)
                return false;

            int currentStepIndex = activeQuest.CurrentQuestStepIndex;
            if (currentStepIndex < 0 || currentStepIndex >= steps.Count)
                return false;

            var stepToggle = steps[currentStepIndex];
            if (stepToggle == null)
                return false;

            // Step-only mode (previous behavior): enabled on this step is enough.
            if (!stepToggle.useObjectives)
                return stepToggle.enabled;

            if (stepToggle.mainObjectives == null || stepToggle.mainObjectives.Count == 0)
                return false;

            var activeStep = activeQuest.ActiveStep;
            if (activeStep == null || activeStep.MainObjectives == null)
                return false;

            if (activeStep.StepMode == QuestStepMode.Sequential)
            {
                int objectiveIndex = activeStep.ActiveMainObjectiveIndex;
                if (objectiveIndex < 0 ||
                    objectiveIndex >= stepToggle.mainObjectives.Count ||
                    objectiveIndex >= activeStep.MainObjectives.Count)
                    return false;

                var toggle = stepToggle.mainObjectives[objectiveIndex];
                var objective = activeStep.MainObjectives[objectiveIndex];
                return toggle != null &&
                       toggle.enabled &&
                       objective != null &&
                       objective.State == QuestCompletionState.InProgress;
            }

            int count = Math.Min(stepToggle.mainObjectives.Count, activeStep.MainObjectives.Count);
            for (int i = 0; i < count; i++)
            {
                var toggle = stepToggle.mainObjectives[i];
                if (toggle == null || !toggle.enabled)
                    continue;

                var objective = activeStep.MainObjectives[i];
                if (objective != null && objective.State == QuestCompletionState.InProgress)
                    return true;
            }

            return false;
        }

#if UNITY_EDITOR
        IEnumerable<string> EditorQuestIds
        {
            get
            {
                if (EditorDb?.Quests == null)
                    yield break;

                for (int i = 0; i < EditorDb.Quests.Count; i++)
                {
                    var quest = EditorDb.Quests[i];
                    if (quest != null)
                        yield return quest.QuestId;
                }
            }
        }

        public void SyncStepsFromQuest()
        {
            if (!IsInProgress)
                return;

            var quest = GetEditorQuest();
            if (quest?.questSteps == null)
                return;

            int targetStepCount = quest.questSteps.Count;
            steps ??= new List<QuestObjectStepToggle>();

            while (steps.Count < targetStepCount)
                steps.Add(new QuestObjectStepToggle());

            while (steps.Count > targetStepCount)
                steps.RemoveAt(steps.Count - 1);

            for (int i = 0; i < targetStepCount; i++)
            {
                var questStep = quest.questSteps[i];
                var stepToggle = steps[i] ?? (steps[i] = new QuestObjectStepToggle());

                string stepName = questStep?.StepName;
                stepToggle.Label = string.IsNullOrEmpty(stepName) ? $"Step {i}" : stepName;

                int targetObjectiveCount = questStep?.mainObjectives?.Count ?? 0;
                stepToggle.mainObjectives ??= new List<QuestObjectObjectiveToggle>();

                while (stepToggle.mainObjectives.Count < targetObjectiveCount)
                    stepToggle.mainObjectives.Add(new QuestObjectObjectiveToggle());

                while (stepToggle.mainObjectives.Count > targetObjectiveCount)
                    stepToggle.mainObjectives.RemoveAt(stepToggle.mainObjectives.Count - 1);

                for (int j = 0; j < targetObjectiveCount; j++)
                {
                    var objective = questStep.mainObjectives[j];
                    var objectiveToggle = stepToggle.mainObjectives[j]
                        ?? (stepToggle.mainObjectives[j] = new QuestObjectObjectiveToggle());

                    string description = objective?.ObjectiveDescription;
                    objectiveToggle.Label = string.IsNullOrEmpty(description)
                        ? $"Objective {j}"
                        : description;
                }
            }
        }

        QuestSO GetEditorQuest()
        {
            if (EditorDb == null || string.IsNullOrEmpty(questId))
                return null;

            return EditorDb.GetQuest(questId);
        }
#endif
    }

    [Serializable]
    public class QuestObjectStepToggle
    {
        [HideInInspector]
        public string Label;

        [Tooltip("When off, this step is ignored (same as unchecked step before).")]
        public bool enabled;

        [ShowIf(nameof(enabled))]
        [Tooltip("When on, match against main objectives. When off, only the step enabled flag is used.")]
        public bool useObjectives;

        [ShowIf("@enabled && useObjectives")]
        [ListDrawerSettings(
            ShowIndexLabels = true,
            ListElementLabelName = "Label",
            Expanded = true)]
        public List<QuestObjectObjectiveToggle> mainObjectives;
    }

    [Serializable]
    public class QuestObjectObjectiveToggle
    {
        [HideInInspector]
        public string Label;

        public bool enabled;
    }
}
