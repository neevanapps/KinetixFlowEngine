using KinetixFlowEngine.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace KinetixFlowEngine.Core.Data
{
    public class ReplayTradeStreamClient : ITradeStreamClient
    {
        private readonly string _dataFolder;
        public bool Completed { get; private set; }
        public event Action<FlowTrade>? OnTrade;

        public ReplayTradeStreamClient(string dataFolder)
        {
            _dataFolder = dataFolder;
        }

        public async Task StartAsync(CancellationToken ct)
        {
            var files = Directory
                .GetFiles(_dataFolder, "*.bin")
                .OrderBy(f => f);

            foreach (var file in files)
            {
                Console.WriteLine($"Replaying {Path.GetFileName(file)}");

                using var reader = new BinaryReader(File.OpenRead(file));

                while (!ct.IsCancellationRequested &&
                       reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    var trade = new FlowTrade
                    {
                        Timestamp = reader.ReadInt64(),
                        Price = (decimal)reader.ReadDouble(),
                        Quantity = (decimal)reader.ReadDouble(),
                        IsBuyerMaker = reader.ReadBoolean()
                    };

                    OnTrade?.Invoke(trade);
                }
            }
            Completed=true;
            Console.WriteLine("Replay completed");
        }
    }
}
