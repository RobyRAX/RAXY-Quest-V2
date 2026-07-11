using System.Collections.Generic;
using UnityEngine;

namespace RAXY.Quest
{
    public interface IQuestDatabase
    {
        public List<QuestSO> Quests { get; }
    }
}
