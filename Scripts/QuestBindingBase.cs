using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RAXY.Quest
{
    public abstract class QuestBindingBase : MonoBehaviour
    {
        protected QuestManagerBase questManager;

        public virtual void Set_QuestManager(QuestManagerBase questMan)
        {
            questManager = questMan;
        }

        [TitleGroup("Editor Data")]
        [SerializeField]
        Object editor_QuestDb;

        protected IQuestDatabase QuestDatabase
        {
            get
            {
                if (editor_QuestDb is IQuestDatabase questDb)
                    return questDb;

                return null;
            }
        }

        [TitleGroup("Settings")]
#if UNITY_EDITOR
        [ValueDropdown(nameof(EditorQuestIds), AppendNextDrawer = true)]
#endif
        public string questId;

#if UNITY_EDITOR
        protected IEnumerable<string> EditorQuestIds
        {
            get
            {
                if (QuestDatabase?.Quests == null)
                    yield break;

                for (int i = 0; i < QuestDatabase.Quests.Count; i++)
                {
                    var quest = QuestDatabase.Quests[i];
                    if (quest != null)
                        yield return quest.QuestId;
                }
            }
        }

        protected QuestSO GetEditorQuest()
        {
            if (QuestDatabase == null || string.IsNullOrEmpty(questId))
                return null;

            return QuestDatabase.GetQuest(questId);
        }
#endif
    }
}
