using System;
using Whois.NET;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("WhoisClient.NET - TestDrive - NETFX40");
        var result = WhoisClient.Query("8.8.8.8");

        Console.WriteLine("{0} - {1}", result.AddressRange.Begin, result.AddressRange.End);
        Console.WriteLine("{0}", result.OrganizationName);
        Console.WriteLine(string.Join(" > ", result.RespondedServers));
    }
}
