using System.Net;
using Newtonsoft.Json;
using NUnit.Framework;
using Whois.NET;

namespace WhoisClient_NET.Test;

[Parallelizable(ParallelScope.All)]
public class WhoisResponseTest
{
    private readonly string ResponseJP =
        "Network Information: [ネットワーク情報]\r\n" +
        "a. [IPネットワークアドレス]     192.41.192.0/24\r\n" +
        "b. [ネットワーク名]             JPNICNET\r\n" +
        "f. [組織名]                     社団法人 日本ネットワークインフォメーションセンター\r\n" +
        "g. [Organization]               Japan Network Information Center\r\n" +
        "m. [管理者連絡窓口]             HH11825JP\r\n" +
        "n. [技術連絡担当者]             MO5920JP\r\n";

    private readonly string ResponseJPWithoutKeyLetters =
        "Network Information: [ネットワーク情報]\r\n" +
        "[IPネットワークアドレス]        27.80.0.0/12\r\n" +
        "[ネットワーク名]\r\n" +
        "[組織名]                        KDDI株式会社\r\n" +
        "[Organization]                  KDDI CORPORATION\r\n" +
        "[管理者連絡窓口]                JP00000127\r\n" +
        "[技術連絡担当者]                JP00000181\r\n" +
        "[Abuse]                         abuse@dion.ne.jp\r\n" +
        "[割振年月日]                    2010/04/28\r\n" +
        "[最終更新]                      2010/04/28 10:50:55(JST)\r\n";

    private readonly string ResponseJPWithIPv6 =
        "Network Information: [ネットワーク情報]\r\n" +
        "[IPネットワークアドレス]        2001:0dc2::/32\r\n" +
        "[ネットワーク名]                JPNIC-NET-JP-20030529\r\n" +
        "[組織名]                        一般社団法人日本ネットワークインフォメーションセンター\r\n" +
        "[Organization]                  Japan Network Information Center\r\n" +
        "[管理者連絡窓口]                SS54384JP\r\n" +
        "[技術連絡担当者]                YK11438JP\r\n" +
        "[技術連絡担当者]                EK6175JP\r\n" +
        "[技術連絡担当者]                TK74577JP\r\n" +
        "[技術連絡担当者]                NH27225JP\r\n" +
        "[技術連絡担当者]                KG13714JP\r\n" +
        "[Abuse]\r\n" +
        "[ネームサーバ]                  ns3.nic.ad.jp\r\n" +
        "[ネームサーバ]                  ns5.nic.ad.jp\r\n" +
        "[割当年月日]                    2003/05/29\r\n" +
        "[返却年月日]\r\n" +
        "[最終更新]                      2023/06/19 16:44:03(JST)\r\n";

    [Test]
    public void OrganizationNameTest_JP()
    {
        new WhoisResponse(null, this.ResponseJP)
            .OrganizationName.Is("社団法人 日本ネットワークインフォメーションセンター");
    }

    [Test]
    public void OrganizationNameTest_JP_WithoutKeyLetters()
    {
        new WhoisResponse(null, this.ResponseJPWithoutKeyLetters)
            .OrganizationName.Is("KDDI株式会社");
    }

    [Test]
    public void OrganizationNameTest_JP_WithIPv6()
    {
        new WhoisResponse(null, this.ResponseJPWithIPv6)
            .OrganizationName.Is("一般社団法人日本ネットワークインフォメーションセンター");
    }

    [Test]
    public void AddressRangeTest_JP()
    {
        var r = new WhoisResponse(null, this.ResponseJP);
        r.AddressRange.IsNotNull();
        r.AddressRange.Begin.ToString().Is("192.41.192.0");
        r.AddressRange.End.ToString().Is("192.41.192.255");
    }

    [Test]
    public void AddressRangeTest_JP_WithoutKeyLetters()
    {
        var r = new WhoisResponse(null, this.ResponseJPWithoutKeyLetters);
        r.AddressRange.IsNotNull();
        r.AddressRange.Begin.ToString().Is("27.80.0.0");
        r.AddressRange.End.ToString().Is("27.95.255.255");
    }

