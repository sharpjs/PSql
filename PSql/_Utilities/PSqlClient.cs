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

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using Path = System.IO.Path;

#nullable enable

namespace PSql
{
    using static RuntimeInformation;

    internal static class PSqlClient
    {
        private const string
            AssemblyPath = "PSql.Client.dll";

        private static string              LoadPath    { get; }
        private static AssemblyLoadContext LoadContext { get; }
        private static Assembly            Assembly    { get; }

        static PSqlClient()
        {
            LoadPath
                =  Path.GetDirectoryName(typeof(PSqlClient).Assembly.Location)
                ?? Environment.CurrentDirectory;

            LoadContext = new AssemblyLoadContext(nameof(PSqlClient));
            LoadContext.Resolving += OnResolving;

            LoadMicrosoftDataSqlClientAssembly();
            Assembly = LoadAssembly(AssemblyPath);
        }

        private static void LoadMicrosoftDataSqlClientAssembly()
        {
            // Get runtime identifier
            var rid = IsOSPlatform(OSPlatform.Windows) ? "win" : "unix";

            // Get path to MDS DLL
            var path = Path.Combine(
                "runtimes", rid, "lib", "netcoreapp3.1", "Microsoft.Data.SqlClient.dll"
            );

            // Load MDS DLL
            LoadAssembly(path);
        }

        internal static dynamic CreateObject(string typeName, params object?[]? arguments)
        {
            // NULLS: CreateInstance returns null only for valueless instances
            // of Nullable<T>, which cannot happen here.
            return Activator.CreateInstance(GetType(typeName), arguments)!;
        }

        internal static Type GetType(string name)
        {
            // NULLS: Does not return null when trowOnError is true.
            return Assembly.GetType(nameof(PSql) + "." + name, throwOnError: true)!;
        }

        private static Assembly? OnResolving(AssemblyLoadContext context, AssemblyName name)
        {
            var path = name.Name;
            if (string.IsNullOrEmpty(path))
                return null;

            if (!HasAssemblyExtension(path))
                path += ".dll";

            return LoadAssembly(context, path);
        }

        private static Assembly LoadAssembly(string path)
        {
            return LoadAssembly(LoadContext, path);
        }

        private static Assembly LoadAssembly(AssemblyLoadContext context, string path)
        {
            path = Path.Combine(LoadPath, path);

            return context.LoadFromAssemblyPath(path);
        }

        private static bool HasAssemblyExtension(string path)
        {
            return path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);
        }
    }
}
