// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace PSql;

/// <summary>
///   Abstract view of a PowerShell-like console.
/// </summary>
public interface IConsole
{
    /// <summary>
    ///   Writes the specified object to the output stream.
    /// </summary>
    /// <param name="obj">
    ///   The object to write.
    /// </param>
    void WriteObject(object obj);

    /// <summary>
    ///   Writes the specified object to the output stream.
    /// </summary>
    /// <param name="obj">
    ///   The object to write.
    /// </param>
    void WriteObject(object obj, bool enumerate);

    /// <summary>
    ///   Writes the specified record to the error stream.
    /// </summary>
    /// <param name="record">
    ///   The record to write.
    /// </param>
    void WriteError(ErrorRecord record);

    /// <summary>
    ///   Writes the specified text to the warning stream.
    /// </summary>
    /// <param name="text">
    ///   The text to write.
    /// </param>
    void WriteWarning(string text);

    /// <summary>
    ///   Writes the specified text to the verbose stream.
    /// </summary>
    /// <param name="text">
    ///   The text to write.
    /// </param>
    void WriteVerbose(string text);

    /// <summary>
    ///   Writes the specified text to the debug stream.
    /// </summary>
    /// <param name="text">
    ///   The text to write.
    /// </param>
    void WriteDebug(string text);

    /// <summary>
    ///   Writes the specified text to the host.
    /// </summary>
    /// <param name="text">
    ///   The text to write.
    /// </param>
    /// <param name="newLine">
    ///   Whether a newline should follow the message.
    /// </param>
    /// <param name="foregroundColor">
    ///   The foreground color to use.
    /// </param>
    /// <param name="backgroundColor">
    ///   The background color to use.
    /// </param>
    /// <remarks>
    ///   This method is similar to the PowerShell <c>Write-Host</c> cmdlet.
    /// </remarks>
    void WriteHost(
        string        text,
        bool          newLine         = true,
        ConsoleColor? foregroundColor = null,
        ConsoleColor? backgroundColor = null);

    /// <summary>
    ///   Writes the specified record to the information stream or host.
    /// </summary>
    /// <param name="record">
    ///   The record to write.
    /// </param>
    void WriteInformation(InformationRecord record);

    /// <summary>
    ///   Writes the specified data to the information stream or host.
    /// </summary>
    /// <param name="data">
    ///   The object or message data to write.
    /// </param>
    /// <param name="tags">
    ///   Tags to be associated with the message data.
    /// </param>
    void WriteInformation(object data, string[] tags);

    /// <summary>
    ///   Writes the specified record to the progress stream.
    /// </summary>
    /// <param name="record">
    ///   The record to write.
    /// </param>
    void WriteProgress(ProgressRecord record);

    /// <summary>
    ///   Writes the specified text to the pipeline execution log.
    /// </summary>
    /// <param name="text">
    ///   The text to write.
    /// </param>
    void WriteCommandDetail(string text);

    /// <summary>
    ///   Confirms an action or group of actions via a prompt with the options
    ///   <b>yes</b> and <b>no</b>.
    /// </summary>
    /// <param name="query">
    ///   A question asking whether the action should be performed.
    /// </param>
    /// <param name="caption">
    ///   A window caption that the user interface might display.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if the action should be performed;
    ///   <see langword="false"/> otherwise.
    /// </returns>
    /// <remarks>
    ///   Unlike <c>ShouldProcess</c>, preference settings or command-line
    ///   parameters do not affect this method.
    /// </remarks>
    bool ShouldContinue(string query, string caption);

    /// <summary>
    ///   Confirms an action or group of actions via a prompt with the options
    ///   <b>yes</b>, <b>yes to all</b>, <b>no</b>, and <b>no to all</b>.
    /// </summary>
    /// <param name="query">
    ///   A question asking whether the action should be performed.
    /// </param>
    /// <param name="caption">
    ///   A window caption that the user interface might display.
    /// </param>
    /// <param name="yesToAll">
    ///   <see langword="true"/> to bypass the prompt and return
    ///   <see langword="true"/>; otherwise, on return, this parameter is set
    ///   to <see langword="true"/> if the user responds <b>yes to all</b>.
    /// </param>
    /// <param name="noToAll">
    ///   <see langword="true"/> to bypass the prompt and return
    ///   <see langword="false"/>; otherwise, on return, this parameter is set
    ///   to <see langword="true"/> if the user responds <b>no to all</b>.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if the action should be performed;
    ///   <see langword="false"/> otherwise.
    /// </returns>
    bool ShouldContinue(string query, string caption,
        ref bool yesToAll, ref bool noToAll);

