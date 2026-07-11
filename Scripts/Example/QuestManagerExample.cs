using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    public class QuestManagerExample : QuestManagerBase
    {
        [TitleGroup("Quest Database")]
        [SerializeField] QuestDatabaseSO questDatabase;

        void Awake()
        {
            if (questDatabase != null)
                SetQuestDatabase(questDatabase);
        }
    }
}
