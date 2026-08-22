using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    public class QuestTriggerBus : QuestBindingBase
    {
        [TitleGroup("Debug Functions")]
        [Button]
        public void Execute()
        {
            questManager?.TakeQuest(questId);
        }
    }
}
