using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    [Serializable]
    public class TakeQuest : QuestAction
    {
        [HideLabel]
        public QuestSO questToTake;

        public override string Label =>
            questToTake != null ? $"TakeQuest ({questToTake.QuestId})" : "TakeQuest";

        public override UniTask ExecuteAsync(
            QuestActionContext ctx,
            CancellationToken ct = default)
        {
            if (questToTake == null)
            {
                Debug.LogWarning("[TakeQuest] questToTake is not set.");
                return UniTask.CompletedTask;
            }

            if (ctx.Manager == null)
            {
                Debug.LogWarning("[TakeQuest] QuestManager is null.");
                return UniTask.CompletedTask;
            }

            ctx.Manager.TakeQuest(questToTake.QuestId);
            return UniTask.CompletedTask;
        }
    }
}
