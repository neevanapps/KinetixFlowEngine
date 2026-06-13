using KinetixFlowEngine.Core.Gpt.Models;

namespace KinetixFlowEngine.Core.Gpt.Services;

public interface IGptSessionManager
{
    GptSession GetCurrentSession();

    void Save();

    int GetNextSequence();

    bool RequiresInitialization();

    void MarkInitialized(string threadId);
}