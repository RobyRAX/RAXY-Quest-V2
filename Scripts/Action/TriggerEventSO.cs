using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RAXY.Event;
using Sirenix.OdinInspector;

namespace RAXY.Quest
{
    [Serializable]
    public class TriggerEventSO : QuestAction
    {
        [HideLabel]
        [HideReferenceObjectPicker]
        public EventSoRaiser eventRaiser;

        public override UniTask ExecuteAsync(
            QuestActionContext ctx,
            CancellationToken ct = default)
        {
            eventRaiser?.Raise();
            return UniTask.CompletedTask;
        }
    }
}
