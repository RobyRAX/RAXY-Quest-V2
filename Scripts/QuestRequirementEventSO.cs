using System;
using RAXY.Event;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    [CreateAssetMenu(fileName = "QuestRequirementEventSO", menuName = "RAXY/Quest/QuestRequirementEventSO")]
    public class QuestRequirementEventSO : EventSO<QuestRequirementParameter>
    {
        [TitleGroup("Quest Requirement Setting")]
        public string requirementId;
    }

    public struct QuestRequirementParameter
    {
        public float floatParam;
        public bool boolParam;
    }
}