    [Test]
    public void AddressRangeTest_JP_WithIPv6()
    {
        var r = new WhoisResponse(null, this.ResponseJPWithIPv6);
        r.AddressRange.IsNotNull();
        r.AddressRange.Begin.ToString().Is("2001:dc2::");
        r.AddressRange.End.ToString().Is("2001:dc2:ffff:ffff:ffff:ffff:ffff:ffff");
    }

    private readonly string ResponseEN1 =
        "% [whois.apnic.net node-2]\r\n" +
        "% Whois data copyright terms    http://www.apnic.net/db/dbcopyright.html\r\n" +
        "\r\n" +
        "inetnum:        192.41.192.0 - 192.41.192.255\r\n" +
        "netname:        JPNICNET\r\n" +
        "descr:          Japan Network Information Center\r\n" +
        "country:        JP\r\n";

    [Test]
    public void OrganizationNameTest_EN1()
    {
        new WhoisResponse(null, this.ResponseEN1)
            .OrganizationName.Is("Japan Network Information Center");
    }

    [Test]
    public void AddressRangeTest_EN1()
    {
        var r = new WhoisResponse(null, this.ResponseEN1);
        r.AddressRange.IsNotNull();
        r.AddressRange.Begin.ToString().Is("192.41.192.0");
        r.AddressRange.End.ToString().Is("192.41.192.255");
    }

    private readonly string ResponseENApnicWithIPv6 =
        "% Whois data copyright terms    http://www.apnic.net/db/dbcopyright.html\r\n" +
        "\r\n" +
        "% Information related to '2001:dc2::/32'\r\n" +
        "\r\n" +
        "% No abuse contact registered for 2001:dc2::/32\r\n" +
        "\r\n" +
        "inet6num:       2001:dc2::/32\r\n" +
        "netname:        JPNIC-NET-JP-20030529\r\n" +
        "descr:          Japan Network Information Center\r\n" +
        "country:        JP\r\n" +
        "admin-c:        JNIC1-AP\r\n" +
        "tech-c:         JNIC1-AP\r\n" +
        "mnt-by:         MAINT-JPNIC\r\n" +
        "remarks:        JPNIC Allocation Block\r\n" +
        "remarks:        Authoritative information regarding assignments and\r\n" +
        "remarks:        allocations made from within this block can also be\r\n" +
        "remarks:        queried at whois.nic.ad.jp. To obtain an English\r\n" +
        "remarks:        output query whois -h whois.nic.ad.jp x.x.x.x/e\r\n" +
        "remarks:        *****************************************************\r\n" +
        "remarks:        For abuse/any contacts regarding this address range,\r\n" +
        "remarks:        please refer to the contacts listed on the inet6num\r\n" +
        "remarks:        object displayed down below, or look up JPNIC Whois.\r\n" +
        "remarks:        ****************************************************\r\n" +
        "status:         ASSIGNED PORTABLE\r\n" +
        "last-modified:  2008-09-04T06:49:18Z\r\n" +
        "source:         APNIC\r\n";

    [Test]
    public void OrganizationNameTest_EN_Apnic_WithIPv6()
    {
        new WhoisResponse(null, this.ResponseENApnicWithIPv6)
            .OrganizationName.Is("Japan Network Information Center");
    }

    [Test]
    public void AddressRangeTest_EN_Apnic_WithIPv6()
    {
        var r = new WhoisResponse(null, this.ResponseENApnicWithIPv6);
        r.AddressRange.IsNotNull();
        r.AddressRange.Begin.ToString().Is("2001:dc2::");
        r.AddressRange.End.ToString().Is("2001:dc2:ffff:ffff:ffff:ffff:ffff:ffff");
    }

    private readonly string ResponseEN2 =
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

    [Test]
    public void OrganizationNameTest_EN2()
    {
        new WhoisResponse(null, this.ResponseEN2)
            .OrganizationName.Is("Asia Pacific Network Information Centre");
    }

