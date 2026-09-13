using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RAXY.Event;
using RAXY.Utility.Localization;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RAXY.Quest
{
    [CreateAssetMenu(fileName = "Quest Step Objective Event SO", menuName = "RAXY/Quest/Quest Step Objective Event SO")]
    public class QuestStepObjectiveEventSO : EventSO<ObjectiveParameter>, IQuestStepObjectiveParameterProvider
    {
        [TitleGroup("Objective")]
        [SerializeField]
        bool useOverrider;

        [TitleGroup("Objective")]
        [SerializeField]
        [HideIf("@useOverrider")]
        List<string> parameters;

        [TitleGroup("Objective")]
        [SerializeField]
        [ShowIf("@useOverrider")]
        Object parameterProviderOverrider;

        [TitleGroup("Objective")]
        [ShowInInspector]
        [ShowIf("@useOverrider")]
        public List<string> Parameters
        {
            get
            {
                if (useOverrider)
                {
                    if (parameterProviderOverrider is IQuestStepObjectiveParameterProvider provider)
                        return provider.Parameters;
                    else
                        return parameters;
                }
                else    
                    return parameters;
            }
        }
    }

    [Serializable]
    public struct ObjectiveParameter
    {
        [ValueDropdown("Parameters", AppendNextDrawer = true)]
        public string parameter;

        public float amount;

#if UNITY_EDITOR
        public IQuestStepObjectiveParameterProvider paramProvider;
        public List<string> Parameters => paramProvider?.Parameters;

        public void Set_ParamProvider(IQuestStepObjectiveParameterProvider param)
        {
            paramProvider = param;
        }
#endif
    }
}