    /// <summary>
    ///   Confirms an action or group of actions via a prompt with the options
    ///   <b>yes</b>, <b>yes to all</b>, <b>no</b>, and <b>no to all</b>.
    /// </summary>
    /// <param name="query">
    ///   A question asking whether the action should be performed.
    /// </param>
    /// <param name="caption">
    ///   A window caption that the user interface might display.
    /// </param>
    /// <param name="hasSecurityImpact">
    ///   <see langword="true"/> if the action has a security impact;
    ///   <see langword="false"/> otherwise.
    ///   The default response is <b>no</b> for actions with a security impact.
    /// </param>
    /// <param name="yesToAll">
    ///   <see langword="true"/> to bypass the prompt and return
    ///   <see langword="true"/>; otherwise, on return, this parameter is set
    ///   to <see langword="true"/> if the user responds <b>yes to all</b>.
    /// </param>
    /// <param name="noToAll">
    ///   <see langword="true"/> to bypass the prompt and return
    ///   <see langword="false"/>; otherwise, on return, this parameter is set
    ///   to <see langword="true"/> if the user responds <b>no to all</b>.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if the action should be performed;
    ///   <see langword="false"/> otherwise.
    /// </returns>
    bool ShouldContinue(string query, string caption, bool hasSecurityImpact,
        ref bool yesToAll, ref bool noToAll);

    /// <summary>
    ///   Confirms whether an action should be performed.
    /// </summary>
    /// <param name="target">
    ///   The name of the target on which an action is to be performed.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if the action should be performed;
    ///   <see langword="false"/> otherwise.
    /// </returns>
    /// <remarks>
    ///   Commands that make changes should invoke a <c>ShouldProcess</c>
    ///   method to give the user an opportunity to confirm that a change
    ///   actually should be performed.
    /// </remarks>
    bool ShouldProcess(string target);

    /// <summary>
    ///   Confirms whether an action should be performed.
    /// </summary>
    /// <param name="target">
    ///   The name of the target on which the action is to be performed.
    /// </param>
    /// <param name="action">
    ///   The name of the action to be performed.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if the action should be performed;
    ///   <see langword="false"/> otherwise.
    /// </returns>
    /// <remarks>
    ///   Commands that make changes should invoke a <c>ShouldProcess</c>
    ///   method to give the user an opportunity to confirm that a change
    ///   actually should be performed.
    /// </remarks>
    bool ShouldProcess(string target, string action);

    /// <summary>
    ///   Confirms whether an action should be performed.
    /// </summary>
    /// <param name="description">
    ///   A description of the action to be performed.
    ///   This text is used for <see cref="ActionPreference.Continue"/>.
    /// </param>
    /// <param name="query">
    ///   A question asking whether the action should be performed.
    ///   This text is used for <see cref="ActionPreference.Inquire"/>.
    /// </param>
    /// <param name="caption">
    ///   A window caption that the user interface might display.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if the action should be performed;
    ///   <see langword="false"/> otherwise.
    /// </returns>
    /// <remarks>
    ///   Commands that make changes should invoke a <c>ShouldProcess</c>
    ///   method to give the user an opportunity to confirm that a change
    ///   actually should be performed.
    /// </remarks>
    bool ShouldProcess(string description, string query, string caption);

    /// <summary>
    ///   Confirms whether an action should be performed.
    /// </summary>
    /// <param name="description">
    ///   A description of the action to be performed.
    ///   This text is used for <see cref="ActionPreference.Continue"/>.
    /// </param>
    /// <param name="query">
    ///   A question asking whether the action should be performed.
    ///   This text is used for <see cref="ActionPreference.Inquire"/>.
    /// </param>
    /// <param name="caption">
    ///   A window caption that the user interface might display.
    /// </param>
    /// <param name="reason">
    ///   On return, indicates the reason(s) for the return value.
    /// </param>
    /// <returns>
    ///   <see langword="true"/> if the action should be performed;
    ///   <see langword="false"/> otherwise.
    /// </returns>
    /// <remarks>
    ///   Commands that make changes should invoke a <c>ShouldProcess</c>
    ///   method to give the user an opportunity to confirm that a change
    ///   actually should be performed.
    /// </remarks>
    bool ShouldProcess(string description, string query, string caption,
        out ShouldProcessReason reason);

    /// <summary>
    ///   Terminates the current command and reports an error.
    /// </summary>
    /// <param name="record">
    ///   The error to report.
    /// </param>
    void ThrowTerminatingError(ErrorRecord record);
}
