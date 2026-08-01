using System.Collections.Generic;

namespace RAXY.Quest
{
    public interface IQuestDatabase
    {
        List<QuestSO> Quests { get; }
        QuestSO GetQuest(string questId);
    }
}
