using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

namespace KinetixFlowEngine.DataBuilder
{
    public class ZipProcessor
    {
        private readonly BinanceCsvConverter _converter = new();

        public void ProcessZip(string zipPath, string outputFolder)
        {
            using var archive = ZipFile.OpenRead(zipPath);

            foreach (var entry in archive.Entries)
            {
                if (!entry.FullName.EndsWith(".csv"))
                    continue;

                var tempCsv = Path.Combine(Path.GetTempPath(), entry.Name);

                entry.ExtractToFile(tempCsv, true);

                var outputFile = Path.Combine(
                    outputFolder,
                    entry.Name.Replace(".csv", ".bin"));

                _converter.ConvertCsvToBinary(tempCsv, outputFile);

                File.Delete(tempCsv);

                Console.WriteLine($"Saved {outputFile}");
            }
        }
    }
}
