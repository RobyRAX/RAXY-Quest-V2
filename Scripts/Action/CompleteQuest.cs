using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    [Serializable]
    public class CompleteQuest : QuestAction
    {
        [HideLabel]
        public QuestSO questToComplete;

        public override string Label =>
            questToComplete != null
                ? $"CompleteQuest ({questToComplete.QuestId})"
                : "CompleteQuest (Context)";

        public override UniTask ExecuteAsync(
            QuestActionContext ctx,
            CancellationToken ct = default)
        {
            string questId = questToComplete != null
                ? questToComplete.QuestId
                : ctx.QuestId;

            if (string.IsNullOrEmpty(questId))
            {
                Debug.LogWarning("[CompleteQuest] QuestId is empty.");
                return UniTask.CompletedTask;
            }

            if (ctx.Manager == null)
            {
                Debug.LogWarning("[CompleteQuest] QuestManager is null.");
                return UniTask.CompletedTask;
            }

            ctx.Manager.CompleteQuest(questId);
            return UniTask.CompletedTask;
        }
    }
}
