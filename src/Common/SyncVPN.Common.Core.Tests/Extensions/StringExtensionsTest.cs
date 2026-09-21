/*
 * Copyright (c) 2024 Proton AG
 *
 * This file is part of SyncVPN.
 *
 * SyncVPN is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * SyncVPN is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with SyncVPN.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SyncVPN.Common.Core.Extensions;

namespace SyncVPN.Common.Core.Tests.Extensions;

[TestClass]
public class StringExtensionsTest
{
    [TestMethod]
    [DataRow("", false)]                                                // Invalid (empty string)
    [DataRow(" ", false)]                                               // Invalid (whitespace only)
    [DataRow(null, false)]                                              // Invalid (null string)
    [DataRow("syncvpn.com", true)]                                        // Valid URL without protocol
    [DataRow("www.syncvpn.com", true)]                                    // Valid URL with "www"
    [DataRow("http://syncvpn.com", true)]                                 // Valid URL with "http" protocol
    [DataRow("https://syncvpn.com", true)]                                // Valid URL with "https" protocol
    [DataRow("https://www.syncvpn.com", true)]                        // Valid URL with "www" and subdomain
    [DataRow("https://www.syncvpn.com/features", true)]               // Valid URL with path
    [DataRow("https://www.syncvpn.com/features?q=test", true)]        // Valid URL with query string
    [DataRow("ftp://ftp.syncvpn.com", true)]                              // Valid FTP URL
    [DataRow("customprotocol://syncvpnapp", true)]                       // Valid custom protocol
    [DataRow("https://syncvpn.com:8080", true)]                       // Valid URL with port
    [DataRow("https://syncvpn.com/#features", true)]                  // Valid URL with fragment
    [DataRow("https://syncvpn.com/path/to/resource", true)]           // Valid URL with long path
    [DataRow("https://syncvpn.com/search?q=abc+def&l=en-US", true)]   // Valid URL with parameters
    [DataRow("https://blog.syncvpn.com", true)]                       // Valid URL with subdomain
    [DataRow("https://www.syncvpn.vpn", true)]                          // Valid URL with uncommon TLD
    [DataRow("http://255.255.255.255", true)]                           // Valid URL with IPv4
    [DataRow("http://[2001:db8::1]", true)]                             // Valid URL with IPv6
    [DataRow("http://[2001:db8::1]:8080", true)]                        // Valid URL with IPv6 and port
    [DataRow("http://[::1]", true)]                                     // Valid URL with loopback IPv6
    [DataRow("//syncvpn.com", false)]                                     // Invalid URL (missing protocol)
    [DataRow("https:// syncvpn.com", false)]                              // Invalid URL (space in domain)
    [DataRow("https:/syncvpn.com", false)]                                // Invalid URL (malformed protocol)
    [DataRow("http://.me", false)]                                      // Invalid URL (missing domain name)
    [DataRow("http://syncvpn..com", false)]                             // Invalid URL (double dots in domain)
    [DataRow("https://syncvpn.com:abcd", false)]                      // Invalid (non-numeric port)
    [DataRow("http:/syncvpn.com", false)]                                 // Invalid URL (single /)
    public void TestUrlValidation(string url, bool expectedResult)
    {
        bool result = url.IsValidUrl();
        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    [DataRow("", false)]
    [DataRow("1.2.3", false)]
    [DataRow("1.2.3.4.5", false)]
    [DataRow("1.2.3.", false)]
    [DataRow(".1.2.3", false)]
    [DataRow("1..2.3", false)]
    [DataRow("a.b.c.d", false)]
    [DataRow("syncvpn.com", false)]
    [DataRow("0.0.0.0", true)]
    [DataRow("0.0.512", false)]
    [DataRow("255.255.255.255", true)]
    [DataRow("256.0.0.0", false)]
    [DataRow("1.2.3.4", true)]
    [DataRow("1.2.3.0/24", false)]
    [DataRow("1.2.3.0/0", false)]
    [DataRow("1.2.3.0/32", false)]
    [DataRow("1.2.3.0/35", false)]
    [DataRow("1.2.3.0/00024", false)]
    [DataRow("1.2.3.0/", false)]
    [DataRow("1.2.3.0/abc", false)]
    [DataRow("0.0.512/32", false)]
    [DataRow("0.0.512/16", false)]
    [DataRow("1", false)]
    [DataRow("1.2.3.0/24/16", false)]
    [DataRow("1.2.3.0/24.16", false)]
    public void TestIpAddressFormatValidation(string ipAddress, bool expectedResult)
    {
        bool result = ipAddress.IsValidIpAddressFormat();
        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    [DataRow("", false)]
    [DataRow("1.2.3", false)]
    [DataRow("1.2.3.4.5", false)]
    [DataRow("1.2.3.", false)]
    [DataRow(".1.2.3", false)]
    [DataRow("1..2.3", false)]
    [DataRow("a.b.c.d", false)]
    [DataRow("syncvpn.com", false)]
    [DataRow("0.0.0.0", true)]
    [DataRow("0.0.512", false)]
    [DataRow("255.255.255.255", true)]
    [DataRow("256.0.0.0", false)]
    [DataRow("1.2.3.4", true)]
    [DataRow("1.2.3.0/24", false)]
    [DataRow("1.2.3.0/0", false)]
    [DataRow("1.2.3.0/32", true)]
    [DataRow("1.2.3.0/35", false)]
    [DataRow("1.2.3.0/00024", false)]
    [DataRow("1.2.3.0/", false)]
    [DataRow("1.2.3.0/abc", false)]
    [DataRow("0.0.512/32", false)]
    [DataRow("0.0.512/16", false)]
    [DataRow("1", false)]
    [DataRow("1.2.3.0/24/16", false)]
    [DataRow("1.2.3.0/24.16", false)]
    public void TestIpAddressValidation(string ipAddress, bool expectedResult)
    {
        bool result = ipAddress.IsValidIpAddress();
        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    [DataRow("", false)]
    [DataRow("1.2.3", false)]
    [DataRow("1.2.3.4.5", false)]
    [DataRow("1.2.3.", false)]
    [DataRow(".1.2.3", false)]
    [DataRow("1..2.3", false)]
    [DataRow("a.b.c.d", false)]
    [DataRow("syncvpn.com", false)]
    [DataRow("0.0.0.0", true)]
    [DataRow("0.0.512", false)]
    [DataRow("255.255.255.255", true)]
    [DataRow("256.0.0.0", false)]
    [DataRow("1.2.3.4", true)]
    [DataRow("1.2.3.0/24", true)]
    [DataRow("1.2.3.0/0", true)]
    [DataRow("1.2.3.0/32", true)]
    [DataRow("1.2.3.0/35", false)]
    [DataRow("1.2.3.0/00024", true)]
    [DataRow("1.2.3.0/", false)]
    [DataRow("1.2.3.0/abc", false)]
    [DataRow("0.0.512/32", false)]
    [DataRow("0.0.512/16", false)]
    [DataRow("1", false)]
    [DataRow("1.2.3.0/24/16", false)]
    [DataRow("1.2.3.0/24.16", false)]
    public void TestIpAddressOrRangeValidation(string ipAddress, bool expectedResult)
    {
        bool result = ipAddress.IsValidIpAddressOrRange();
        Assert.AreEqual(expectedResult, result);
    }

    [TestMethod]
    [DataRow(null, -1)]
    [DataRow("", -1)]
    [DataRow(" ", -1)]
    [DataRow("a", -1)]
    [DataRow("BC", -1)]
    [DataRow("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ`~!@#$%^&*()-_=+[{]}\\\\|;:'\",<.>/? \r\n", -1)]
    [DataRow("1d", 0)]
    [DataRow("12345678901234567890", 0)]
    [DataRow("e2fgh", 1)]
    [DataRow("e2345fgh", 1)]
    [DataRow("e2fgh345", 1)]
    [DataRow("ij3klmn456", 2)]
    [DataRow("opq789rstu0", 3)]
    public void TestIndexOfFirstDigit(string text, int expectedResult)
    {
        int result = text.IndexOfFirstDigit();
        Assert.AreEqual(expectedResult, result);
    }
}