// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql.Commands;

[TestFixture]
public class ExpandSqlCmdDirectivesCommandTests
{
    [Test]
    public void Invoke_Typical()
    {
        var (output, exception) = ScriptExecutor.Execute(
            """
            $Sql = @"
            :setvar VarA ValA
            :setvar VarB "Val B"
            :setvar File include.sql
            -- `$(VarA) -- not replaced in this test
            GO
            :r `$(File)
            "@

            Set-Content include.sql @"
            SELECT ColA = '`$(VarA)', ColB = '`$(VarB)';
            "@

            Expand-SqlCmdDirectives $Sql
            """
        );

        exception.ShouldBeNull();

        output.Select(o => (string?) o?.BaseObject).ShouldBe([
            """
            -- $(VarA) -- not replaced in this test

            """,
            """
            SELECT ColA = 'ValA', ColB = 'Val B';

            """
        ]);
    }

    [Test]
    public void Invoke_Define()
    {
        var (output, exception) = ScriptExecutor.Execute(
            """
            $Sql = @"
            PRINT '`$(VarA)';
            "@

            Expand-SqlCmdDirectives $Sql -Define @{ VarA = "ValA" }
            """
        );

        exception.ShouldBeNull();

        output.Select(o => (string?) o?.BaseObject).ShouldBe([
            """
            PRINT 'ValA';
            """
        ]);
    }

    [Test]
    public void Invoke_ReplaceVariablesInComments()
    {
        var (output, exception) = ScriptExecutor.Execute(
            """
            $Sql = @"
            :setvar Foo Bar
            -- `$(Foo)
            "@

            Expand-SqlCmdDirectives $Sql -ReplaceVariablesInComments
            """
        );

        exception.ShouldBeNull();

        output.Select(o => (string?) o?.BaseObject).ShouldBe([
            """
            -- Bar
            """
        ]);
    }

    [Test]
    public void Invoke_NoContent()
    {
        var (output, exception) = ScriptExecutor.Execute(
            """
            $Sql = @"
            :setvar _ "There is no actual content in this script."
            "@

            Expand-SqlCmdDirectives $Sql
            """
        );

        exception.ShouldBeNull();

        output.ShouldBeEmpty();
    }

    [Test]
    public void ReplaceVariablesInComments_Get()
    {
        new TestCommand().ReplaceVariablesInComments.IsPresent.ShouldBeFalse();
    }

    [Test]
    public void ProcessRecord_NullSql()
    {
        // PowerShell parameter validation should prevent this case, but the
        // code should still handle it gracefully
        new TestCommand { Sql = null }.ProcessRecord();
    }

    [Test]
    public void ProcessRecord_EmptySql()
    {
        // PowerShell parameter validation should prevent this case, but the
        // code should still handle it gracefully
        new TestCommand { Sql = [] }.ProcessRecord();
    }

    [Test]
    public void ProcessRecord_EmptySqlItem()
    {
        // PowerShell parameter validation should prevent this case, but the
        // code should still handle it gracefully
        new TestCommand { Sql = [""] }.ProcessRecord();
    }

    private class TestCommand : ExpandSqlCmdDirectivesCommand
    {
        public new void ProcessRecord() => base.ProcessRecord();
    }
}
