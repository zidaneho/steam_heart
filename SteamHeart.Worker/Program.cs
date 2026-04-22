// Program.cs
using System;
using System.Threading.Tasks;

// Make sure this namespace matches where your "SteamCatalogService" is defined
// using SteamHeartAPI.Services; 

Console.WriteLine("Initializing Steam Catalog Service...");

try 
{
    // 1. Instantiate the service
    var service = new SteamCatalogService();

    // 2. Run the update
    await service.UpdateGameListAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"CRITICAL ERROR: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
}

Console.WriteLine("Job finished. Press any key to exit.");
Console.ReadKey();