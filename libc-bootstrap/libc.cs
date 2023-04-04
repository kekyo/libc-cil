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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

[assembly:InternalsVisibleTo("libc.tests")]

[assembly: DebuggerTypeProxy(typeof(C.type.__pointer_visualizer), Target = typeof(void*))]
[assembly: DebuggerTypeProxy(typeof(C.type.__pointer_visualizer), Target = typeof(byte*))]
[assembly: DebuggerTypeProxy(typeof(C.type.__pointer_visualizer), Target = typeof(sbyte*))]

namespace C;

public static partial class text
{
    private static bool get_debugging_switch(string environment_name, bool default_value =
#if DEBUG
            true
#else
            false
#endif
        ) =>
        Environment.GetEnvironmentVariable(environment_name) is { } vs &&
        bool.TryParse(vs, out var v) ? v : default_value;

    private static T get_debugging_switch<T>(string environment_name, T default_value = default!)
        where T : Enum
    {
        try
        {
            if (Environment.GetEnvironmentVariable(environment_name) is { } vs)
            {
                return (T)Enum.Parse(typeof(T), vs, true);
            }
        }
        catch
        {
        }
        return default_value;
    }

    private static readonly bool is_enabled_trap =
        get_debugging_switch("LIBC_CIL_DBG_TRAP");

    ////////////////////////////////////////////////////////////

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
        Marshal.Copy((nint)str, buf, 0, (int)len);
        return Encoding.UTF8.GetString(buf);
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static unsafe string __ngetstrn(sbyte* str, nuint len)
    {
        var l = strlen(str);
        var lr = Math.Min((uint)l, (uint)len);
        var buf = new byte[lr];
        Marshal.Copy((nint)str, buf, 0, (int)lr);
        return Encoding.UTF8.GetString(buf);
    }

    ////////////////////////////////////////////////////////////

    [DebuggerStepperBoundary]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static void __force_trap()
    {
        if (Debugger.IsAttached)
        {
            Debugger.Break();
        }
        else
        {
            Debugger.Launch();
        }
    }

    [DebuggerStepperBoundary]
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static void __trap()
    {
        if (is_enabled_trap)
        {
            __force_trap();
        }
    }

    ////////////////////////////////////////////////////////////

    // void exit(int code);
    public static unsafe void exit(int code) =>
        Environment.Exit(code);
}
