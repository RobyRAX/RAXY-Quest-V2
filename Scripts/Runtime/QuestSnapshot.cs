using System;
using System.Collections.Generic;

namespace RAXY.Quest
{
    [Serializable]
    public sealed class QuestSnapshot
    {
        public int version = 1;
        public string trackedQuestId;
        public List<string> completedQuestIds = new();
        public List<QuestRequirementSnapshot> requirements = new();
        public List<QuestRuntimeSnapshot> activeQuests = new();
    }

    [Serializable]
    public sealed class QuestRequirementSnapshot
    {
        public string requirementId;
        public QuestRequirementParameter value;
    }

    [Serializable]
    public sealed class QuestRuntimeSnapshot
    {
        public string questId;
        public string definitionSignature;
        public int currentStepIndex;
        public List<QuestStepSnapshot> steps = new();
    }

    [Serializable]
    public sealed class QuestStepSnapshot
    {
        public QuestCompletionState state;
        public int activeMainObjectiveIndex;
        public List<QuestObjectiveSnapshot> mainObjectives = new();
        public List<QuestObjectiveSnapshot> optionalObjectives = new();
    }

    [Serializable]
    public sealed class QuestObjectiveSnapshot
    {
        public QuestCompletionState state;
        public float currentAmount;
    }
}
