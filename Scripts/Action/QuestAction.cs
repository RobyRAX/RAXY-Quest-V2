using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace RAXY.Quest
{
    public enum QuestActionTrigger
    {
        Taken,
        Entered,
        StepCompleted,
        Completed
    }

    public readonly struct QuestActionContext
    {
        public readonly string QuestId;
        public readonly QuestSO QuestSO;
        public readonly QuestManagerBase Manager;
        public readonly QuestActionTrigger Trigger;
        public readonly int StepIndex;

        public QuestActionContext(
            string questId,
            QuestSO questSO,
            QuestManagerBase manager,
            QuestActionTrigger trigger,
            int stepIndex = -1)
        {
            QuestId = questId;
            QuestSO = questSO;
            Manager = manager;
            Trigger = trigger;
            StepIndex = stepIndex;
        }
    }

    [Serializable]
    public abstract class QuestAction
    {
        public virtual string Label => GetType().Name;

        public abstract UniTask ExecuteAsync(
            QuestActionContext ctx,
            CancellationToken ct = default);
    }
}
