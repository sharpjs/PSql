using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace PSql
{
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
}
