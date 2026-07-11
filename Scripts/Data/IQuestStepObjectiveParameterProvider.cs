using System.Collections.Generic;

namespace RAXY.Quest
{
    public interface IQuestStepObjectiveParameterProvider
    {
        public List<string> Parameters { get; }
    }
}
