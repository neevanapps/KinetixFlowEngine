using KinetixFlowEngine.Core.Engine;
using KinetixFlowEngine.Core.Signal;
using KinetixFlowEngine.Core.Strategy;
using KinetixFlowEngine.Core.Trading;

namespace KinetixFlowEngine.Core.Prop
{
    public class AccountRuntime
    {
        public PropAccountConfig Config { get; set; } = default!;
        public PropAccountState State { get; set; } = new();
        public PropChallengeGuard Guard { get; set; } = new();
    }
}