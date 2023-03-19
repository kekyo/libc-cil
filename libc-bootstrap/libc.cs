/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

[assembly:InternalsVisibleTo("libc.tests")]

namespace C;

public static partial class text
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static unsafe void* __alloc_obj(object obj)
    {
        var handle = GCHandle.Alloc(obj, GCHandleType.Normal);
        return GCHandle.ToIntPtr(handle).ToPointer();
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static unsafe object? __get_obj(void* p)
    {
        var handle = GCHandle.FromIntPtr((nint)p);
        return handle.Target;
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static unsafe void __release_obj(void* p)
    {
        var handle = GCHandle.FromIntPtr((nint)p);
        handle.Free();
    }

    ////////////////////////////////////////////////////////////

    // They are using Encoding.UTF8 instead of Marshal.StringToHGlobalAnsi().
    // Because it will not always convert on UTF8.
    // And Marshal.StringToCoTaskMemUTF8 is not available on older .NET.

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static unsafe sbyte* __nstrdup(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        fixed (byte* p = &bytes[0])
        {
            var size = (nuint)bytes.Length;
            var mem = (sbyte*)malloc(size + 1);
            memcpy(mem, p, size);
            *(mem + size) = 0;
            return mem;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static unsafe string __ngetstr(sbyte* str)
    {
        var len = strlen(str);
        var buf = new byte[len];
        Marshal.Copy((IntPtr)str, buf, 0, (int)len);
        return Encoding.UTF8.GetString(buf);
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static unsafe string __ngetstrn(sbyte* str, nuint len)
    {
        var buf = new byte[len];
        Marshal.Copy((IntPtr)str, buf, 0, (int)len);
        return Encoding.UTF8.GetString(buf);
    }

    ////////////////////////////////////////////////////////////

    // void exit(int code);
    public static unsafe void exit(int code) =>
        Environment.Exit(code);
}
