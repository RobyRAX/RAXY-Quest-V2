using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    public class QuestSmokeTest : MonoBehaviour
    {
        [TitleGroup("Kill Test")]
        [SerializeField] 
        QuestStepObjectiveEventSO killEventSO;

        [TitleGroup("Kill Test")]
        [SerializeField] 
        string killTarget = "Smallin";

        [TitleGroup("Kill Test")]
        [Button]
        void RaiseKillEvent()
        {
            if (killEventSO == null)
            {
                Debug.LogWarning("[QuestSmokeTest] Kill event SO is not assigned.");
                return;
            }

            killEventSO.Raise(killTarget);
            Debug.Log($"[QuestSmokeTest] Raised kill event for '{killTarget}'.");
        }

        [TitleGroup("Collect Test")]
        [SerializeField] 
        QuestStepObjectiveEventSO collectEventSO;

        [TitleGroup("Collect Test")]
        [SerializeField] 
        string collectTarget = "Coin";

        [TitleGroup("Collect Test")]
        [Button]
        void RaiseCollectEvent()
        {
            if (collectEventSO == null)
            {
                Debug.LogWarning("[QuestSmokeTest] Collect event SO is not assigned.");
                return;
            }

            collectEventSO.Raise(collectTarget);
            Debug.Log($"[QuestSmokeTest] Raised collect event for '{collectTarget}'.");
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
                boolParam = requirementBoolValue
            });

            Debug.Log($"[QuestSmokeTest] Raised requirement event (float={requirementFloatValue}, bool={requirementBoolValue}).");
        }
    }
}
