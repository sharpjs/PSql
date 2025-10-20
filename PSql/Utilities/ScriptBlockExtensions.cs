// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Collections.ObjectModel;

namespace PSql;

using static ScopedItemOptions;

internal static class ScriptBlockExtensions
{
    public static Collection<PSObject>
        InvokeWithUnderscore(this ScriptBlock block, object? value)
    {
        var underscore = new PSVariable("_", value, Constant | Private);
        var variables  = new List<PSVariable> { underscore };

        return block.InvokeWithContext(functionsToDefine: null, variables);
    }
}
