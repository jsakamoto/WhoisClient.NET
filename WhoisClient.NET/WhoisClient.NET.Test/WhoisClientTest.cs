using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Whois.NET;

namespace WhoisClient_NET.Test
{
    [TestClass]
    public class WhoisClientTest
    {
        [TestMethod]
        public void WhoisClientIPTest()
        {
            WhoisResponse response = WhoisClient.Query("4.4.4.4");
            Assert.AreEqual("Level 3 Communications, Inc. LVLT-STATIC-4-4-16 (NET-4-4-0-0-1)", response.OrganizationName);
            Assert.AreEqual("4.4.0.0-4.4.255.255", response.AddressRange.ToString());
        }

        [TestMethod]
        public void WhoisClientIP2Test()
        {
            WhoisResponse response = WhoisClient.Query("65.100.170.169");
            Assert.AreEqual("Qwest Communications Company, LLC QWEST-INET-115 (NET-65-100-0-0-1)", response.OrganizationName);
            Assert.AreEqual("65.100.0.0-65.103.255.255", response.AddressRange.ToString());
        }

        [TestMethod]
        public void WhoisClientIP3Test()
        {
            WhoisResponse response = WhoisClient.Query("108.234.177.20");
            Assert.AreEqual("AT&T Internet Services", response.OrganizationName);
            Assert.AreEqual("108.192.0.0-108.255.255.255", response.AddressRange.ToString());
        }

        [TestMethod]
        public void WhoisClientIP4Test()
        {
            WhoisResponse response = WhoisClient.Query("190.190.132.64");
            Assert.AreEqual("Prima S.A.", response.OrganizationName);
            Assert.AreEqual("190.0.0.0-190.1.255.255", response.AddressRange.ToString());
        }

        [TestMethod]
        public void WhoisClientIP5Test()
        {
            WhoisResponse response = WhoisClient.Query("31.116.94.96");
            Assert.AreEqual("EE route", response.OrganizationName);
            Assert.AreEqual("31.64.0.0-31.127.255.255", response.AddressRange.ToString());
        }

        [TestMethod]
        public void WhoisClientDomainTest()
        {
            WhoisResponse response = WhoisClient.Query("facebook.com");
            Assert.AreEqual("Facebook, Inc.", response.OrganizationName);
            Assert.IsNull(response.AddressRange);
        }

    }
}
