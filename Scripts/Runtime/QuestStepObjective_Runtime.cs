using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    [Serializable]
    public class QuestStepObjective_Runtime
    {
        //[ShowInInspector]
        public QuestStepObjective QuestStepObjectiveData { get; }

        [ShowInInspector]
        public QuestStepObjectiveEventSO ObjectiveEventSO => QuestStepObjectiveData?.eventSO;

        [ShowInInspector]
        public string ObjectiveParameter => QuestStepObjectiveData?.objectiveParameter;

        public string ObjectiveDescription =>
            QuestStepObjectiveData?.descriptionProvider?.String ?? "(No Description)";

        [ShowInInspector]
        public QuestCompletionState State { get; private set; }

        [ShowInInspector]
        public float CurrentAmount { get; private set; }

        [ShowInInspector]
        public float RequiredAmount => QuestStepObjectiveData?.requiredAmount ?? 0f;

        [ShowInInspector]
        public bool IsCompleted => State == QuestCompletionState.Completed;

        public event Action<QuestStepObjective_Runtime> OnProgressed;
        public event Action<QuestStepObjective_Runtime> OnCompleted;

        Action<string> _eventHandler;
        bool _isSubscribed;

        public QuestStepObjective_Runtime(QuestStepObjective data)
        {
            QuestStepObjectiveData = data;
            State = QuestCompletionState.NotStarted;
        }

        public void Activate()
        {
            if (State == QuestCompletionState.Completed)
                return;

            State = QuestCompletionState.InProgress;

            var eventSO = QuestStepObjectiveData?.eventSO;
            if (eventSO == null)
                return;

            _eventHandler ??= OnEventRaised;

            if (!_isSubscribed)
            {
                eventSO.Subscribe(_eventHandler);
                _isSubscribed = true;
            }
        }

        public void Deactivate()
        {
            var eventSO = QuestStepObjectiveData?.eventSO;
            if (eventSO != null && _isSubscribed && _eventHandler != null)
            {
                eventSO.Unsubscribe(_eventHandler);
                _isSubscribed = false;
            }
        }

        public void AddProgress(float amount = 1f)
        {
            if (State != QuestCompletionState.InProgress)
                return;

            var requiredAmount = RequiredAmount;
            var clamped = requiredAmount > 0f
                ? Mathf.Clamp(CurrentAmount + amount, 0f, requiredAmount)
                : CurrentAmount + amount;

            if (Mathf.Approximately(clamped, CurrentAmount))
                return;

            CurrentAmount = clamped;
            OnProgressed?.Invoke(this);

            if (requiredAmount <= 0f || CurrentAmount >= requiredAmount)
                MarkCompleted();
        }

        void OnEventRaised(string param)
        {
            var objectiveParameter = QuestStepObjectiveData?.objectiveParameter;
            if (!string.IsNullOrEmpty(objectiveParameter) && param != objectiveParameter)
                return;

            AddProgress(1f);
        }

        void MarkCompleted()
        {
            if (State == QuestCompletionState.Completed)
                return;

            State = QuestCompletionState.Completed;
            Deactivate();
            OnCompleted?.Invoke(this);
        }
    }
}
