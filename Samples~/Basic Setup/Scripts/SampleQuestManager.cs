using System.Collections.Generic;
using RAXY.Quest;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// Sample manager: raises a quest-completed requirement event so chained quests can unlock.
/// </summary>
public class SampleQuestManager : QuestManagerExample
{
    [TitleGroup("Sample Wiring")]
    [SerializeField]
    QuestRequirementEventSO questCompleteRequirementSO;

    protected override void OnQuestCompletedCallback(string questId)
    {
        base.OnQuestCompletedCallback(questId);

        if (questCompleteRequirementSO == null)
            return;

        var completedQuestIds = new List<string>();
        foreach (var quest in QuestStatusDict)
        {
            if (quest.Value.CompletionState == QuestCompletionState.Completed)
                completedQuestIds.Add(quest.Key);
        }

        questCompleteRequirementSO.Raise(new QuestRequirementParameter
        {
            listStringParam = completedQuestIds
        });
    }
}
