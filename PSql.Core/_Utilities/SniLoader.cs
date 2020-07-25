using System.IO;
using System.Runtime.InteropServices;

namespace PSql
{
    // Microsoft.Data.SqlClient 2.0.0 and later, at least when used within this
    // PowerShell module, has trouble locating the appropriate SNI DLL.  The
    // workaround is to load it manually.

    internal static class SniLoader
    {
        internal static void Load()
        {
            // Does platform need SNI?
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return; // no

            // Get runtime identifier
            var rid = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X86   => "win-x86",
                Architecture.X64   => "win-x64",
                Architecture.Arm   => "win-arm",
                Architecture.Arm64 => "win-arm64",
                _                  => null
            };

            // Does runtime need SNI?
            if (rid == null)
                return; // no

            // Get path to SNI DLL
            var sniDllPath = Path.Combine(
                Path.GetDirectoryName(typeof(SniLoader).Assembly.Location),
                "runtimes",
                rid,
                "native",
                "Microsoft.Data.SqlClient.SNI.dll"
            );

            // Load SNI DLL
            NativeLibrary.Load(sniDllPath);
        }
    }
}
