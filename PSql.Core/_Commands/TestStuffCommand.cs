using System;
using System.Management.Automation;

namespace PSql
{
    [Cmdlet(VerbsDiagnostic.Test, "Stuff")]
    public class TestStuffCommand : Cmdlet
    {
        protected override void ProcessRecord()
        {
        }
    }
}
