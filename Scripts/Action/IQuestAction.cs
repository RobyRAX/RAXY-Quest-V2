using System.Threading;
using Cysharp.Threading.Tasks;

namespace RAXY.Quest
{
    public interface IQuestAction
    {
        string Label { get; }

        UniTask ExecuteAsync(CancellationToken ct = default);
    }
}
