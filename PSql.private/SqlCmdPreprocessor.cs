// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Collections;
using System.Globalization;

namespace PSql;

/// <summary>
///   Facade for <see cref="Prequel.SqlCmdPreprocessor"/>.
/// </summary>
/// <remarks>
///   This type exists as part of the technique recommended
///   <a href="https://learn.microsoft.com/en-us/powershell/scripting/dev-cross-plat/resolving-dependency-conflicts">here</a>
///   to isolate dependencies of PSql so that they do not conflict with
///   dependencies of other modules.
/// </remarks>
public class SqlCmdPreprocessor
{
    private readonly Prequel.SqlCmdPreprocessor _preprocessor;

    /// <summary>
    ///   Initializes a new <see cref="SqlCmdPreprocessor"/> instance.
    /// </summary>
    public SqlCmdPreprocessor()
    {
        _preprocessor = new();
    }

    /// <summary>
    ///   Defines the specified <c>sqlcmd</c> variables.
    /// </summary>
    /// <param name="entries">
    ///   The variables to define.
    /// </param>
    /// <returns>
    ///   The preprocessor instance, to facilitate chaining.
    /// </returns>
    public SqlCmdPreprocessor WithVariables(IDictionary? entries)
    {
        if (entries == null)
            return this;

        var variables = _preprocessor.Variables;

        foreach (var obj in entries)
        {
            if (obj is not DictionaryEntry entry)
                continue;

            if (entry.Key is null)
                continue;

            var key = Convert.ToString(entry.Key, CultureInfo.InvariantCulture);

            if (string.IsNullOrEmpty(key))
                continue;

            var value = Convert.ToString(entry.Value, CultureInfo.InvariantCulture);

            variables[key] = value ?? string.Empty;
        }

        return this;
    }

    /// <summary>
    ///   Preprocesses the specified text.
    /// </summary>
    /// <param name="text">
    ///   The text to be preprocessed.
    /// </param>
    /// <returns>
    ///   The result of preprocessing <paramref name="text"/>, split into
    ///   <c>GO</c>-separated batches.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="text"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="Prequel.SqlCmdException">
    ///   The preprocessor encountered an error in the usage of a <c>sqlcmd</c>
    ///   feature.
    /// </exception>
    public IEnumerable<string> Process(string script)
    {
        return _preprocessor.Process(script);
    }
}
