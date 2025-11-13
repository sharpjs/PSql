// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

using static SqlClientVersion;
using static AzureAuthenticationMode;

[TestFixture]
public class SqlClientVersionExtensionsTests
{
    [Test]
    //                                     2.1 1.1
    //        MODE                    5 4 3 : 2 : 1 L
    [TestCase(Default,             0b_1_1_1_1_1_1_1_0)]
    [TestCase(SqlPassword,         0b_1_1_1_1_1_1_1_0)]
    [TestCase(AadPassword,         0b_1_1_1_1_1_1_1_0)]
    [TestCase(AadIntegrated,       0b_1_1_1_1_1_1_1_0)]
    [TestCase(AadInteractive,      0b_1_1_1_1_1_1_1_0)]
    [TestCase(AadServicePrincipal, 0b_1_1_1_1_1_0_0_0)]
    [TestCase(AadDeviceCodeFlow,   0b_1_1_1_1_0_0_0_0)]
    [TestCase(AadManagedIdentity,  0b_1_1_1_1_0_0_0_0)]
    [TestCase(AadDefault,          0b_1_1_1_0_0_0_0_0)]
    [TestCase(-1,                  0b_0_0_0_0_0_0_0_0)]
    public void SupportsAuthenticationMode(AzureAuthenticationMode mode, long bitmap)                                                           
    {
        for (var version = Legacy; version < Mds5; version++)
        {
            var expected = (bitmap & 1) != 0;

            version.SupportsAuthenticationMode(mode).ShouldBe(expected);

            bitmap >>= 1;
        }
    }

    [Test]
    [TestCase(Mds3, false)]
    [TestCase(Mds4, true )]
    public void GetDefaultEncrypt(SqlClientVersion version, bool value)
    {
        version.GetDefaultEncrypt().ShouldBe(value);
    }

    [Test]
    [TestCase(Mds1_1, "ApplicationIntent" )]
    [TestCase(Mds2,   "Application Intent")]
    public void GetApplicationIntentKey(SqlClientVersion version, string value)
    {
        version.GetApplicationIntentKey().ShouldBe(value);
    }

    [Test]
    [TestCase(Mds1_1, "MultipleActiveResultSets"   )]
    [TestCase(Mds2,   "Multiple Active Result Sets")]
    public void GetMultipleActiveResultSetsKey(SqlClientVersion version, string value)
    {
        version.GetMultipleActiveResultSetsKey().ShouldBe(value);
    }

    [Test]
    [TestCase(Mds1_1, "TrustServerCertificate"  )]
    [TestCase(Mds2,   "Trust Server Certificate")]
    public void GetTrustServerCertificateKey(SqlClientVersion version, string value)
    {
        version.GetTrustServerCertificateKey().ShouldBe(value);
    }
}
