using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace RAXY.Quest
{
    [Serializable]
    public class QuestStepChangedEvent : UnityEvent<int>
    {
    }

    public class QuestEventListener : QuestBindingBase
    {
        [TitleGroup("Settings")]
        [Tooltip("Only fire OnQuestStepChangedEvent for this step. Use Any Step to fire on every step change.")]
#if UNITY_EDITOR
        [ValueDropdown(nameof(EditorStepOptions))]
#endif
        public int filterStepIndex = -1;

        [TitleGroup("Events")]
        public UnityEvent OnQuestTakenEvent;

        [TitleGroup("Events")]
        public UnityEvent OnQuestCompletedEvent;

        [TitleGroup("Events")]
        public QuestStepChangedEvent OnQuestStepChangedEvent = new();

        [TitleGroup("Runtime")]
        [ShowInInspector]
        [ReadOnly]
        public int LastStepIndex { get; private set; } = -1;

        public override void Set_QuestManager(QuestManagerBase questMan)
        {
            Unsubscribe();
            base.Set_QuestManager(questMan);
            Subscribe();
        }

        void OnDestroy()
        {
            Unsubscribe();
        }

        public void Subscribe()
        {
            Unsubscribe();

            if (questManager != null)
            {
                questManager.OnQuestTaken += HandleQuestTaken;
                questManager.OnQuestCompleted += HandleQuestCompleted;
                questManager.OnQuestStepChanged += HandleQuestStepChanged;
            }
        }

        public void Unsubscribe()
        {
            if (questManager != null)
            {
                questManager.OnQuestTaken -= HandleQuestTaken;
                questManager.OnQuestCompleted -= HandleQuestCompleted;
                questManager.OnQuestStepChanged -= HandleQuestStepChanged;
            }
        }

        void HandleQuestTaken(string id)
        {
            if (id == questId)
                OnQuestTakenEvent?.Invoke();
        }

        void HandleQuestCompleted(string id)
        {
            if (id == questId)
                OnQuestCompletedEvent?.Invoke();
        }

        void HandleQuestStepChanged(string id, int stepIndex)
        {
            if (id != questId)
                return;

            if (filterStepIndex >= 0 && stepIndex != filterStepIndex)
                return;

            LastStepIndex = stepIndex;
            OnQuestStepChangedEvent?.Invoke(stepIndex);
        }

#if UNITY_EDITOR
        IEnumerable<ValueDropdownItem<int>> EditorStepOptions()
        {
            yield return new ValueDropdownItem<int>("Any Step", -1);

            var quest = GetEditorQuest();
            if (quest?.questSteps == null)
                yield break;

            for (int i = 0; i < quest.questSteps.Count; i++)
            {
                var step = quest.questSteps[i];
                string stepName = step?.StepName;
                string label = string.IsNullOrEmpty(stepName) ? $"Step {i}" : $"[{i}] {stepName}";
                yield return new ValueDropdownItem<int>(label, i);
            }
        }
#endif
    }
}
