using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using RAXY.Utility.Localization;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using System.Collections;
#endif

namespace RAXY.Quest
{
    [CreateAssetMenu(fileName = "Quest SO", menuName = "RAXY/Quest/Quest SO")]
    public class QuestSO : ScriptableObject
    {
        const int LegacyQuestTypeUnset = -1;

        [TitleGroup("Quest Type")]
        [ValueDropdown("GetQuestTypeOptions")]
        public string questType = QuestTypeIds.Main;

        [SerializeField, HideInInspector, FormerlySerializedAs("questType")]
        int legacyQuestType = LegacyQuestTypeUnset;

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
        public List<IQuestAction> actions_OnTaken = new();

        [TitleGroup("Quest Actions")]
        [SerializeReference]
        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "Label", Expanded = true)]
        [HideReferenceObjectPicker]
        public List<IQuestAction> actions_OnCompleted = new();

        public string QuestId => name;
        public string QuestName => questNameProvider.String;

        void OnEnable()
        {
            MigrateLegacyQuestType();
        }

        void MigrateLegacyQuestType()
        {
            if (legacyQuestType < 0)
                return;

            questType = legacyQuestType switch
            {
                0 => QuestTypeIds.Main,
                1 => "Side",
                _ => QuestTypeIds.Main
            };

            legacyQuestType = LegacyQuestTypeUnset;

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

#if UNITY_EDITOR
        IEnumerable GetQuestTypeOptions()
        {
            return QuestTypeEditorBridge.GetTypes();
        }

        void DrawRefresh_Btn()
        {
            if (Sirenix.Utilities.Editor.SirenixEditorGUI.ToolbarButton(Sirenix.Utilities.Editor.EditorIcons.Refresh))
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
        public List<string> requiredListString;
    }
}
