using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    public class QuestSmokeTest : MonoBehaviour
    {
        [TitleGroup("Objective Test")]
        [SerializeField] 
        [OnValueChanged("OnObjectiveEventSoChanged")]
        QuestStepObjectiveEventSO objectiveEventSO;

        [TitleGroup("Objective Test")]
        [SerializeField] 
        [HideLabel]
        ObjectiveParameter param = new ObjectiveParameter();

#if UNITY_EDITOR
        void OnObjectiveEventSoChanged()
        {
            param.Set_ParamProvider(objectiveEventSO);
        }
#endif

        [TitleGroup("Objective Test")]
        [Button]
        void RaiseObjectiveEvent()
        {
            if (objectiveEventSO == null)
            {
                Debug.LogWarning("[QuestSmokeTest] Collect event SO is not assigned.");
                return;
            }

            objectiveEventSO.Raise(param);
            Debug.Log($"[QuestSmokeTest] Raised collect event for '{param}'.");
        }

        [TitleGroup("Requirement Test")]
        [SerializeField] 
        QuestRequirementEventSO requirementEventSO;

        [TitleGroup("Requirement Test")]
        [SerializeField] 
        float requirementFloatValue;

        [TitleGroup("Requirement Test")]
        [SerializeField] 
        bool requirementBoolValue;

        [TitleGroup("Requirement Test")]
        [SerializeField]
        List<string> requirementListStringValue = new();

        [TitleGroup("Requirement Test")]
        [Button]
        void RaiseRequirementEvent()
        {
            if (requirementEventSO == null)
            {
                Debug.LogWarning("[QuestSmokeTest] Requirement event SO is not assigned.");
                return;
            }

            requirementEventSO.Raise(new QuestRequirementParameter
            {
                floatParam = requirementFloatValue,
                boolParam = requirementBoolValue,
                listStringParam = requirementListStringValue
            });

            Debug.Log($"[QuestSmokeTest] Raised requirement event (float={requirementFloatValue}, bool={requirementBoolValue}, listString=[{string.Join(", ", requirementListStringValue ?? new List<string>())}]).");
        }
    }
}
