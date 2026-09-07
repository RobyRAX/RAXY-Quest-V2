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
        Vector2 _listScroll;
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
            // Stale MonoBehaviour reference after exiting play mode.
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
                DrawManagerSection();
                EditorGUILayout.Space(8f);
                DrawToolbar();
                EditorGUILayout.Space(4f);
                DrawQuestList();
            }
        }

        void DrawManagerSection()
        {
            // Clear destroyed play-mode refs that still sit in the static field.
            if (!ReferenceEquals(QuestManagerBase.BaseInstance, null) &&
                QuestManagerBase.BaseInstance == null)
            {
                QuestManagerBase.BaseInstance = null;
            }

            var manager = QuestManagerBase.BaseInstance;
            bool hasManager = manager != null;

            if (hasManager)
            {
                EditorGUILayout.HelpBox(
                    $"Runtime mode — linked to '{manager.name}' ({manager.GetType().Name}).",
                    MessageType.Info);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Find Quest Manager Instance", GUILayout.Height(22f)))
                        FindQuestManagerInstance();

                    if (GUILayout.Button("Clear Reference", GUILayout.Width(120f), GUILayout.Height(22f)))
                        QuestManagerBase.BaseInstance = null;
                }

                EditorGUILayout.ObjectField("Instance", manager, typeof(QuestManagerBase), true);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Editor mode — QuestManagerBase.BaseInstance is not set.\n" +
                    "Enter Play Mode (with QuestManager in the scene), then Find Instance to enable Take/Complete.",
                    MessageType.Warning);

                if (GUILayout.Button("Find Quest Manager Instance", GUILayout.Height(24f)))
                    FindQuestManagerInstance();
            }
        }

        void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _filter = EditorGUILayout.TextField("Filter", _filter);
                if (GUILayout.Button("Refresh List", GUILayout.Width(100f)))
                    RefreshQuestList();
            }

            EditorGUILayout.LabelField($"Quests: {_entries.Count}", EditorStyles.miniLabel);
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
            _listScroll = EditorGUILayout.BeginScrollView(_listScroll, GUILayout.ExpandHeight(true));

            foreach (var entry in _entries)
            {
                if (entry.Quest == null)
                    continue;

                if (!PassesFilter(entry))
                    continue;

                DrawQuestRow(entry, hasManager);
            }

            EditorGUILayout.EndScrollView();
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

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(questId, EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(quest.questType.ToString(), EditorStyles.miniLabel, GUILayout.Width(48f));

                    if (hasManager)
                        EditorGUILayout.LabelField(GetStatusLabel(questId), EditorStyles.miniLabel, GUILayout.Width(90f));
                }

                EditorGUILayout.LabelField("Folder", entry.FolderPath, EditorStyles.miniLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Ping", GUILayout.Width(50f)))
                    {
                        EditorGUIUtility.PingObject(quest);
                        Selection.activeObject = quest;
                    }

                    using (new EditorGUI.DisabledScope(!hasManager))
                    {
                        if (GUILayout.Button("Take Quest"))
                            TakeQuest(questId);

                        if (GUILayout.Button("Complete Quest"))
                            CompleteQuest(questId);
                    }
                }

                if (!hasManager)
                {
                    EditorGUILayout.LabelField(
                        "Take / Complete disabled — Find Quest Manager Instance first.",
                        EditorStyles.centeredGreyMiniLabel);
                }
            }
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
