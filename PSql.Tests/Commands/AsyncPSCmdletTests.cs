// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Management.Automation.Host;
using PSql.Internal;

namespace PSql.Commands;

using static ScriptExecutor;
using static TestAsyncPSCmdletCommand;
using static TestAsyncPSCmdletCommand.TestMoment;

[TestFixture]
public class AsyncPSCmdletTests
{
    [Test]
    [TestCase(BeforeProcessing,      typeof(ImmediateDispatcher ))]
    [TestCase(DuringProcessing,      typeof(MainThreadDispatcher))]
    [TestCase(DuringProcessingAsync, typeof(MainThreadDispatcher))]
    [TestCase(AfterProcessing,       typeof(ImmediateDispatcher ))]
    public void Dispatcher_Get(TestMoment moment, Type expectedType)
    {
        var (output, exception) = Execute(
            $"Test-AsyncPSCmdlet -Case Dispatcher -Moment {moment}"
        );

        exception.ShouldBeNull();

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBeOfType(expectedType);
    }

    [Test]
    public void CancellationToken_Get()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case CancellationToken"
        );

        exception.ShouldBeNull();

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBeOfType<CancellationToken>()
            .ShouldNotBe(CancellationToken.None);
    }

    [Test]
    [TestCase(BeforeProcessing)]
    [TestCase(AfterProcessing)]
    public void Run_Invalid(TestMoment moment)
    {
        var (output, exception) = Execute(
            $"Test-AsyncPSCmdlet -Case Run -Moment {moment}"
        );

        exception.ShouldBeOfType<InvalidOperationException>()
            .Message.ShouldBe("This method requires prior invocation of BeginProcessing.");
    }

    [Test]
    [TestCase(DuringProcessing)]
    //[TestCase(DuringProcessingAsync)] // TODO: Should this be allowed?
    public void Run_Ok(TestMoment moment)
    {
        var (output, exception) = Execute(
            $"Test-AsyncPSCmdlet -Case Run -Moment {moment}"
        );

        exception.ShouldBeNull();

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBe("Run invoked code that emitted this output object.");
    }

    [Test]
    public void InvokePendingMainThreadActions()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case InvokePendingMainThreadActions -Moment DuringProcessing"
        );

        exception.ShouldBeNull();

        output.Count.ShouldBe(2);

        output[0].ShouldNotBeNull()
            .BaseObject.ShouldBe("Run invoked code that emitted this output object.");

        output[1].ShouldNotBeNull()
            .BaseObject.ShouldBe("Invoked InvokePendingMainThreadActions.");
    }

    [Test]
    public void WaitForAsyncActions()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case WaitForAsyncActions -Moment DuringProcessing"
        );

        exception.ShouldBeNull();

        output.Count.ShouldBe(3);

        output[0].ShouldNotBeNull()
            .BaseObject.ShouldBe("Before WaitForAsyncActions.");

        output[1].ShouldNotBeNull()
            .BaseObject.ShouldBe("Invoked WaitForAsyncActions.");

        output[2].ShouldNotBeNull()
            .BaseObject.ShouldBe("After WaitForAsyncActions.");
    }

    [Test]
    public void WriteObject0()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case WriteObject0"
        );

        exception.ShouldBeNull();

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBe("This is an output object.");
    }

    [Test]
    public void WriteObject1()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case WriteObject1"
        );

        exception.ShouldBeNull();

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBe("This is an output object.");
    }

    [Test]
    public void WriteHost()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case WriteHost"
        );

        exception.ShouldBeNull();

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBe(new PSInformation("This is a host message."));
    }

    [Test]
    public void WriteCommandDetail()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case WriteCommandDetail"
        );

        exception.ShouldBeNull();

        output.ShouldBeEmpty();
    }

    [Test]
    public void WriteDebug()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case WriteDebug -Debug"
        );

        exception.ShouldBeNull();

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBe(new PSDebug("This is a debug message."));
    }

    [Test]
    public void WriteVerbose()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case WriteVerbose -Verbose"
        );

        exception.ShouldBeNull();

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBe(new PSVerbose("This is a verbose message."));
    }

    [Test]
    public void WriteInformation0()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case WriteInformation0 -InformationAction Continue"
        );

        exception.ShouldBeNull();

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBe(new PSInformation("This is an information message."));
    }

    [Test]
    public void WriteInformation1()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case WriteInformation1 -InformationAction Continue"
        );

        exception.ShouldBeNull();

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBe(new PSInformation("This is an information message."));
    }

    [Test]
    public void WriteWarning()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case WriteWarning"
        );

        exception.ShouldBeNull();

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBe(new PSWarning("This is a warning message."));
    }

    [Test]
    public void WriteError()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case WriteError -ErrorAction Continue"
        );

        // Because ScriptExecutor.Execute() captures output errors as exceptions
        exception.ShouldNotBeNull()
            .Message.ShouldBe("This is an error message.");

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBe(new PSError("This is an error message."));
    }

    [Test]
    public void WriteProgress()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case WriteProgress"
        );

        exception.ShouldBeNull();

        output.Count.ShouldBe(5);

        var progresses = output.Select(o => o?.BaseObject).OfType<PSProgress>().ToList();

        progresses.Count.ShouldBe(5);
        progresses[0].Message.ShouldBe("parent = -1 id = 0 act = Test AsyncPSCmdlet stat = Testing cur =  pct = 0 sec = -1 type = Processing");
        progresses[1].Message.ShouldBe("parent = -1 id = 0 act = Test AsyncPSCmdlet stat = Testing cur =  pct = 25 sec = -1 type = Processing");
        progresses[2].Message.ShouldBe("parent = -1 id = 0 act = Test AsyncPSCmdlet stat = Testing cur =  pct = 50 sec = -1 type = Processing");
        progresses[3].Message.ShouldBe("parent = -1 id = 0 act = Test AsyncPSCmdlet stat = Testing cur =  pct = 75 sec = -1 type = Processing");
        progresses[4].Message.ShouldBe("parent = -1 id = 0 act = Test AsyncPSCmdlet stat = Testing cur =  pct = 100 sec = -1 type = Processing");
    }

    [Test]
    public void ShouldContinue0()
    {
        const string ExpectedMessage
            = "A command that prompts the user failed because the host program "
            + "or the command type does not support user interaction. The host "
            + "was attempting to request confirmation with the following message: "
            + "Continue?";

        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case ShouldContinue0"
        );

        exception.ShouldBeOfType<HostException>()
            .Message.ShouldBe(ExpectedMessage);

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBeOfType<PSError>()
            .Message.ShouldBe(ExpectedMessage);
    }

    [Test]
    public void ShouldContinue1()
    {
        // This test passes yesToAll = true
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case ShouldContinue1"
        );

        exception.ShouldBeNull();

        output.ShouldBeEmpty();
    }

    [Test]
    public void ShouldContinue2()
    {
        // This test passes noToAll = true
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case ShouldContinue2"
        );

        exception.ShouldBeNull();

        output.ShouldBeEmpty();
    }

    [Test]
    public void ShouldProcess0()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case ShouldProcess0 -Verbose"
        );

        exception.ShouldBeNull();

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBe(new PSVerbose(@"Performing the operation ""Test-AsyncPSCmdlet"" on target ""AsyncPSCmdlet""."));
    }

    [Test]
    public void ShouldProcess1()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case ShouldProcess1 -Verbose"
        );

        exception.ShouldBeNull();

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBe(new PSVerbose(@"Performing the operation ""Test"" on target ""AsyncPSCmdlet""."));
    }

    [Test]
    public void ShouldProcess2()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case ShouldProcess2 -Verbose"
        );

        exception.ShouldBeNull();

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBe(new PSVerbose("Testing AsyncPSCmdlet."));
    }

    [Test]
    public void ShouldProcess3()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case ShouldProcess3 -Verbose"
        );

        exception.ShouldBeNull();

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBe(new PSVerbose("Testing AsyncPSCmdlet."));
    }

    [Test]
    public void StopProcessing()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case StopProcessing"
        );

        exception.ShouldBeNull();

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBe(new PSInformation("Canceling..."));
    }

    [Test]
    public void ThrowTerminatingError()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case ThrowTerminatingError"
        );

        // Because ScriptExecutor.Execute() captures output errors as exceptions
        exception.ShouldNotBeNull()
            .Message.ShouldBe("This is an error message.");

        output.ShouldHaveSingleItem().ShouldNotBeNull()
            .BaseObject.ShouldBe(new PSError("This is an error message."));
    }

    [Test]
    public void Dispose_Multiple()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case MultipleDispose -Moment AfterProcessing"
        );

        exception.ShouldBeNull();

        output.ShouldBeEmpty();
    }

    [Test]
    public void Dispose_Unmanaged()
    {
        var (output, exception) = Execute(
            "Test-AsyncPSCmdlet -Case UnmanagedDispose -Moment AfterProcessing"
        );

        exception.ShouldBeNull();

        output.ShouldBeEmpty();
    }
}
