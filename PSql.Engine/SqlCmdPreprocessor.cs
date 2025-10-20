// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Collections;
using System.Globalization;

namespace PSql;

/// <inheritdoc cref="Prequel.SqlCmdPreprocessor"/>
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
    public void SetVariables(IDictionary? entries)
    {
        if (entries == null)
            return;

        var variables = _preprocessor.Variables;

        variables.Clear();

        foreach (var obj in entries)
        {
            if (obj is not DictionaryEntry entry)
                continue;

            if (entry.Key is null)
                continue;

            var key = Convert.ToString(entry.Key, CultureInfo.InvariantCulture);

            if (key.IsNullOrEmpty())
                continue;

            var value = Convert.ToString(entry.Value, CultureInfo.InvariantCulture);

            variables[key] = value ?? string.Empty;
        }
    }

    /// <inheritdoc cref="Prequel.SqlCmdPreprocessor.Process(string, string?)"/>
    public IEnumerable<string> Process(string text)
    {
        return _preprocessor.Process(text);
    }
}
