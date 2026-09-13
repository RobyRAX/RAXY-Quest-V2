using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    [Serializable]
    public class TakeQuest : IQuestAction
    {
        [HideLabel]
        public QuestSO questToTake;

        public string Label =>
            questToTake != null ? $"TakeQuest ({questToTake.QuestId})" : "TakeQuest";

        public UniTask ExecuteAsync(CancellationToken ct = default)
        {
            if (questToTake == null)
            {
                Debug.LogWarning("[TakeQuest] questToTake is not set.");
                return UniTask.CompletedTask;
            }

            if (QuestManagerBase.BaseInstance == null)
            {
                Debug.LogWarning("[TakeQuest] QuestManager BaseInstance is null.");
                return UniTask.CompletedTask;
            }

            QuestManagerBase.BaseInstance.TakeQuest(questToTake.QuestId);
            return UniTask.CompletedTask;
        }
    }
}
