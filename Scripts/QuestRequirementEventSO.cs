using System;
using System.Collections.Generic;
using RAXY.Event;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    [CreateAssetMenu(fileName = "QuestRequirementEventSO", menuName = "RAXY/Quest/QuestRequirementEventSO")]
    public class QuestRequirementEventSO : EventSO<QuestRequirementParameter>
    {
        [TitleGroup("Quest Requirement Setting")]
        [SerializeField]
        string requirementId;

        public string RequirementId
        {
            get
            {
                if (string.IsNullOrEmpty(requirementId))
                    return name;
                else
                    return requirementId;
            }
        }
    }

    [Serializable]
    public struct QuestRequirementParameter
    {
        public float floatParam;
        public bool boolParam;
        public List<string> listStringParam;
    }
}
