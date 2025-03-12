using System.Net.Sockets;
using NUnit.Framework;
using Whois.NET;

namespace WhoisClient_NET.Test;

[Parallelizable(ParallelScope.All)]
public class IpTests
{
    public static object[][] IpTestCases = new object[][]
    {
        // string ip, string expectedOrgName, string expectedAddressRange
        new object[] { @"4.4.4.4", @"Level 3 Parent, LLC", @"4.0.0.0-4.127.255.255" },
        new object[] { @"65.100.170.169", @"CenturyLink Communications, LLC", @"65.100.0.0-65.103.255.255" },
        new object[] { @"108.234.177.20", @"AT&T Enterprises, LLC", @"108.192.0.0-108.255.255.255" },
        new object[] { @"31.116.94.96", @"EE Limited", @"31.90.0.0-31.127.255.255" },
        new object[] { @"104.130.122.229", @"Rackspace Hosting", "104.130.0.0-104.130.255.255" },
        new object[] { @"2600::", @"Sprint", "2600::-2600:f:ffff:ffff:ffff:ffff:ffff:ffff" },
    };

    [Test]
    [TestCaseSource(nameof(IpTestCases))]
    public void WhoisClientTest(string ip, string expectedOrgName, string expectedAddressRange)
    {
        var response = WhoisClient.Query(ip, new WhoisQueryOptions());
        response.OrganizationName.Is(expectedOrgName);
        response.AddressRange.ToString().Is(expectedAddressRange);
    }

    [Test]
    [TestCaseSource(nameof(IpTestCases))]
    public async Task WhoisClientAsyncTest(string ip, string expectedOrgName, string expectedAddressRange)
    {
        var response = await WhoisClient.QueryAsync(ip, new WhoisQueryOptions());
        response.OrganizationName.Is(expectedOrgName);
        response.AddressRange.ToString().Is(expectedAddressRange);
    }

    [Test]
    public void RawQuery_InvalidWhoisServerSpecified_TransportExceptionSwallowed()
    {
        var response = WhoisClient.RawQuery("4.4.4.4", new WhoisQueryOptions { Server = "unknown.server.pp" });

        response.Is(string.Empty);
    }

    [Test]
    public void RawQuery_InvalidWhoisServerSpecified_TransportExceptionRethrown()
    {
        TestDelegate action = () => WhoisClient.RawQuery("4.4.4.4", new WhoisQueryOptions
        {
            Server = "unknown.server.pp",
            RethrowExceptions = true
        });

        var exception = Assert.Throws<AggregateException>(action)!;
        exception.InnerException.IsInstanceOf<SocketException>();
    }

    [Test]
    public void Query_With3Retries_InvalidWhoisServerSpecified_TransportExceptionSwallowed()
    {
        var response = WhoisClient.Query("4.4.4.4", new WhoisQueryOptions
        {
            Server = "unknown.server.pp",
            Retries = 3
        });

        response.Raw.Is(string.Empty);
    }

    [Test]
    public void Query_With3Retries_InvalidWhoisServerSpecified_TransportExceptionRethrown()
    {
        TestDelegate action = () => WhoisClient.Query("4.4.4.4", new WhoisQueryOptions
        {
            Server = "unknown.server.pp",
            Retries = 3,
            RethrowExceptions = true
        });

        var exception = Assert.Throws<AggregateException>(action)!;
        exception.InnerException.IsInstanceOf<SocketException>();
    }

    [Test]
    public async Task RawQueryAsync_InvalidWhoisServerSpecified_TransportExceptionSwallowed()
    {
        var response = await WhoisClient.RawQueryAsync("4.4.4.4", new WhoisQueryOptions { Server = "unknown.server.pp" });

        response.Is(string.Empty);
    }

    [Test]
    public void RawQueryAsync_InvalidWhoisServerSpecified_TransportExceptionRethrown()
    {
        AsyncTestDelegate action = () => WhoisClient.RawQueryAsync("4.4.4.4", new WhoisQueryOptions
        {
            Server = "unknown.server.pp",
            RethrowExceptions = true
        });

        Assert.ThrowsAsync<SocketException>(action);
    }

    [Test]
    public async Task QueryAsync_With3Retries_InvalidWhoisServerSpecified_TransportExceptionSwallowed()
    {
        var response = await WhoisClient.QueryAsync("4.4.4.4", new WhoisQueryOptions
        {
            Server = "unknown.server.pp",
            Retries = 3
        });

        response.Raw.Is(string.Empty);
    }

    [Test]
    public void QueryAsync_With3Retries_InvalidWhoisServerSpecified_TransportExceptionRethrown()
    {
        AsyncTestDelegate action = () => WhoisClient.QueryAsync("4.4.4.4", new WhoisQueryOptions
        {
            Server = "unknown.server.pp",
            Retries = 3,
            RethrowExceptions = true
        });

        Assert.ThrowsAsync<SocketException>(action);
    }
}
