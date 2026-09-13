using System;
using System.Collections.Generic;
using System.IO;
using RAXY.Utility.Editor.Hub;
using UnityEditor;
using UnityEngine;

namespace RAXY.Quest.Editor
{
    public sealed class QuestHubModule : IRaxyHubModule
    {
        struct QuestEntry
        {
            public QuestSO Quest;
            public string AssetPath;
            public string FolderPath;
        }

        readonly List<QuestEntry> _entries = new();
        Vector2 _scroll;
        string _filter = "";

        public string Id => "quest";
        public string DisplayName => "Quest";
        public int Order => 100;

        public void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            RefreshQuestList();
        }

        public void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
                QuestManagerBase.BaseInstance = null;

            EditorApplication.delayCall += () =>
            {
                if (EditorWindow.HasOpenInstances<RaxyProjectHubWindow>())
                    EditorWindow.GetWindow<RaxyProjectHubWindow>().Repaint();
            };
        }

        public void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.ExpandHeight(true)))
            {
                _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
                DrawManagerSection();
                EditorGUILayout.Space(10f);
                DrawToolbar();
                EditorGUILayout.Space(6f);
                DrawQuestList();
                EditorGUILayout.EndScrollView();
            }
        }

        void DrawManagerSection()
        {
            if (!ReferenceEquals(QuestManagerBase.BaseInstance, null) &&
                QuestManagerBase.BaseInstance == null)
            {
                QuestManagerBase.BaseInstance = null;
            }

            var manager = QuestManagerBase.BaseInstance;
            bool hasManager = manager != null;

            if (hasManager)
            {
                RaxyHubGui.DrawStatusBanner(
                    true,
                    "Runtime connected",
                    $"Linked to '{manager.name}' ({manager.GetType().Name}).");

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (RaxyHubGui.PrimaryButton("Find Quest Manager Instance"))
                        FindQuestManagerInstance();

                    if (RaxyHubGui.SecondaryButton("Clear", 80f))
                        QuestManagerBase.BaseInstance = null;
                }

                EditorGUILayout.Space(4f);
                EditorGUILayout.ObjectField("Instance", manager, typeof(QuestManagerBase), true);
            }
            else
            {
                RaxyHubGui.DrawStatusBanner(
                    false,
                    "Editor mode — manager not linked",
                    "Enter Play Mode with a QuestManager in the scene, then Find Instance to enable Take/Complete.");

                if (RaxyHubGui.PrimaryButton("Find Quest Manager Instance"))
                    FindQuestManagerInstance();
            }
        }

        void DrawToolbar()
        {
            _filter = RaxyHubGui.DrawToolbarRow(_filter, out bool refresh);
            if (refresh)
                RefreshQuestList();

            RaxyHubGui.DrawCountChip($"{_entries.Count} quests");
        }

        void DrawQuestList()
        {
            if (_entries.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "No QuestSO assets found. Create one via Assets > Create > RAXY > Quest > QuestSO.",
                    MessageType.Info);
                return;
            }

            bool hasManager = QuestManagerBase.BaseInstance != null;

            foreach (var entry in _entries)
            {
                if (entry.Quest == null)
                    continue;

                if (!PassesFilter(entry))
                    continue;

                DrawQuestRow(entry, hasManager);
            }

            if (!hasManager)
                RaxyHubGui.DrawHint("Take / Complete disabled until a Quest Manager is linked.");
        }

        bool PassesFilter(QuestEntry entry)
        {
            if (string.IsNullOrWhiteSpace(_filter))
                return true;

            string q = _filter.Trim();
            return entry.Quest.QuestId.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                   || entry.FolderPath.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0
                   || entry.Quest.questType.ToString().IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        void DrawQuestRow(QuestEntry entry, bool hasManager)
        {
            var quest = entry.Quest;
            string questId = quest.QuestId;

            RaxyHubGui.BeginCard();
            RaxyHubGui.DrawTitleRow(
                questId,
                quest.questType.ToString(),
                hasManager ? GetStatusLabel(questId) : null);
            RaxyHubGui.DrawMutedPath(entry.FolderPath);

            EditorGUILayout.Space(4f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (RaxyHubGui.SecondaryButton("Ping", 56f))
                {
                    EditorGUIUtility.PingObject(quest);
                    Selection.activeObject = quest;
                }

                using (new EditorGUI.DisabledScope(!hasManager))
                {
                    if (RaxyHubGui.PrimaryButton("Take Quest"))
                        TakeQuest(questId);

                    if (RaxyHubGui.PrimaryButton("Complete Quest"))
                        CompleteQuest(questId);
                }
            }

            RaxyHubGui.EndCard();
        }

        string GetStatusLabel(string questId)
        {
            var manager = QuestManagerBase.BaseInstance;
            if (manager == null)
                return "—";

            if (manager.ActiveQuests != null && manager.ActiveQuests.ContainsKey(questId))
                return "InProgress";

            if (manager.QuestStatusDict != null &&
                manager.QuestStatusDict.TryGetValue(questId, out var status) &&
                status != null)
                return status.CompletionState.ToString();

            return "Unknown";
        }

        void RefreshQuestList()
        {
            _entries.Clear();

            string[] guids = AssetDatabase.FindAssets("t:QuestSO");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var quest = AssetDatabase.LoadAssetAtPath<QuestSO>(path);
                if (quest == null)
                    continue;

                _entries.Add(new QuestEntry
                {
                    Quest = quest,
                    AssetPath = path,
                    FolderPath = Path.GetDirectoryName(path)?.Replace('\\', '/') ?? path
                });
            }

            _entries.Sort((a, b) =>
                string.Compare(a.Quest.QuestId, b.Quest.QuestId, StringComparison.OrdinalIgnoreCase));
        }

        void FindQuestManagerInstance()
        {
            var found = UnityEngine.Object.FindObjectsByType<QuestManagerBase>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            if (found == null || found.Length == 0)
            {
                Debug.LogWarning(
                    "[RAXY Hub / Quest] No QuestManagerBase found in loaded scenes. " +
                    "Enter Play Mode and ensure a QuestManager is present.");
                return;
            }

            if (found.Length > 1)
            {
                Debug.LogWarning(
                    $"[RAXY Hub / Quest] Found {found.Length} QuestManagerBase instances; using '{found[0].name}'.");
            }

            QuestManagerBase.BaseInstance = found[0];
            Debug.Log($"[RAXY Hub / Quest] Linked BaseInstance to '{found[0].name}'.");
            EditorWindow.GetWindow<RaxyProjectHubWindow>()?.Repaint();
        }

        void TakeQuest(string questId)
        {
            var manager = QuestManagerBase.BaseInstance;
            if (manager == null)
            {
                Debug.LogWarning("[RAXY Hub / Quest] BaseInstance is null.");
                return;
            }

            manager.TakeQuest(questId);
        }

        void CompleteQuest(string questId)
        {
            var manager = QuestManagerBase.BaseInstance;
            if (manager == null)
            {
                Debug.LogWarning("[RAXY Hub / Quest] BaseInstance is null.");
                return;
            }

            manager.CompleteQuest(questId);
        }
    }
}
