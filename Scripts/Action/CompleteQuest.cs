using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    [Serializable]
    public class CompleteQuest : IQuestAction
    {
        [HideLabel]
        public QuestSO questToComplete;

        public string Label =>
            questToComplete != null
                ? $"CompleteQuest ({questToComplete.QuestId})"
                : "CompleteQuest";

        public UniTask ExecuteAsync(CancellationToken ct = default)
        {
            if (questToComplete == null)
            {
                Debug.LogWarning("[CompleteQuest] questToComplete is not set.");
                return UniTask.CompletedTask;
            }

            string questId = questToComplete.QuestId;
            if (string.IsNullOrEmpty(questId))
            {
                Debug.LogWarning("[CompleteQuest] QuestId is empty.");
                return UniTask.CompletedTask;
            }

            if (QuestManagerBase.BaseInstance == null)
            {
                Debug.LogWarning("[CompleteQuest] QuestManager BaseInstance is null.");
                return UniTask.CompletedTask;
            }

            QuestManagerBase.BaseInstance.CompleteQuest(questId);
            return UniTask.CompletedTask;
        }
    }
}
