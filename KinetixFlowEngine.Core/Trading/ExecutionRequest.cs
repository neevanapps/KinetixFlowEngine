using KinetixFlowEngine.Core.Strategy;

namespace KinetixFlowEngine.Core.Trading
{
    public class ExecutionRequest
    {
        public string AccountId { get; set; } = default!;
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string StrategyName { get; set; } = default!;
        public SignalDirection Direction { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal StopLoss { get; set; }
        public decimal TakeProfit { get; set; }
    }

}