using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    public class QuestManagerExample : QuestManagerBase
    {
        [TitleGroup("Quest Database")]
        [SerializeField] 
        [PropertyOrder(-1)]
        QuestDatabaseSO questDatabase;

        protected override void Awake()
        {
            base.Awake();

            if (questDatabase != null)
                SetQuestDatabase(questDatabase);

            Init();
        }
    }
}
