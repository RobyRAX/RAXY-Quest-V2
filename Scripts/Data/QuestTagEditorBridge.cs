using System;
using System.Collections.Generic;

namespace RAXY.Quest
{
    /// <summary>
    /// Editor registers a provider so runtime QuestSO can show tag ValueDropdown
    /// without referencing the Editor assembly.
    /// </summary>
    public static class QuestTagEditorBridge
    {
        static Func<IReadOnlyList<string>> _provider;

        public static void SetProvider(Func<IReadOnlyList<string>> provider)
        {
            _provider = provider;
        }

        public static IReadOnlyList<string> GetTags()
        {
            var tags = _provider?.Invoke();
            if (tags != null && tags.Count > 0)
                return tags;

            return Array.Empty<string>();
        }
    }
}
