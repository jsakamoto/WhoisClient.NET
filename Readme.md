# WhoisClient.NET [![NuGet Package](https://img.shields.io/nuget/v/WhoisClient.NET.svg)](https://www.nuget.org/packages/WhoisClient.NET/) [![Build status](https://ci.appveyor.com/api/projects/status/lufktg9k1i5khpqp?svg=true)](https://ci.appveyor.com/project/jsakamoto/whoisclient-net)

## Project Description

This is .NET Class library implementing a WHOIS client.

## How to install

To install this library into your application, use the NuGet repository.

```
PM> Install-Package WhoisClient.NET
```

## Sample source code (C#)

```csharp
using Whois.NET;
...
var result = WhoisClient.Query("192.41.192.40");

Console.WriteLine("{0} - {1}", result.AddressRange.Begin, result.AddressRange.End); // "199.71.0.0 - 199.71.0.255"
Console.WriteLine("{0}", result.OrganizationName); // "American Registry for Internet Numbers"
Console.WriteLine(string.Join(" > ", result.RespondedServers)); // "whois.arin.net" 
```
