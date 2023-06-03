using Whois.NET;
using Xunit;

namespace WhoisClient_NET.Test;

public class IpTests
{
    public static object[][] IpTestCases = new object[][]
    {
        // string ip, string expectedOrgName, string expectedAddressRange
        new object[] { @"4.4.4.4", @"Level 3 Parent, LLC", @"4.0.0.0-4.127.255.255" },
        new object[] { @"65.100.170.169", @"CenturyLink Communications, LLC", @"65.100.0.0-65.103.255.255" },
        new object[] { @"108.234.177.20", @"AT&T Corp.", @"108.192.0.0-108.255.255.255" },
        new object[] { @"31.116.94.96", @"EE route", @"31.64.0.0-31.127.255.255" },
        new object[] { @"104.130.122.229", @"Rackspace Hosting", "104.130.0.0-104.130.255.255" },
        new object[] { @"2600::", @"Sprint", "2600::-2600:f:ffff:ffff:ffff:ffff:ffff:ffff" },
    };

    [Theory]
    [MemberData(nameof(IpTestCases))]
    public void WhoisClientTest(string ip, string expectedOrgName, string expectedAddressRange)
    {
        var response = WhoisClient.Query(ip);
        response.OrganizationName.Is(expectedOrgName);
        response.AddressRange.ToString().Is(expectedAddressRange);
    }

    [Theory]
    [MemberData(nameof(IpTestCases))]
    public async Task WhoisClientAsyncTest(string ip, string expectedOrgName, string expectedAddressRange)
    {
        var response = await WhoisClient.QueryAsync(ip);
        response.OrganizationName.Is(expectedOrgName);
        response.AddressRange.ToString().Is(expectedAddressRange);
    }
}
