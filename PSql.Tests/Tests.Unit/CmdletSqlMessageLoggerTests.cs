// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Management.Automation.Internal;
using System.Reflection;

namespace PSql.Tests.Unit;

[TestFixture]
public class CmdletSqlMessageLoggerTests : TestHarnessBase
{
    private readonly CmdletSqlMessageLogger _logger;
    private readonly TestCmdlet             _cmdlet;
    private readonly Mock<ICommandRuntime2> _runtime;

    public CmdletSqlMessageLoggerTests()
    {
        _runtime = Mocks.Create<ICommandRuntime2>();
        _cmdlet  = new TestCmdlet() { CommandRuntime = _runtime.Object };
        _logger  = new CmdletSqlMessageLogger(_cmdlet);
    }

    [Test]
    public void Construct_NullCmdlet()
    {
        Invoking(() => new CmdletSqlMessageLogger(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void LogInformation()
    {
        _runtime
            .Setup(r => r.WriteInformation(It.Is<InformationRecord>(r
                => ((HostInformationMessage) r.MessageData).Message == "a"
            )))
            .Verifiable();

        _logger.LogInformation("a");
    }

    [Test]
    public void LogError()
    {
        _runtime
            .Setup(r => r.WriteWarning("a"))
            .Verifiable();

        _logger.LogError("a");
    }
}

internal class TestCmdlet : Cmdlet
{
    public TestCmdlet()
    {
        var info = new CmdletInfo("Test-Cmdlet", typeof(TestCmdlet));

        typeof(InternalCommand)
            .GetProperty("CommandInfo", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(this, info);
    }
}
