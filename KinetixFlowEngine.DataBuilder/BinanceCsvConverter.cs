using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace KinetixFlowEngine.DataBuilder
{
    public class BinanceCsvConverter
    {
        public void ConvertCsvToBinary(string csvFile, string outputFile)
        {
            Console.WriteLine($"Converting {Path.GetFileName(csvFile)}");

            using var reader = new StreamReader(csvFile);
            using var writer = new TradeBinaryWriter(outputFile);

            string? line;

            // Skip header row
            reader.ReadLine();

            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',');

                if (parts.Length < 7)
                    continue;

                var trade = new ReplayTrade
                {
                    Price = double.Parse(parts[1], CultureInfo.InvariantCulture),
                    Quantity = double.Parse(parts[2], CultureInfo.InvariantCulture),
                    Timestamp = long.Parse(parts[5]),
                    IsBuyerMaker = bool.Parse(parts[6])
                };

                writer.Write(trade);
            }
        }
    }
}
