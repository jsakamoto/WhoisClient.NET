using System;
using System.Threading.Tasks;
using Whois.NET;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("WhoisClient.NET - TestDrive - NETFX45 Async");

        SyncVersion();
        AsyncVersion().Wait();
    }

    private static void SyncVersion()
    {
        Console.WriteLine("\n---- Sync version ----");
        var result = WhoisClient.Query("8.8.8.8");

        Console.WriteLine("{0} - {1}", result.AddressRange.Begin, result.AddressRange.End);
        Console.WriteLine("{0}", result.OrganizationName);
        Console.WriteLine(string.Join(" > ", result.RespondedServers));
    }

    private static async Task AsyncVersion()
    {
        Console.WriteLine("\n---- Async version ----");
        var result = await WhoisClient.QueryAsync("8.8.8.8");

        Console.WriteLine("{0} - {1}", result.AddressRange.Begin, result.AddressRange.End);
        Console.WriteLine("{0}", result.OrganizationName);
        Console.WriteLine(string.Join(" > ", result.RespondedServers));
    }
}
