# WhoisClient.NET [![NuGet Package](https://img.shields.io/nuget/v/WhoisClient.NET.svg)](https://www.nuget.org/packages/WhoisClient.NET/) [![Build status](https://ci.appveyor.com/api/projects/status/lufktg9k1i5khpqp?svg=true)](https://ci.appveyor.com/project/jsakamoto/whoisclient-net)

## Project Description

This is .NET Class library implementing a WHOIS client.

## How to install

To install this library into your application, use the NuGet repository.

```
PM> Install-Package WhoisClient.NET
```

### *Note - for .NET Framework 4.0

`WhoisClient.NET` ver.2.x support "async" version methods for also .NET Framework 4.0 with powered by `Microsoft.Bcl.Async` NuGet package.

But if you don't want get dependencies for `Microsoft.Bcl.Async` and no need "async" version method, you can stay using v.1.x by like the follow install command.

```
PM> Install-Package WhoisClient.NET -Version 1.1.1
```

## Sample source code (C#)

### Async version

```csharp
using Whois.NET;
...
private async Task DoIt()
{
  var result = await WhoisClient.QueryAsync("8.8.8.8");
  
  Console.WriteLine("{0} - {1}", result.AddressRange.Begin, result.AddressRange.End); // "8.8.8.0 - 8.8.8.255"
  Console.WriteLine("{0}", result.OrganizationName); // "Google Inc. LVLT-GOGL-8-8-8 (NET-8-8-8-0-1)"
  Console.WriteLine(string.Join(" > ", result.RespondedServers)); // "whois.iana.org > whois.arin.net" 
}
```

### Sync version

```csharp
using Whois.NET;
...
private void DoIt()
{
  var result = WhoisClient.Query("8.8.8.8");
  
  Console.WriteLine("{0} - {1}", result.AddressRange.Begin, result.AddressRange.End); // "8.8.8.0 - 8.8.8.255"
  Console.WriteLine("{0}", result.OrganizationName); // "Google Inc. LVLT-GOGL-8-8-8 (NET-8-8-8-0-1)"
  Console.WriteLine(string.Join(" > ", result.RespondedServers)); // "whois.iana.org > whois.arin.net" 
}
```
