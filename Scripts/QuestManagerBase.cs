using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    public abstract class QuestManagerBase : MonoBehaviour
    {
        public event Action<string> OnQuestTaken;
        public event Action<string> OnQuestCompleted;
        public event Action<string, int> OnQuestStepChanged;
        public event Action<QuestStepObjective_Runtime> OnObjectiveProgressed;
        public event Action<QuestStepObjective_Runtime> OnObjectiveCompleted;
        public event Action<string> OnTrackedQuestChanged;

        [TitleGroup("Dependency")]
        [ShowInInspector]
        public IQuestDatabase QuestDatabase { get; private set; }

        [TitleGroup("Tracked Quest")]
        [ShowInInspector]
        public string TrackedQuest => trackedQuest;

        [TitleGroup("Tracked Quest")]
        [SerializeField]
        string trackedQuest;

        [TitleGroup("Requirements")]
        [ShowInInspector]
        [HideReferenceObjectPicker]
        public Dictionary<string, QuestRequirementParameter> TrackedRequirementDict = new();

        [TitleGroup("All Quests")]
        [ShowInInspector]
        [DictionaryDrawerSettings(KeyLabel = "Quest Id", ValueLabel = "Status")]
        [HideReferenceObjectPicker]
        public Dictionary<string, QuestStatus_Runtime> QuestStatusDict { get; private set; } = new();

        [TitleGroup("Active Quests")]
        [ShowInInspector]
        [DictionaryDrawerSettings(KeyLabel = "Quest Id", ValueLabel = "Progress")]
        [HideReferenceObjectPicker]
        public Dictionary<string, Quest_Runtime> ActiveQuests { get; private set; } = new();

        readonly Dictionary<QuestRequirementEventSO, Action<QuestRequirementParameter>> _requirementHandlers = new();

        [TitleGroup("Quest Objects")]
        public List<QuestObject> questObjects = new();

        [HorizontalGroup("Quest Objects/Action")]
        [Button("Scan")]
        public void ScanQuestObjects()
        {
            questObjects?.Clear();

            var allQuestObjects = FindObjectsByType<QuestObject>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (var qo in allQuestObjects)
            {
                qo.Set_QuestManager(this);
                questObjects.Add(qo);
            }

            RefreshQuestObjects();
        }

        [HorizontalGroup("Quest Objects/Action")]
        [Button("Refresh")]
        public void RefreshQuestObjects()
        {
            if (questObjects == null)
                return;

            for (int i = 0; i < questObjects.Count; i++)
            {
                var qo = questObjects[i];
                if (qo == null)
                    continue;

                qo.Refresh();
            }
        }

        [TitleGroup("Dependency")]
        [Button]
        public void SetQuestDatabase(IQuestDatabase database)
        {
            QuestDatabase = database;
        }

        protected virtual void Awake()
        {
            ScanQuestObjects();
        }

        public void RefreshQuestStatusDict()
        {
            if (QuestStatusDict == null)
                return;

            foreach (var status in QuestStatusDict.Values)
                status.UpdateRequirementStatus(TrackedRequirementDict);

            RefreshQuestObjects();
        }

        [TitleGroup("All Quests")]
        [Button]
        public void InitQuestManager()
        {
            if (QuestDatabase == null)
            {
                Debug.LogError("[QuestManager] QuestDatabase is not set.");
                return;
            }

            QuestStatusDict ??= new Dictionary<string, QuestStatus_Runtime>();
            ActiveQuests ??= new Dictionary<string, Quest_Runtime>();

            foreach (var quest in QuestDatabase.Quests)
            {
                if (quest == null)
                    continue;

                if (!QuestStatusDict.ContainsKey(quest.QuestId))
                    QuestStatusDict[quest.QuestId] = new QuestStatus_Runtime(quest);
            }

            SubscribeAllRequirements();
            RefreshQuestStatusDict();
        }

        [TitleGroup("Debug Function")]
        [Button]
        public void TakeQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId))
                return;

            if (QuestDatabase == null)
            {
                Debug.LogWarning("[QuestManager] QuestDatabase is not set.");
                return;
            }

            QuestStatusDict ??= new Dictionary<string, QuestStatus_Runtime>();
            ActiveQuests ??= new Dictionary<string, Quest_Runtime>();

            if (!QuestStatusDict.TryGetValue(questId, out var status))
            {
                Debug.LogWarning($"[QuestManager] QuestId '{questId}' not found.");
                return;
            }

            if (status.CompletionState == QuestCompletionState.InProgress)
            {
                Debug.LogWarning($"[QuestManager] QuestId '{questId}' is already in progress.");
                return;
            }

            if (status.CompletionState == QuestCompletionState.Completed)
            {
                Debug.LogWarning($"[QuestManager] QuestId '{questId}' is already completed.");
                return;
            }

            if (status.CompletionState == QuestCompletionState.NotStarted)
            {
                Debug.LogWarning($"[QuestManager] QuestId '{questId}' has not met its requirements yet.");
                return;
            }

            if (ActiveQuests.ContainsKey(questId))
                return;

            QuestSO questData = status.QuestSO ?? QuestDatabase?.GetQuest(questId);

            if (questData == null)
            {
                Debug.LogWarning($"[QuestManager] Quest data for '{questId}' not found.");
                return;
            }

            status.SetInProgress();

            var questRuntime = new Quest_Runtime(questData);
            questRuntime.OnStepChanged += stepIndex =>
            {
                OnQuestStepChanged?.Invoke(questId, stepIndex);
                RefreshQuestObjects();
            };
            questRuntime.OnObjectiveProgressed += objective =>
            {
                OnObjectiveProgressed?.Invoke(objective);
                RefreshQuestObjects();
            };
            questRuntime.OnObjectiveCompleted += objective =>
            {
                OnObjectiveCompleted?.Invoke(objective);
                RefreshQuestObjects();
            };
            questRuntime.OnQuestCompleted += () => QuestCompletedHandler(questId);

            ActiveQuests.Add(questId, questRuntime);

            if (questRuntime.QuestSteps_Runtime.Count > 0)
                questRuntime.ActivateStep(0);
            else
                QuestCompletedHandler(questId);

            Debug.Log($"[QuestManager] Quest '{questId}' started.");
            OnQuestTaken?.Invoke(questId);
            RefreshQuestObjects();

            if (string.IsNullOrEmpty(trackedQuest))
                SetQuestAsTracked(questId);
        }

        [TitleGroup("Tracked Quest")]
        [Button]
        public void SetQuestAsTracked(string questId)
        {
            if (string.IsNullOrEmpty(questId))
                return;

            ActiveQuests ??= new Dictionary<string, Quest_Runtime>();
            if (!ActiveQuests.ContainsKey(questId))
                return;

            trackedQuest = questId;
            OnTrackedQuestChanged?.Invoke(trackedQuest);
        }

        [TitleGroup("Tracked Quest")]
        [Button]
        public void UntrackQuest()
        {
            trackedQuest = "";
            OnTrackedQuestChanged?.Invoke(trackedQuest);
        }

        [TitleGroup("Debug Function")]
        [Button]
        public void CompleteQuest(string questId)
        {
            if (string.IsNullOrEmpty(questId))
                return;

            if (ActiveQuests == null || !ActiveQuests.ContainsKey(questId))
            {
                Debug.LogWarning($"[QuestManager] QuestId '{questId}' is not active.");
                return;
            }

            if (trackedQuest == questId)
                UntrackQuest();

            var questRuntime = ActiveQuests[questId];
            questRuntime.Deactivate();
            ActiveQuests.Remove(questId);

            if (QuestStatusDict != null && QuestStatusDict.TryGetValue(questId, out var status))
                status.SetCompleted();

            OnQuestCompleted?.Invoke(questId);
            RefreshQuestObjects();
        }

        public Quest_Runtime GetActiveQuest(string questId)
        {
            ActiveQuests ??= new Dictionary<string, Quest_Runtime>();
            return ActiveQuests.TryGetValue(questId, out var quest) ? quest : null;
        }

        public QuestStatus_Runtime GetQuestStatus(string questId)
        {
            QuestStatusDict ??= new Dictionary<string, QuestStatus_Runtime>();
            return QuestStatusDict.TryGetValue(questId, out var status) ? status : null;
        }

        void QuestCompletedHandler(string questId)
        {
            if (ActiveQuests == null || !ActiveQuests.ContainsKey(questId))
                return;

            if (trackedQuest == questId)
                UntrackQuest();

            ActiveQuests.Remove(questId);

            if (QuestStatusDict != null && QuestStatusDict.TryGetValue(questId, out var status))
                status.SetCompleted();

            Debug.Log($"[QuestManager] Quest '{questId}' completed.");
            OnQuestCompleted?.Invoke(questId);
            RefreshQuestObjects();
        }

        void SubscribeAllRequirements()
        {
            UnsubscribeAllRequirements();

            if (QuestDatabase?.Quests == null)
                return;

            foreach (var quest in QuestDatabase.Quests)
            {
                if (quest?.questRequirements == null)
                    continue;

                foreach (var entry in quest.questRequirements)
                {
                    var eventSO = entry?.requirementEventSO;
                    if (eventSO == null || _requirementHandlers.ContainsKey(eventSO))
                        continue;

                    var requirementId = eventSO.RequirementId;
                    Action<QuestRequirementParameter> handler = param =>
                    {
                        if (string.IsNullOrEmpty(requirementId))
                            return;

                        TrackedRequirementDict[requirementId] = param;
                        RefreshQuestStatusDict();
                    };

                    eventSO.Subscribe(handler);
                    _requirementHandlers[eventSO] = handler;
                }
            }
        }

        void UnsubscribeAllRequirements()
        {
            foreach (var pair in _requirementHandlers)
                pair.Key.Unsubscribe(pair.Value);

            _requirementHandlers.Clear();
        }

        void OnDestroy()
        {
            UnsubscribeAllRequirements();

            if (ActiveQuests == null)
                return;

            foreach (var quest in ActiveQuests.Values)
                quest.Deactivate();

            ActiveQuests.Clear();
        }
    }

    public enum QuestCompletionState
    {
        NotStarted,
        CanBeTaken,
        InProgress,
        Completed
    }
}
