using KinetixFlowEngine.DataBuilder;
using System.IO.Compression;

string rawFolder = @"D:\Projects\Crypto\BTCUSDT";
string outputFolder = Path.Combine(rawFolder, "bin");

Directory.CreateDirectory(outputFolder);

var processor = new ZipProcessor();

var zipFiles = Directory.GetFiles(rawFolder, "*.zip");

Console.WriteLine($"Found {zipFiles.Length} zip files");

foreach (var zip in zipFiles.OrderBy(x => x))
{
    Console.WriteLine($"Processing {Path.GetFileName(zip)}");

    processor.ProcessZip(zip, outputFolder);
}

Console.WriteLine("Conversion completed");