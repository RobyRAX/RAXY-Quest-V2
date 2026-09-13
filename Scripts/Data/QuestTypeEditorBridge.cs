using System;
using System.Collections.Generic;

namespace RAXY.Quest
{
    /// <summary>
    /// Editor registers a provider so runtime QuestSO can show a ValueDropdown
    /// without referencing the Editor assembly.
    /// </summary>
    public static class QuestTypeEditorBridge
    {
        static Func<IReadOnlyList<string>> _provider;

        public static void SetProvider(Func<IReadOnlyList<string>> provider)
        {
            _provider = provider;
        }

        public static IReadOnlyList<string> GetTypes()
        {
            var types = _provider?.Invoke();
            if (types != null && types.Count > 0)
                return types;

            return new[] { QuestTypeIds.Main };
        }
    }
}
