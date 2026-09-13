using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RAXY.Event;
using Sirenix.OdinInspector;

namespace RAXY.Quest
{
    [Serializable]
    public class TriggerEventSO : IQuestAction
    {
        [HideLabel]
        [HideReferenceObjectPicker]
        public EventSoRaiser eventRaiser;

        public string Label => "TriggerEventSO";

        public UniTask ExecuteAsync(CancellationToken ct = default)
        {
            eventRaiser?.Raise();
            return UniTask.CompletedTask;
        }
    }
}
