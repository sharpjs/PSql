// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

namespace PSql.Commands;

[Cmdlet(
    VerbsDiagnostic.Test, "AsyncPSCmdlet",
    SupportsShouldProcess = true,
    ConfirmImpact         = ConfirmImpact.None
)]
public class TestAsyncPSCmdletCommand : AsyncPSCmdlet
{
    public enum TestCase 
    {
        Dispatcher, CancellationToken,
        Run, InvokePendingMainThreadActions, WaitForAsyncActions,
        WriteObject0, WriteObject1, WriteHost,
        WriteDebug, WriteVerbose, WriteInformation0, WriteInformation1,
        WriteWarning, WriteError, WriteProgress, WriteCommandDetail,
        ShouldContinue0, ShouldContinue1, ShouldContinue2,
        ShouldProcess0, ShouldProcess1, ShouldProcess2, ShouldProcess3,
        StopProcessing, ThrowTerminatingError, MultipleDispose, UnmanagedDispose,
    }

    public enum TestMoment
    {
        BeforeProcessing,
        DuringProcessing,
        DuringProcessingAsync,
        AfterProcessing,
    }

    [Parameter(Mandatory = true, Position = 0)]
    public TestCase Case { get; set; }

    [Parameter(Position = 1)]
    public TestMoment Moment { get; set; } = TestMoment.DuringProcessingAsync;

    protected override void BeginProcessing()
    {
        InvokeTestCase(TestMoment.BeforeProcessing);

        base.BeginProcessing();
    }

    protected override void ProcessRecord()
    {
        InvokeTestCase(TestMoment.DuringProcessing);

        Run(ProcessRecordAsync);
    }

    private async Task ProcessRecordAsync()
    {
        await Task.Yield(); // move to another thread

        InvokeTestCase(TestMoment.DuringProcessingAsync);
    }

    protected override void EndProcessing()
    {
        base.EndProcessing();

        InvokeTestCase(TestMoment.AfterProcessing);
    }

    private void InvokeTestCase(TestMoment currentMoment)
    {
        if (Moment != currentMoment)
            return;

        switch (Case)
        {
            case TestCase.Dispatcher:                     TestDispatcher();                     break;
            case TestCase.CancellationToken:              TestCancellationToken();              break;
            case TestCase.Run:                            TestRun();                            break;
            case TestCase.InvokePendingMainThreadActions: TestInvokePendingMainThreadActions(); break;
            case TestCase.WaitForAsyncActions:            TestWaitForAsyncActions();            break;    
            case TestCase.WriteObject0:                   TestWriteObject0();                   break;
            case TestCase.WriteObject1:                   TestWriteObject1();                   break;
            case TestCase.WriteHost:                      TestWriteHost();                      break;
            case TestCase.WriteDebug:                     TestWriteDebug();                     break;
            case TestCase.WriteVerbose:                   TestWriteVerbose();                   break;
            case TestCase.WriteInformation0:              TestWriteInformation0();              break;
            case TestCase.WriteInformation1:              TestWriteInformation1();              break;
            case TestCase.WriteWarning:                   TestWriteWarning();                   break;
            case TestCase.WriteError:                     TestWriteError();                     break;
            case TestCase.WriteProgress:                  TestWriteProgress();                  break;
            case TestCase.WriteCommandDetail:             TestWriteCommandDetail();             break;
            case TestCase.ShouldContinue0:                TestShouldContinue0();                break;
            case TestCase.ShouldContinue1:                TestShouldContinue1();                break;
            case TestCase.ShouldContinue2:                TestShouldContinue2();                break;
            case TestCase.ShouldProcess0:                 TestShouldProcess0();                 break;
            case TestCase.ShouldProcess1:                 TestShouldProcess1();                 break;
            case TestCase.ShouldProcess2:                 TestShouldProcess2();                 break;
            case TestCase.ShouldProcess3:                 TestShouldProcess3();                 break;
            case TestCase.StopProcessing:                 TestStopProcessing();                 break;
            case TestCase.ThrowTerminatingError:          TestThrowTerminatingError();          break;
            case TestCase.MultipleDispose:                TestMultipleDispose();                break;
            default:   /* UnmanagedDispose: */            TestUnmanagedDispose();               break;
        }
    }

    private void TestDispatcher()
    {
        WriteObject(Dispatcher);
    }

    private void TestCancellationToken()
    {
        WriteObject(CancellationToken);
    }

