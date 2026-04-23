// Program.cs
using System;
using System.Threading.Tasks;

var mode = args.Length > 0 ? args[0] : "upload";

try
{
    if (mode == "csv")
    {
        var outputPath = args.Length > 1 ? args[1] : "games.csv";
        Console.WriteLine($"Mode: CSV export → {outputPath}");
        var exporter = new SteamCsvExportService();
        await exporter.ExportToCsvAsync(outputPath);
    }
    else if (mode == "enrich")
    {
        Console.WriteLine("Mode: Enrichment");
        var enricher = new SteamEnrichmentService();
        await enricher.EnrichAllAsync();
    }
    else
    {
        Console.WriteLine("Mode: API upload");
        Console.WriteLine("Initializing Steam Catalog Service...");
        var service = new SteamCatalogService();
        await service.UpdateGameListAsync();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"CRITICAL ERROR: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}
