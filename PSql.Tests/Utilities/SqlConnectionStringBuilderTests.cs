// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql;

[TestFixture]
public class SqlConnectionStringBuilderTests
{
    // This test class only backfills coverage gaps in other tests.

    [Test]
    [TestCase(null)]
    [TestCase(""  )]
    public void Append_NullOrEmpty(string? value)
    {
        var builder = new SqlConnectionStringBuilder(SqlClientVersion.Latest);

        builder.AppendClientName(value);

        builder.ToString().ShouldBe("Workstation ID=");
    }

    [Test]
    public void Append_ContainsNul()
    {
        Should.Throw<FormatException>(() =>
        {
            new SqlConnectionStringBuilder(SqlClientVersion.Latest)
            .AppendClientName("A\0B");
        });
    }
}
