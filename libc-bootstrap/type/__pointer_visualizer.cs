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
using System.Runtime.InteropServices;
using System.Text;

namespace C.type;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed unsafe class __pointer_visualizer
{
    private static readonly nuint max_length =
        Environment.GetEnvironmentVariable("LIBC_CIL_DBG_PTR_MAX_LEN") is { } vs &&
        uint.TryParse(vs, out var v) ?
            Math.Min(v, 4096) :
            64;

    private readonly byte* ptr;

    public string c_str
    {
        get
        {
            if (max_length == 0)
            {
                return "(disabled)";
            }

            try
            {
                return this.ptr != null ?
                    text.__ngetstrn((sbyte*)this.ptr, max_length)! :
                    "(null)";
            }
            catch (Exception ex)
            {
                return $"{ex.GetType().FullName}: {ex.Message}";
            }
        }
    }

    public byte[] bytes_view
    {
        get
        {
            if (max_length == 0)
            {
                return null!;
            }

            try
            {
                if (this.ptr != null)
                {
                    var v = new byte[max_length];
                    Marshal.Copy((nint)this.ptr, v, 0, v.Length);
                    return v;
                }
                else
                {
                    return null!;
                }
            }
            catch (Exception)
            {
                return null!;
            }
        }
    }

    public string bytes_str
    {
        get
        {
            if (max_length == 0)
            {
                return "(disabled)";
            }

            try
            {
                if (this.ptr != null)
                {
                    var sb = new StringBuilder();
                    for (nuint index = 0; index < max_length; index++)
                    {
                        if (sb.Length >= 1)
                        {
                            sb.Append(',');
                        }
                        sb.AppendFormat("0x{0:x2}", *(this.ptr + index));
                    }
                    return sb.ToString();
                }
                else
                {
                    return "(null)";
                }
            }
            catch (Exception ex)
            {
                return $"{ex.GetType().FullName}: {ex.Message}";
            }
        }
    }

    public __pointer_visualizer(void* ptr) =>
        this.ptr = (byte*)ptr;
    public __pointer_visualizer(byte* ptr) =>
        this.ptr = ptr;
    public __pointer_visualizer(sbyte* ptr) =>
        this.ptr = (byte*)ptr;
}
