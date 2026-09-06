using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RAXY.Quest
{
    public abstract class QuestBindingBase : MonoBehaviour
    {
        protected QuestManagerBase questManager;

        public virtual void Set_QuestManager(QuestManagerBase questMan)
        {
            questManager = questMan;
        }

        void Start()
        {
            questManager = QuestManagerBase.BaseInstance;
        }

        [TitleGroup("Settings")]
        [ValueDropdown(nameof(EditorQuestIds), AppendNextDrawer = true)]
        public string questId;

#if UNITY_EDITOR
        [TitleGroup("Editor Data")]
        [SerializeField]
        Object editor_QuestDb;

        [TitleGroup("Editor Data")]
        [Button]
        void Find_QuestDatabaseSO()
        {
            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (so is IQuestDatabase)
                {
                    editor_QuestDb = so;
                    return;
                }
            }

            editor_QuestDb = null;
        }

        protected IQuestDatabase QuestDatabase
        {
            get
            {
                if (editor_QuestDb is IQuestDatabase questDb)
                    return questDb;
                return null;
            }
        }
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
