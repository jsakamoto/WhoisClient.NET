# WhoisClient.NET [![NuGet Package](https://img.shields.io/nuget/v/WhoisClient.NET.svg)](https://www.nuget.org/packages/WhoisClient.NET/) [![unit tests](https://github.com/jsakamoto/WhoisClient.NET/actions/workflows/unit-tests.yml/badge.svg?branch=master&event=push)](https://github.com/jsakamoto/WhoisClient.NET/actions/workflows/unit-tests.yml)

## Project Description

This is .NET Class library implementing a WHOIS client.

## How to install

To install this library into your application, use the NuGet repository.

```
PM> Install-Package WhoisClient.NET
```

## Sample source code (C#)

### Async version

```csharp
using Whois.NET;
...
private async Task QueryByIPAddress()
{
  var result = await WhoisClient.QueryAsync("8.8.8.8");
  
  Console.WriteLine("{0} - {1}", result.AddressRange.Begin, result.AddressRange.End); // "8.8.8.0 - 8.8.8.255"
  Console.WriteLine("{0}", result.OrganizationName); // "Google Inc. LVLT-GOGL-8-8-8 (NET-8-8-8-0-1)"
  Console.WriteLine(string.Join(" > ", result.RespondedServers)); // "whois.iana.org > whois.arin.net" 
}

private async Task QueryByDomain()
{
  var result = await WhoisClient.QueryAsync("google.com");
  
  Console.WriteLine("{0}", result.OrganizationName); // "Google Inc."
  Console.WriteLine(string.Join(" > ", result.RespondedServers)); // "whois.iana.org > whois.verisign-grs.com > whois.markmonitor.com" 
}
```

### Sync version

```csharp
using Whois.NET;
...
private void QueryByIPAddress()
{
  var result = WhoisClient.Query("8.8.8.8");
  
  Console.WriteLine("{0} - {1}", result.AddressRange.Begin, result.AddressRange.End); // "8.8.8.0 - 8.8.8.255"
  Console.WriteLine("{0}", result.OrganizationName); // "Google Inc. LVLT-GOGL-8-8-8 (NET-8-8-8-0-1)"
  Console.WriteLine(string.Join(" > ", result.RespondedServers)); // "whois.iana.org > whois.arin.net" 
}

private async void QueryByDomain()
{
  var result = WhoisClient.Query("google.com");
  
  Console.WriteLine("{0}", result.OrganizationName); // "Google Inc."
  Console.WriteLine(string.Join(" > ", result.RespondedServers)); // "whois.iana.org > whois.verisign-grs.com > whois.markmonitor.com" 
}
```

## Supported Framework

### v.4.x

- .NET Standard 2.0
  - .NET 5 or later
  - .NET Core 2.0 or later
  - .NET Frameword v.4.6.2 or later

### v.3.x

- .NET Core 1.1 or later (.NET Standard 1.4)
- .NET Frameword v.4.5 or later

**NOTICE** - WhoisClient.NET v.3.x is no longer supported .NET Framework **4.0**.

### v.2.x

- .NET Frameword v.4.0 or later

**NOTICE** - WhoisClient.NET v.2.x isn't supported .NET Core.

#### Async version for .NET Framework 4.0

`WhoisClient.NET` ver.2.x support "async" version methods for also .NET Framework 4.0 with powered by `Microsoft.Bcl.Async` NuGet package.

But if you don't want get dependencies for `Microsoft.Bcl.Async` and no need "async" version method, you can stay using v.1.x by like the follow install command.

```
PM> Install-Package WhoisClient.NET -Version 1.1.1
```

## Release notes

The release notes is [here](https://github.com/jsakamoto/WhoisClient.NET/blob/master/RELEASE-NOTES.txt).

## License

[Microsoft Public License](https://github.com/jsakamoto/WhoisClient.NET/blob/master/License.md)
