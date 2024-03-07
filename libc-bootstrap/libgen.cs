/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Threading;

namespace C;

public static partial class text
{
    private static nint __basename = 0;
    private static nint __dirname = 0;
    
    // char *basename(char *path);
    public static unsafe sbyte* basename(sbyte* path)
    {
        var p = __ngetstr(path);
        var bn = Path.GetFileName(p);
        var pbn = __nstrdup(bn);
        var lpbn = (nint)Interlocked.Exchange(ref __basename, (nint)pbn);
        if (lpbn != 0)
        {
            heap.free((void*)lpbn, null, 0);
        }
        return pbn;
    }
    
    // char *dirname(char *path);
    public static unsafe sbyte* dirname(sbyte* path)
    {
        var p = __ngetstr(path);
        var dn = Path.GetDirectoryName(p) switch
        {
            null => ".",
            "" => ".",
            var n => n,
        };
        var pdn = __nstrdup(dn);
        var lpdn = (nint)Interlocked.Exchange(ref __dirname, (nint)pdn);
        if (lpdn != 0)
        {
            heap.free((void*)lpdn, null, 0);
        }
        return pdn;
    }
}
