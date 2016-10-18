using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Whois.NET;

namespace WhoisClient_NET.Test
{
    [TestClass]
    public class WhoisResponseTest
    {
        private string ResponseJP = 
            "Network Information: [ネットワーク情報]\r\n" +
            "a. [IPネットワークアドレス]     192.41.192.0/24\r\n" +
            "b. [ネットワーク名]             JPNICNET\r\n" +
            "f. [組織名]                     社団法人 日本ネットワークインフォメーションセンター\r\n" +
            "g. [Organization]               Japan Network Information Center\r\n" +
            "m. [管理者連絡窓口]             HH11825JP\r\n" +
            "n. [技術連絡担当者]             MO5920JP\r\n";

        [TestMethod]
        public void OrganizationNameTest_JP()
        {
            new WhoisResponse(null, ResponseJP)
                .OrganizationName.Is("社団法人 日本ネットワークインフォメーションセンター");
        }

        [TestMethod]
        public void AddressRangeTest_JP()
        {
            var r = new WhoisResponse(null, ResponseJP);
            r.AddressRange.Begin.ToString().Is("192.41.192.0");
            r.AddressRange.End.ToString().Is("192.41.192.255");
        }

        private string ResponseEN1 = 
            "% [whois.apnic.net node-2]\r\n" +
            "% Whois data copyright terms    http://www.apnic.net/db/dbcopyright.html\r\n" +
            "\r\n" +
            "inetnum:        192.41.192.0 - 192.41.192.255\r\n" +
            "netname:        JPNICNET\r\n" +
            "descr:          Japan Network Information Center\r\n" +
            "country:        JP\r\n";

        [TestMethod]
        public void OrganizationNameTest_EN1()
        {
            new WhoisResponse(null, ResponseEN1)
                .OrganizationName.Is("Japan Network Information Center");
        }

        [TestMethod]
        public void AddressRangeTest_EN1()
        {
            var r = new WhoisResponse(null, ResponseEN1);
            r.AddressRange.Begin.ToString().Is("192.41.192.0");
            r.AddressRange.End.ToString().Is("192.41.192.255");
        }

        private string ResponseEN2 =
            "#\r\n" +
            "# Query terms are ambiguous.  The query is assumed to be:\r\n" +
            "#     \"n 192.41.192.40\"\r\n" +
            "#\r\n" +
            "# Use \"?\" to get help.\r\n" +
            "#\r\n" +
            "\r\n" +
            "#\r\n" +
            "# The following results may also be obtained via:\r\n" +
            "# http://whois.arin.net/rest/nets;q=192.41.192.40?showDetails=true&showARIN=false&ext=netref2\r\n" +
            "#\r\n" +
            "\r\n" +
            "NetRange:       192.41.178.0 - 192.41.197.255\r\n" +
            "CIDR:           192.41.192.0/22, 192.41.184.0/21, 192.41.178.0/23, 192.41.196.0/23, 192.41.180.0/22\r\n" +
            "OriginAS:       \r\n" +
            "NetName:        APNIC-ERX-192-41-178-0\r\n" +
            "NetHandle:      NET-192-41-178-0-1\r\n" +
            "Parent:         NET-192-0-0-0-0\r\n" +
            "NetType:        Early Registrations, Transferred to APNIC\r\n" +
            "Comment:        This IP address range is not registered in the ARIN database.\r\n" +
            "Comment:        This range was transferred to the APNIC Whois Database as\r\n" +
            "Comment:        part of the ERX (Early Registration Transfer) project.\r\n" +
            "Comment:        For details, refer to the APNIC Whois Database via\r\n" +
            "Comment:        WHOIS.APNIC.NET or http://wq.apnic.net/apnic-bin/whois.pl\r\n" +
            "Comment:        \r\n" +
            "Comment:        ** IMPORTANT NOTE: APNIC is the Regional Internet Registry\r\n" +
            "Comment:        for the Asia Pacific region.  APNIC does not operate networks\r\n" +
            "Comment:        using this IP address range and is not able to investigate\r\n" +
            "Comment:        spam or abuse reports relating to these addresses.  For more\r\n" +
            "Comment:        help, refer to http://www.apnic.net/apnic-info/whois_search2/abuse-and-spamming\r\n" +
            "RegDate:        2005-01-31\r\n" +
            "Updated:        2009-10-08\r\n" +
            "Ref:            http://whois.arin.net/rest/net/NET-192-41-178-0-1\r\n" +
            "\r\n" +
            "OrgName:        Asia Pacific Network Information Centre\r\n" +
            "OrgId:          APNIC\r\n" +
            "Address:        PO Box 3646\r\n" +
            "City:           South Brisbane\r\n" +
            "StateProv:      QLD\r\n" +
            "PostalCode:     4101\r\n" +
            "Country:        AU\r\n" +
            "RegDate:        \r\n" +
            "Updated:        2012-01-24\r\n" +
            "Ref:            http://whois.arin.net/rest/org/APNIC\r\n" +
            "\r\n" +
            "ReferralServer: whois://whois.apnic.net\r\n" +
            "\r\n" +
            "OrgTechHandle: AWC12-ARIN\r\n" +
            "OrgTechName:   APNIC Whois Contact\r\n" +
            "OrgTechPhone:  +61 7 3858 3188 \r\n" +
            "OrgTechEmail:  search-apnic-not-arin@apnic.net\r\n" +
            "OrgTechRef:    http://whois.arin.net/rest/poc/A\r\n";

        [TestMethod]
        public void OrganizationNameTest_EN2()
        {
            new WhoisResponse(null, ResponseEN2)
                .OrganizationName.Is("Asia Pacific Network Information Centre");
        }

        [TestMethod]
        public void RespondedServersTest()
        {
            WhoisResponse WR = WhoisClient.Query("150.126.0.0");
            Assert.AreEqual(3, WR.RespondedServers.Length);
        }
    }
}
