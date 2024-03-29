/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using System.Diagnostics;

namespace C;

public static partial class text
{
    // void __assert(const char *expr, const char *file, int line)
    [DebuggerStepperBoundary]
    public static unsafe void __assert(sbyte* expr, sbyte* file, int line) =>
        Trace.Fail($"{__ngetstr(file)}:{line}: Assertion `{__ngetstr(expr) ?? "(unknown)"}' failed.");
}
