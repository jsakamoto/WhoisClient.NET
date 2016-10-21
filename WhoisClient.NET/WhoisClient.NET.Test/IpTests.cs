using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Whois.NET;

namespace WhoisClient_NET.Test
{
    using System.Threading.Tasks;

    [TestClass]
    public class IpTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCase(@"4.4.4.4", @"Level 3 Communications, Inc. LVLT-STATIC-4-4-16 (NET-4-4-0-0-1)", @"4.4.0.0-4.4.255.255")]
        [TestCase(@"65.100.170.169", @"Qwest Communications Company, LLC QWEST-INET-115 (NET-65-100-0-0-1)", @"65.100.0.0-65.103.255.255")]
        [TestCase(@"108.234.177.20", @"AT&T Internet Services", @"108.192.0.0-108.255.255.255")]
        [TestCase(@"190.190.132.64", @"Prima S.A.", @"190.0.0.0-190.1.255.255")]
        [TestCase(@"31.116.94.96", @"EE route", @"31.64.0.0-31.127.255.255")]
        public void WhoisClientTest()
        {
            TestContext.Run(
                (string ip, string expectedOrganizationName, string expectedAddressRange) =>
                    {
                        WhoisResponse response = WhoisClient.Query(ip);
                        Assert.AreEqual(expectedOrganizationName, response.OrganizationName);
                        Assert.AreEqual(expectedAddressRange, response.AddressRange.ToString());
                    });
        }

        [TestMethod]
        [TestCase(@"4.4.4.4", @"Level 3 Communications, Inc. LVLT-STATIC-4-4-16 (NET-4-4-0-0-1)", @"4.4.0.0-4.4.255.255")]
        [TestCase(@"65.100.170.169", @"Qwest Communications Company, LLC QWEST-INET-115 (NET-65-100-0-0-1)", @"65.100.0.0-65.103.255.255")]
        [TestCase(@"108.234.177.20", @"AT&T Internet Services", @"108.192.0.0-108.255.255.255")]
        [TestCase(@"190.190.132.64", @"Prima S.A.", @"190.0.0.0-190.1.255.255")]
        [TestCase(@"31.116.94.96", @"EE route", @"31.64.0.0-31.127.255.255")]
        public async Task WhoisClientAsyncTest()
        {
            await TestContext.RunAsync(
                async (string ip, string expectedOrganizationName, string expectedAddressRange) =>
                {
                    WhoisResponse response = await WhoisClient.QueryAsync(ip).ConfigureAwait(false);
                    Assert.AreEqual(expectedOrganizationName, response.OrganizationName);
                    Assert.AreEqual(expectedAddressRange, response.AddressRange.ToString());
                });
        }
    }
}
