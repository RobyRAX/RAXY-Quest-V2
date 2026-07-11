using System;
using System.Collections.Generic;
using RAXY.Utility.Localization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    [HideReferenceObjectPicker]
    [Serializable]
    public class QuestStep
    {
        [HideReferenceObjectPicker]
        public StringProvider questStepNameProvider;
        public string StepName => questStepNameProvider.String;

        public QuestStepMode stepMode;

        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "ObjectiveDescription", Expanded = true)]
        [HideReferenceObjectPicker]
        public List<QuestStepObjective> mainObjectives;

        [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "ObjectiveDescription", Expanded = true)]
        [HideReferenceObjectPicker]
        public List<QuestStepObjective> optionalObjectives;
    }

    public enum QuestStepMode
    {
        Sequential,
        Parallel
    }
}
