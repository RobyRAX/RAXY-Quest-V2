using UnityEngine;

namespace RAXY.Quest
{
    public static class QuestRuntimeInspectorColors
    {
        const float StatusBlend = 0.55f;

        static readonly Color ZebraEvenColor = new Color(0.18f, 0.18f, 0.18f, 1f);
        static readonly Color ZebraOddColor = new Color(0.26f, 0.26f, 0.26f, 1f);

        static readonly Color ActiveTint = new Color(0.22f, 0.42f, 0.26f, 1f);
        static readonly Color CompletedTint = new Color(0.22f, 0.32f, 0.48f, 1f);

        public static Color GetMainObjectiveElementColor(QuestStep_Runtime step, int index, Color defaultColor)
        {
            if (step?.MainObjectives == null || index < 0 || index >= step.MainObjectives.Count)
                return GetZebraColor(index);

            var objective = step.MainObjectives[index];

            if (objective.IsCompleted)
                return BlendWithZebra(index, CompletedTint);

            if (step.StepMode == QuestStepMode.Sequential)
            {
                if (index == step.ActiveMainObjectiveIndex && objective.State == QuestCompletionState.InProgress)
                    return BlendWithZebra(index, ActiveTint);

                return GetZebraColor(index);
            }

            if (objective.State == QuestCompletionState.InProgress)
                return BlendWithZebra(index, ActiveTint);

            return GetZebraColor(index);
        }

        public static Color GetOptionalObjectiveElementColor(QuestStep_Runtime step, int index, Color defaultColor)
        {
            if (step?.OptionalObjectives == null || index < 0 || index >= step.OptionalObjectives.Count)
                return GetZebraColor(index);

            var objective = step.OptionalObjectives[index];

            if (objective.IsCompleted)
                return BlendWithZebra(index, CompletedTint);

            if (step.State == QuestCompletionState.InProgress &&
                objective.State == QuestCompletionState.InProgress)
                return BlendWithZebra(index, ActiveTint);

            return GetZebraColor(index);
        }

        public static Color GetQuestStepElementColor(Quest_Runtime quest, int index, Color defaultColor)
        {
            if (quest?.QuestSteps_Runtime == null || index < 0 || index >= quest.QuestSteps_Runtime.Count)
                return GetZebraColor(index);

            var step = quest.QuestSteps_Runtime[index];

            if (step.State == QuestCompletionState.Completed)
                return BlendWithZebra(index, CompletedTint);

            if (index == quest.CurrentQuestStepIndex && step.State == QuestCompletionState.InProgress)
                return BlendWithZebra(index, ActiveTint);

            return GetZebraColor(index);
        }

        static Color GetZebraColor(int index) =>
            index % 2 == 0 ? ZebraEvenColor : ZebraOddColor;

        static Color BlendWithZebra(int index, Color statusTint) =>
            Color.Lerp(GetZebraColor(index), statusTint, StatusBlend);
    }
}
