// Copyright Subatomix Research Inc.
// SPDX-License-Identifier: MIT

using System.Diagnostics.CodeAnalysis;

#if !NET5_0
namespace System.Runtime.CompilerServices;

[ExcludeFromCodeCoverage]
internal static class IsExternalInit
{
    // The presence of this class is required to make the compiler happy when
    // using C# 9 record types on target frameworks < net5.0.
}
#endif
