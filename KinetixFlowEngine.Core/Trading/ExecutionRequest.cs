using KinetixFlowEngine.Core.Strategy;

namespace KinetixFlowEngine.Core.Trading
{
    public class ExecutionRequest
    {
        public string AccountId { get; set; } = default!;
        public string StrategyName { get; set; } = default!;
        public SignalDirection Direction { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public decimal StopLoss { get; set; }
        public decimal TakeProfit { get; set; }
    }

    public class ExecutionResult
    {
        public bool Success { get; set; }
        public decimal FilledPrice { get; set; }
        public decimal FilledQuantity { get; set; }
        public string? Error { get; set; }
    }
}