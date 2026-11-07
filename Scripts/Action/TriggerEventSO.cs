using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RAXY.Sequence;
using RAXY.Event;
using Sirenix.OdinInspector;

namespace RAXY.Quest
{
    [Serializable]
    public class TriggerEventSO : SequenceableBase, IQuestAction
    {
        [HideLabel]
        [HideReferenceObjectPicker]
        public EventSoRaiser eventRaiser;

        public override string Label => "TriggerEventSO";

        public override UniTask ExecuteAsync(CancellationToken ct = default)
        {
            eventRaiser?.Raise();
            return UniTask.CompletedTask;
        }
    }
}