    private readonly string ResponseENArinWithIPv6 =
        "NetRange:       2001:500:110:: - 2001:500:110:FFFF:FFFF:FFFF:FFFF:FFFF\r\n" +
        "CIDR:           2001:500:110::/48\r\n" +
        "NetName:        ARIN-CHA-CHA\r\n" +
        "NetHandle:      NET6-2001-500-110-1\r\n" +
        "Parent:         ARIN-001 (NET6-2001-400-0)\r\n" +
        "NetType:        Direct Allocation\r\n" +
        "OriginAS:       AS10745\r\n" +
        "Organization:   ARIN Operations (ARINOPS)\r\n" +
        "RegDate:        2016-05-10\r\n" +
        "Updated:        2021-12-14\r\n" +
        "Ref:            https://rdap.arin.net/registry/ip/2001:500:110::\r\n" +
        "\r\n" +
        "OrgName:        ARIN Operations\r\n" +
        "OrgId:          ARINOPS\r\n" +
        "Address:        PO Box 232290\r\n" +
        "City:           Centreville\r\n" +
        "StateProv:      VA\r\n" +
        "PostalCode:     20120\r\n" +
        "Country:        US\r\n" +
        "RegDate:        2012-09-07\r\n" +
        "Updated:        2023-04-25\r\n" +
        "Ref:            https://rdap.arin.net/registry/entity/ARINOPS\r\n";

    [Test]
    public void OrganizationNameTest_EN_Arin_WithIPv6()
    {
        new WhoisResponse(null, this.ResponseENArinWithIPv6)
            .OrganizationName.Is("ARIN Operations");
    }

    [Test]
    public void AddressRangeTest_EN_Arin_WithIPv6()
    {
        var r = new WhoisResponse(null, this.ResponseENArinWithIPv6);
        r.AddressRange.IsNotNull();
        r.AddressRange.Begin.ToString().Is("2001:500:110::");
        r.AddressRange.End.ToString().Is("2001:500:110:ffff:ffff:ffff:ffff:ffff");
    }

    [Test]
    public void RespondedServersTest()
    {
        var WR = WhoisClient.Query("150.126.0.0", new WhoisQueryOptions());
        WR.RespondedServers.Length.Is(3);
    }

    [Test]
    public void JsonSerializationByJSONNETTest()
    {
        var response = new WhoisResponse(
            new[] { "whois.iana.org", "whois.apnic.net", "whois.afrinic.net" },
            "NetRange: 150.126.0.0 - 150.126.255.255\n" +
            "OrgName:  Santa Cruz Operation Incorporated");
        var json = JsonConvert.SerializeObject(response);
        json.Is("{" +
            @"""RespondedServers"":[""whois.iana.org"",""whois.apnic.net"",""whois.afrinic.net""]," +
            @"""Raw"":""NetRange: 150.126.0.0 - 150.126.255.255\nOrgName:  Santa Cruz Operation Incorporated""," +
            @"""OrganizationName"":""Santa Cruz Operation Incorporated""," +
            @"""AddressRange"":{""Begin"":""150.126.0.0"",""End"":""150.126.255.255""}" +
            "}");
    }

    [Test]
    public void JsonDeserializationByJSONNETTest()
    {
        var json = "{" +
            @"""RespondedServers"":[""whois.iana.org"",""whois.apnic.net"",""whois.afrinic.net""]," +
            @"""Raw"":""NetRange: 150.126.0.0 - 150.126.255.255\nOrgName:  Santa Cruz Operation Incorporated""," +
            @"""OrganizationName"":""Santa Cruz Operation Incorporated""," +
            @"""AddressRange"":{""Begin"":""150.126.0.0"",""End"":""150.126.255.255""}" +
            "}";
        var response = JsonConvert.DeserializeObject<WhoisResponse>(json);
        if (response == null) throw new NullReferenceException();

        response.RespondedServers.Is("whois.iana.org", "whois.apnic.net", "whois.afrinic.net");
        response.OrganizationName.Is("Santa Cruz Operation Incorporated");
        response.AddressRange.Begin.Is(IPAddress.Parse("150.126.0.0"));
        response.AddressRange.End.Is(IPAddress.Parse("150.126.255.255"));
        response.Raw.Is(
            "NetRange: 150.126.0.0 - 150.126.255.255\n" +
            "OrgName:  Santa Cruz Operation Incorporated");
    }
}
