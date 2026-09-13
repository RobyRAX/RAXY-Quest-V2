using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RAXY.Quest.Editor
{
    /// <summary>
    /// Project-scoped Quest editor settings (Project Hub).
    /// Saved under ProjectSettings/RaxyQuestEditorSettings.asset.
    /// </summary>
    [FilePath("ProjectSettings/RaxyQuestEditorSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class QuestEditorSettings : ScriptableSingleton<QuestEditorSettings>
    {
        public const string DefaultGeneratedScriptPath = "Assets/RAXY Generated/QuestTypes.cs";

        [SerializeField]
        List<string> questTypes = new() { QuestTypeIds.Main, "Side" };

        [SerializeField]
        string generatedScriptPath = DefaultGeneratedScriptPath;

        [SerializeField]
        string generatedNamespace = "RAXY.Quest";

        public IReadOnlyList<string> QuestTypes
        {
            get
            {
                EnsureMainLocked();
                return questTypes;
            }
        }

        public string GeneratedScriptPath
        {
            get => string.IsNullOrWhiteSpace(generatedScriptPath)
                ? DefaultGeneratedScriptPath
                : generatedScriptPath;
            set => generatedScriptPath = value;
        }

        public string GeneratedNamespace
        {
            get => generatedNamespace ?? string.Empty;
            set => generatedNamespace = value ?? string.Empty;
        }

        public List<string> MutableQuestTypes
        {
            get
            {
                EnsureMainLocked();
                return questTypes;
            }
        }

        [InitializeOnLoadMethod]
        static void RegisterBridge()
        {
            QuestTypeEditorBridge.SetProvider(() => instance.QuestTypes);
            instance.EnsureMainLocked();
        }

        public void EnsureMainLocked()
        {
            if (questTypes == null)
                questTypes = new List<string>();

            // Remove duplicate Mains, keep one at index 0.
            for (int i = questTypes.Count - 1; i >= 0; i--)
            {
                if (string.Equals(questTypes[i], QuestTypeIds.Main, StringComparison.Ordinal)
                    && i != 0)
                {
                    questTypes.RemoveAt(i);
                }
            }

            if (questTypes.Count == 0
                || !string.Equals(questTypes[0], QuestTypeIds.Main, StringComparison.Ordinal))
            {
                questTypes.RemoveAll(t =>
                    string.Equals(t, QuestTypeIds.Main, StringComparison.Ordinal));
                questTypes.Insert(0, QuestTypeIds.Main);
            }
        }

        public void SaveSettings() => Save(true);
    }
}
