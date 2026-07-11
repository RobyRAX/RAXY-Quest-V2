using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RAXY.Quest
{
    [CreateAssetMenu(fileName = "QuestDatabaseSO", menuName = "RAXY/Quest/QuestDatabaseSO")]
    public class QuestDatabaseSO : ScriptableObject, IQuestDatabase
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
