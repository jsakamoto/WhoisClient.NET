using System.Threading.Tasks;
using Whois.NET;
using Xunit;

namespace WhoisClient_NET.Test
{
    public class IpTests
    {
        [Theory]
        [InlineData(@"4.4.4.4", @"Level 3 Communications, Inc. LVLT-STATIC-4-4-16 (NET-4-4-0-0-1)", @"4.4.0.0-4.4.255.255")]
        [InlineData(@"65.100.170.169", @"Alana Scheffield USW-FFCF (NET-65-100-170-168-1)", @"65.100.170.168-65.100.170.175")]
        [InlineData(@"108.234.177.20", @"AT&T Corp.", @"108.192.0.0-108.255.255.255")]
        [InlineData(@"190.190.132.64", @"Telecom Argentina S.A.", @"190.0.0.0-190.1.255.255")]
        [InlineData(@"31.116.94.96", @"EE route", @"31.64.0.0-31.127.255.255")]
        public void WhoisClientTest(string ip, string expectedOrgName, string expectedAddressRange)
        {
            var response = WhoisClient.Query(ip);
            response.OrganizationName.Is(expectedOrgName);
            response.AddressRange.ToString().Is(expectedAddressRange);
        }

        [Theory]
        [InlineData(@"4.4.4.4", @"Level 3 Communications, Inc. LVLT-STATIC-4-4-16 (NET-4-4-0-0-1)", @"4.4.0.0-4.4.255.255")]
        [InlineData(@"65.100.170.169", @"Alana Scheffield USW-FFCF (NET-65-100-170-168-1)", @"65.100.170.168-65.100.170.175")]
        [InlineData(@"108.234.177.20", @"AT&T Corp.", @"108.192.0.0-108.255.255.255")]
        [InlineData(@"190.190.132.64", @"Telecom Argentina S.A.", @"190.0.0.0-190.1.255.255")]
        [InlineData(@"31.116.94.96", @"EE route", @"31.64.0.0-31.127.255.255")]
        public async Task WhoisClientAsyncTest(string ip, string expectedOrgName, string expectedAddressRange)
        {
            var response = await WhoisClient.QueryAsync(ip).ConfigureAwait(false);
            response.OrganizationName.Is(expectedOrgName);
            response.AddressRange.ToString().Is(expectedAddressRange);
        }
    }
}
