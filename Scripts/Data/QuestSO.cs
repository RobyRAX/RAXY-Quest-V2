using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using RAXY.Utility.Localization;
using Sirenix.Utilities.Editor;
using Cysharp.Threading.Tasks;
using System;

namespace RAXY.Quest
{
    public enum QuestType
    {
        Main,
        Side
    }

    [CreateAssetMenu(fileName = "QuestSO", menuName = "RAXY/Quest/QuestSO")]
    public class QuestSO : ScriptableObject
    {
        [TitleGroup("Quest Type")]
        public QuestType questType = QuestType.Main;

        [TitleGroup("Quest Type")]
        [LabelText("Auto Complete After Steps")]
        public bool autoComplete = true;

        [TitleGroup("Quest Name")]
        [HideLabel]
        [HideReferenceObjectPicker]
        public StringProvider questNameProvider;

        [TitleGroup("Quest Requirement")]
        public List<QuestRequirementEntry> questRequirements;

        [TitleGroup("Quest Step")]
        [ListDrawerSettings(ShowIndexLabels = true, 
                            ListElementLabelName = "StepName", 
                            Expanded = true,
                            OnTitleBarGUI = "DrawRefresh_Btn")]
        [HideReferenceObjectPicker]
        public List<QuestStep> questSteps;

        [TitleGroup("Quest Actions")]
        [SerializeReference]
        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "Label", Expanded = true)]
        [HideReferenceObjectPicker]
        public List<QuestAction> actions_OnTaken = new();

        [TitleGroup("Quest Actions")]
        [SerializeReference]
        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "Label", Expanded = true)]
        [HideReferenceObjectPicker]
        public List<QuestAction> actions_OnCompleted = new();

        public string QuestId => name;
        public string QuestName => questNameProvider.String;

#if UNITY_EDITOR
        void DrawRefresh_Btn()
        {
            if (SirenixEditorGUI.ToolbarButton(EditorIcons.Refresh))
            {
                foreach (var step in questSteps)
                {
                    step.questStepNameProvider.RefreshCacheAsync().Forget();

                    foreach (var objective in step.mainObjectives)
                    {
                        objective.descriptionProvider.RefreshCacheAsync().Forget();
                    }

                    foreach (var objective in step.optionalObjectives)
                    {
                        objective.descriptionProvider.RefreshCacheAsync().Forget();
                    }
                }
            }
        }
#endif
    }

    [Serializable]
    public class QuestRequirementEntry
    {
        public QuestRequirementEventSO requirementEventSO;
        public float requiredFloat;
        public bool requiredBool;
    }
}
