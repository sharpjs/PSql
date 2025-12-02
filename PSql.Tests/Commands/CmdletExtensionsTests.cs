// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql.Commands;

using static ScriptExecutor;

[TestFixture]
public class CmdletExtensionsTests
{
    [Test]
    public void WriteHost_NullCmdlet()
    {
        Should.Throw<ArgumentNullException>(() =>
        {
            default(PSCmdlet)!.WriteHost("");
        });
    }

    [Test]
    public void WriteHost_Null()
    {
        var (output, exception) = Execute(
            "Test-CmdletExtensions -Case WriteHost -Message $null"
        );

        exception.ShouldBeNull();

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBe(new PSInformation(""));
    }

    [Test]
    public void WriteHost_NotNull()
    {
        var (output, exception) = Execute(
            "Test-CmdletExtensions -Case WriteHost -Message Foo"
        );

        exception.ShouldBeNull();

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBe(new PSInformation("Foo"));
    }
}
