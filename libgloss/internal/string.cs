/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace C;

public static partial class text
{
    internal static unsafe void* __memcpy(void* dst, void* src, nuint n)
    {
#if true
        return (void*)interop.__memcpy((nint)dst, (nint)src, (nint)n);
#else
        var r = n;
        var d = (byte*)dst;
        var s = (byte*)src;
        while (r > 0)
        {
            *d = *s;
            d++;
            s++;
            r--;
        }
        return dst;
#endif
    }

    internal static unsafe void* __memset(void* s, int c, nuint n)
    {
#if true
        return (void*)interop.__memset((nint)s, c, (nint)n);
#else
        var r = n;
        var s_ = (byte*)s;
        var c_ = (byte)c;
        while (r > 0)
        {
            *s_ = c_;
            s_++;
            r--;
        }
        return s;
#endif
    }

    internal static unsafe int __memcmp(void* s1, void* s2, nuint n)
    {
        var r = n;
        var s1_ = (byte*)s1;
        var s2_ = (byte*)s2;
        while (r > 0)
        {
            var num = *s1_ - *s2_;
            if (num != 0)
            {
                return num;
            }
            s1_++;
            s2_++;
            r--;
        }
        return 0;
    }

    internal static unsafe nuint __strlen(sbyte* p)
    {
        nuint len = 0;
        while (*p != 0)
        {
            p++;
            len++;
        }
        return len;
    }

    internal static unsafe int __strncmp(sbyte* s1, sbyte* s2, nuint n)
    {
        var r = n;
        while (r > 0)
        {
            var num = (byte)*s1 - (byte)*s2;
            if (num != 0)
            {
                return num;
            }
            if (*s1 == 0)
            {
                break;
            }
            s1++;
            s2++;
            r--;
        }
        return 0;
    }
}
