#if ISOLATED
/*
    Copyright 2020 Jeffrey Sharp

    Permission to use, copy, modify, and distribute this software for any
    purpose with or without fee is hereby granted, provided that the above
    copyright notice and this permission notice appear in all copies.

    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
*/

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
#endif
