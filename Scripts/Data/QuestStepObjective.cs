using System;
using System.Collections.Generic;
using RAXY.Utility.Localization;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    [Serializable]
    public class QuestStepObjective
    {
        [TitleGroup("Objective Setting")]
        public QuestStepObjectiveEventSO eventSO;

        [TitleGroup("Objective Setting")]
        [ShowIf("@eventSO != null")]
        [ValueDropdown("Parameters")]
        public string objectiveParameter;
        List<string> Parameters
        {
            get
            {
                if (eventSO == null)
                    return null;
                
                return eventSO.Parameters;
            }
        }

        [TitleGroup("Objective Setting")]
        [ShowIf("@eventSO != null")]
        public float requiredAmount;

        [TitleGroup("Objective Description")]
        [HideReferenceObjectPicker]
        public StringProvider descriptionProvider;

        public string ObjectiveDescription => descriptionProvider.String;
    }
}
