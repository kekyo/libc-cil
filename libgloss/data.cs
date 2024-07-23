/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;

namespace C;

public static partial class data
{
    public static unsafe sbyte** environ = __getenviron();

    private static unsafe sbyte** __getenviron()
    {
        var envs = Environment.GetEnvironmentVariables();
        var penv = (sbyte**)text.heap.malloc(
            (nuint)((envs.Count + 1) * sizeof(sbyte*)), null, 0);
        var index = 0;
        foreach (var entry in envs)
        {
            var kv = (DictionaryEntry)entry!;
            penv[index++] = text.__nstrdup($"{kv.Key}={kv.Value}");
        }
        penv[index] = null;
        return penv;
    }
}
