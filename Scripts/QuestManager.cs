using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    public class QuestManager : QuestManagerBase
    {
        [TitleGroup("Quest Database")]
        [SerializeField]
        List<QuestSO> questDatabase = new();

        protected override void Awake()
        {
            base.Awake();

            BaseInstance = this;

            SetQuestDatabase(new InlineQuestDatabase(questDatabase));

            InitQuestManager();
        }

        public void RegisterQuests(IEnumerable<QuestSO> quests)
        {
            if (quests == null)
                return;

            questDatabase.AddRange(quests);
            SetQuestDatabase(new InlineQuestDatabase(questDatabase));
            InitQuestManager();
        }
    }

    public class InlineQuestDatabase : IQuestDatabase
    {
        readonly List<QuestSO> _quests;

        public InlineQuestDatabase(List<QuestSO> quests)
        {
            _quests = quests ?? new List<QuestSO>();
        }

        public List<QuestSO> Quests => _quests;

        public QuestSO GetQuest(string questId)
        {
            if (_quests == null)
                return null;

            for (int i = 0; i < _quests.Count; i++)
            {
                if (_quests[i] != null && _quests[i].QuestId == questId)
                    return _quests[i];
            }

            return null;
        }
    }
}