    private void TestRun()
    {
        Run(async () =>
        {
            await Task.Yield(); // move to another thread
            WriteObject("Run invoked code that emitted this output object.");
        });
    }

    private void TestInvokePendingMainThreadActions()
    {
        using var e = new ManualResetEventSlim();

        Run(async () =>
        {
            await Task.Yield(); // move to another thread
            WriteObject("Run invoked code that emitted this output object.");
            e.Set();
        });

        e.Wait();
        InvokePendingMainThreadActions();
        WriteObject("Invoked InvokePendingMainThreadActions.");
    }

    private void TestWaitForAsyncActions()
    {
        Run(async () =>
        {
            await Task.Yield();
            WriteObject("Before WaitForAsyncActions.");
        });

        WaitForAsyncActions();
        WriteObject("Invoked WaitForAsyncActions.");

        Run(async () =>
        {
            await Task.Yield();
            WriteObject("After WaitForAsyncActions.");
        });
    }

    private void TestWriteObject0()
    {
        WriteObject("This is an output object.");
    }

    private void TestWriteObject1()
    {
        WriteObject("This is an output object.", enumerate: false);
    }

    private void TestWriteHost()
    {
        WriteHost("This is a host message.");
    }

    private void TestWriteDebug()
    {
        WriteDebug("This is a debug message.");
    }

    private void TestWriteVerbose()
    {
        WriteVerbose("This is a verbose message.");
    }

    private void TestWriteInformation0()
    {
        WriteInformation(new("This is an information message.", "Test-AsyncPSCmdlet"));
    }

    private void TestWriteInformation1()
    {
        WriteInformation("This is an information message.", null);
    }

    private void TestWriteWarning()
    {
        WriteWarning("This is a warning message.");
    }

    private void TestWriteError()
    {
        WriteError(new(
            new Exception("This is an error message."),
            errorId: "TestError",
            ErrorCategory.NotSpecified,
            targetObject: null
        ));
    }

    private void TestWriteProgress()
    {
        for (var percent = 0;; percent += 25)
        {
            var record = new ProgressRecord(
                activityId:        0,
                activity:          "Test AsyncPSCmdlet",
                statusDescription: "Testing"
            )
            { PercentComplete = percent };

            WriteProgress(record);

            if (percent >= 100)
                break;

            Thread.Sleep(100); // ms
        }
    }

    private void TestWriteCommandDetail()
    {
        WriteCommandDetail("This is a command detail message.");
    }

    [ExcludeFromCodeCoverage(
        Justification = "Always throws in non-interactive test session; end of method unreachable."
    )]
    private void TestShouldContinue0()
    {
        ShouldContinue("Continue?", "Prompt");
    }

    private void TestShouldContinue1()
    {
        var (yesToAll, noToAll) = (true, false);

        ShouldContinue("Continue?", "Prompt", ref yesToAll, ref noToAll);
    }

    private void TestShouldContinue2()
    {
        var (yesToAll, noToAll) = (false, true);

        ShouldContinue("Continue?", "Prompt", hasSecurityImpact: false, ref yesToAll, ref noToAll);
    }

    private void TestShouldProcess0()
    {
        ShouldProcess(target: "AsyncPSCmdlet");
    }

    private void TestShouldProcess1()
    {
        ShouldProcess(target: "AsyncPSCmdlet", action: "Test");
    }

    private void TestShouldProcess2()
    {
        ShouldProcess(
            "Testing AsyncPSCmdlet.",
            "Are you sure you want to test AsyncPSCmdlet?",
            "Prompt"
        );
    }

    private void TestShouldProcess3()
    {
        ShouldProcess(
            "Testing AsyncPSCmdlet.",
            "Are you sure you want to test AsyncPSCmdlet?",
            "Prompt",
            out var reason
        );
    }

    private void TestStopProcessing()
    {
        // Support StopProcessing running on any thread
        Task.Run(StopProcessing);

        CancellationToken.WaitHandle.WaitOne();
    }

    private void TestThrowTerminatingError()
    {
        ThrowTerminatingError(new(
            new Exception("This is an error message."),
            errorId: "TestError",
            ErrorCategory.NotSpecified,
            targetObject: null
        ));
    }

    private void TestMultipleDispose()
    {
        Dispose();
    }

    private void TestUnmanagedDispose()
    {
        Dispose(managed: false);
    }
}
