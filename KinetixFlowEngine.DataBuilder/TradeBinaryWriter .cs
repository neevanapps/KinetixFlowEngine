using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.DataBuilder
{
    public class TradeBinaryWriter : IDisposable
    {
        private readonly BinaryWriter _writer;

        public TradeBinaryWriter(string file)
        {
            _writer = new BinaryWriter(File.Open(file, FileMode.Create, FileAccess.Write));
        }

        public void Write(ReplayTrade trade)
        {
            _writer.Write(trade.Timestamp);
            _writer.Write(trade.Price);
            _writer.Write(trade.Quantity);
            _writer.Write(trade.IsBuyerMaker);
        }

        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}
