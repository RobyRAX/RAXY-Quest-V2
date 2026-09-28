using System.Collections.Generic;
using System.Linq;
using RAXY.Quest;
using UnityEngine;

[CreateAssetMenu(fileName = "SampleQuestDatabase", menuName = "RAXY/Quest/Sample Quest Database")]
public class SampleQuestDatabaseSO : ScriptableObject, IQuestDatabase
{
    [SerializeField]
    List<QuestSO> quests;

    public List<QuestSO> Quests => quests;

    public QuestSO GetQuest(string questId)
    {
        return quests?.FirstOrDefault(q => q != null && q.QuestId == questId);
    }
}
