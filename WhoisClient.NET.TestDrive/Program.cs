using Whois.NET;
using static System.Console;
using static System.Runtime.InteropServices.RuntimeInformation;

WriteLine($"WhoisClient.NET - TestDrive - {FrameworkDescription}");

SyncVersion();
await AsyncVersion();

static void SyncVersion()
{
    WriteLine("\n---- Sync version ----");
    var result = WhoisClient.Query("8.8.8.8");

    WriteLine($"{result.AddressRange.Begin} - {result.AddressRange.End}");
    WriteLine(result.OrganizationName);
    WriteLine(string.Join(" > ", result.RespondedServers));
}

static async Task AsyncVersion()
{
    WriteLine("\n---- Async version ----");
    var result = await WhoisClient.QueryAsync("8.8.8.8");

    WriteLine($"{result.AddressRange.Begin} - {result.AddressRange.End}");
    WriteLine(result.OrganizationName);
    WriteLine(string.Join(" > ", result.RespondedServers));
}
