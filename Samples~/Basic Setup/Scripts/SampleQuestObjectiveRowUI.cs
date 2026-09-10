using RAXY.Quest;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SampleQuestObjectiveRowUI : MonoBehaviour
{
    const string NoDescription = "(No Description)";

    [SerializeField] Image statusIcon;
    [SerializeField] TextMeshProUGUI objectiveTmp;
    [SerializeField] Color activeColor = Color.white;
    [SerializeField] Color completedColor = new(0.7f, 0.7f, 0.7f, 1f);

    public void Setup(QuestStepObjective_Runtime objective, string fallbackLabel)
    {
        if (objectiveTmp != null)
            objectiveTmp.text = BuildLabel(objective, fallbackLabel);

        if (statusIcon != null)
            statusIcon.color = objective.IsCompleted ? completedColor : activeColor;
    }

    static string BuildLabel(QuestStepObjective_Runtime objective, string fallbackLabel)
    {
        var description = objective.ObjectiveDescription;
        if (string.IsNullOrEmpty(description) || description == NoDescription)
            description = fallbackLabel;

        if (objective.RequiredAmount > 1f)
        {
            description =
                $"{description} ({Mathf.FloorToInt(objective.CurrentAmount)}/{Mathf.FloorToInt(objective.RequiredAmount)})";
        }

        return description;
    }
}
