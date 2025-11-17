// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using PSql.Commands;

namespace PSql;

using static E.SqlMessageConstants;

[TestFixture]
public class CmdletSqlMessageLoggerTests : TestHarnessBase
{
    private readonly CmdletSqlMessageLogger _logger;
    private readonly Mock<ICmdlet>          _cmdlet;

    public CmdletSqlMessageLoggerTests()
    {
        _cmdlet = Mocks.Create<ICmdlet>();
        _logger = new(_cmdlet.Object);
    }

    [Test]
    public void Construct_NullCmdlet()
    {
        Should.Throw<ArgumentNullException>(() =>
        {
            _ = new CmdletSqlMessageLogger(null!);
        });
    }

    [Test]
    public void Log_Information()
    {
        _cmdlet
            .Setup(c => c.WriteHost("a", true, null, null))
            .Verifiable();

        _logger.Log("foo", line: 42, number: 1337, MaxInformationalSeverity, message: "a");
    }

    [Test]
    public void Log_Error()
    {
        (11).ShouldBeGreaterThan(MaxInformationalSeverity);

        _cmdlet
            .Setup(c => c.WriteWarning("foo:42: E1337:11: a"))
            .Verifiable();

        _logger.Log("foo", line: 42, number: 1337, severity: 11, "a");
    }
}
