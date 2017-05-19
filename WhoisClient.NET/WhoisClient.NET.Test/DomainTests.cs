using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Whois.NET;

namespace WhoisClient_NET.Test
{
    [TestClass]
    public class DomainTests
    {
        public TestContext TestContext { get; set; }

        /* facebook.com, microsoft.com, google.com, yahoo.com, github.com and most all large domains use MarkMonitor which rate limits queries.
         * If you are using a continuous testing solution then this test may fail from time to time.
         *
         * MarkMonitor.com for information purposes, and to assist persons in obtaining information about or related to a domain name registration record.
         * MarkMonitor.com does not guarantee its accuracy.
         * By submitting a WHOIS query, you agree that you will use this Data only for lawful purposes and that, under no circumstances will you use this Data to:
         *  (1) allow, enable, or otherwise support the transmission of mass unsolicited, commercial advertising or solicitations via e-mail (spam); or
         *  (2) enable high volume, automated, electronic processes that apply to MarkMonitor.com (or its systems). MarkMonitor.com reserves the right to modify these terms at any time. By submitting this query, you agree to abide by this policy.
         */

        [TestMethod]
        [TestCase(@"facebook.com", @"Facebook, Inc.")]
        public void WhoisClientTest()
        {
            TestContext.Run((string domain, string expectedOrgName) =>
            {
                var response = WhoisClient.Query(domain);
                response.OrganizationName.Is(expectedOrgName);
                response.AddressRange.IsNull();
            });
        }

        [TestMethod]
        [TestCase(@"google.com", @"Google Inc.")]
        public async Task WhoisClientAsyncTest()
        {
            await TestContext.RunAsync(async (string domain, string expectedOrgName) =>
            {
                var response = await WhoisClient.QueryAsync(domain).ConfigureAwait(false);
                response.OrganizationName.Is(expectedOrgName);
                response.AddressRange.IsNull();
            });
        }
    }
}