namespace KinetixFlowEngine.Core.Execution
{
    public class ExecutionResult
    {
        public bool Success { get; set; }

        public decimal FilledPrice { get; set; }
        public decimal FilledQuantity { get; set; }

        public string OrderId { get; set; } = string.Empty;

        public string Error { get; set; } = string.Empty;
    }
}