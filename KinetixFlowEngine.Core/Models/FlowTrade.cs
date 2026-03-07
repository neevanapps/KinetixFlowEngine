namespace KinetixFlowEngine.Core.Models
{
    public class FlowTrade
    {
        public long TradeId { get; set; }

        public decimal Price { get; set; }

        public decimal Quantity { get; set; }

        public bool IsBuyerMaker { get; set; }

        public long Timestamp { get; set; }
    }
}