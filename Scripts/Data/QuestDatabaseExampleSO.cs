using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RAXY.Quest
{
    [CreateAssetMenu(fileName = "QuestDatabaseExampleSO", menuName = "RAXY/Quest/QuestDatabaseExampleSO")]
    public class QuestDatabaseExampleSO : ScriptableObject, IQuestDatabase
    {
        [SerializeField]
        List<QuestSO> quests;

        public List<QuestSO> Quests => quests;

        public QuestSO GetQuest(string questId)
        {
            return quests?.FirstOrDefault(q => q != null && q.QuestId == questId);
        }
    }
}
