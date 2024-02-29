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
using System.Globalization;
using System.Text;
using C.type;

namespace C;

public static partial class text
{
    private static class stdio
    {
        private const sbyte __percent = (sbyte)'%';
        private const sbyte __dot = (sbyte)'.';
        private const sbyte __string = (sbyte)'s';
        private const sbyte __char = (sbyte)'c';
        private const sbyte __long = (sbyte)'l';
        private const sbyte __half = (sbyte)'h';
        private const sbyte __digit = (sbyte)'d';
        private const sbyte __unsigned = (sbyte)'u';
        private const sbyte __float = (sbyte)'f';
        private const sbyte __hexl = (sbyte)'x';
        private const sbyte __hexu = (sbyte)'X';
        private const sbyte __pointer = (sbyte)'p';
        private const sbyte __exp = (sbyte)'e';
        private const sbyte __minus = (sbyte)'-';
        private const sbyte __zero = (sbyte)'0';
        private const sbyte __asterisk = (sbyte)'*';
        
        private static readonly byte[] __null = Encoding.UTF8.GetBytes("(null)");

        private enum __modifiers
        {
            __none,
            __tiny,
            __half,
            __long,
        }

        private static unsafe void output_pads(
             __writer wr, nuint field_width, nuint len, bool zero_pads)
        {
            if (len < field_width)
            {
                var l = field_width - len;
                var buf = new __array_holder<sbyte>((int)l);
                memset(buf, zero_pads ? (sbyte)0x30 : (sbyte)0x20, l);
                wr(buf, l);
            }
        }

        public static unsafe void vwrprintf(
            __writer wr, sbyte* fmt, va_list ap)
        {
            while (*fmt != 0)
            {
                var start = fmt;
                while (*fmt != 0 && *fmt != __percent)
                {
                    fmt++;
                }

                var len = (nuint)(fmt - start);
                // %%
                if (*fmt == __percent && *(fmt + 1) == __percent)
                {
                    len++;
                    wr(start, len);
                    fmt += 2;
                    continue;
                }
                if (len >= 1)
                {
                    wr(start, len);
                    continue;
                }

                fmt++;

                // Precision: %- %1 %* %.
                nuint field_width = 0;
                nuint fraction_width = 0;
                var after_pads = false;
                var zero_pads = false;
                var trim = false;
                if (isdigit(*fmt) != 0 || *fmt == __asterisk || *fmt == __dot || *fmt == __minus)
                {
                    // %-
                    if (*fmt == __minus)  // -
                    {
                        after_pads = true;
                        fmt++;
                    }
                    // %0
                    if (*fmt == __zero)  // 0
                    {
                        zero_pads = true;
                        fmt++;
                    }
                    // %12
                    if (isdigit(*fmt) != 0)
                    {
                        do
                        {
                            field_width = field_width * 10 + (byte)*fmt - 0x30;
                            fmt++;
                        }
                        while (isdigit(*fmt) != 0);
                    }
                    // %*
                    else if (*fmt == __asterisk)
                    {
                        field_width = (nuint)va_arg<int>(&ap);
                        fmt++;
                    }
                    // %.
                    else if (*fmt == __dot)
                    {
                        fmt++;
                        // %.12
                        if (isdigit(*fmt) != 0)
                        {
                            do
                            {
                                fraction_width = fraction_width * 10 + (byte)*fmt - 0x30;
                                fmt++;
                            }
                            while (isdigit(*fmt) != 0);
                        }
                        // %.*
                        else if (*fmt == __asterisk)
                        {
                            field_width = (nuint)va_arg<int>(&ap);
                            trim = true;
                            fmt++;
                        }
                    }
                }

                // Modifier flags: %l
                var mod = __modifiers.__none;
                switch (*fmt)
                {
                    case __half:
                        fmt++;
                        if (*fmt == __half)
                        {
                            mod = __modifiers.__tiny;
                            fmt++;
                        }
                        else
                        {
                            mod = __modifiers.__half;
                        }
                        break;
                    case __long:
                        fmt++;
                        mod = __modifiers.__long;
                        if (*fmt == __long)
                        {
                            fmt++;
                        }
                        break;
                }

                if (*fmt == __string)
                {
                    var ps = (sbyte*)va_arg_ptr(&ap);
                    if (ps != null)
                    {
                        var p = ps;
                        len = 0;
                        if (trim)
                        {
                            while (*p != 0 && len < field_width)
                            {
                                p++;
                                len++;
                            }
                        }
                        else
                        {
                            while (*p != 0)
                            {
                                p++;
                                len++;
                            }
                        }
                        if (!after_pads)
                        {
                            output_pads(wr, field_width, len, zero_pads);
                        }
                        wr(ps, len);
                        if (after_pads)
                        {
                            output_pads(wr, field_width, len, zero_pads);
                        }
                    }
                    else
                    {
                        fixed (void* p = &__null[0])
                        {
                            wr((sbyte*)p, (nuint)__null.Length);
                        }
                    }
                    fmt++;
                }
                else if (*fmt == __char)
                {
                    var v = ((char)va_arg<sbyte>(&ap)).
                        ToString(CultureInfo.InvariantCulture);
                    var bytes = Encoding.UTF8.GetBytes(v);
                    len = (nuint)bytes.Length;
                    if (!after_pads)
                    {
                        output_pads(wr, field_width, len, zero_pads);
                    }
                    fixed (void* p = &bytes[0])
                    {
                        wr((sbyte*)p, len);
                    }
                    if (after_pads)
                    {
                        output_pads(wr, field_width, len, zero_pads);
                    }
                    fmt++;
                }
                else if (*fmt == __digit)
                {
                    var ff = fraction_width >= 1 ?
                        $"G{fraction_width}" : "";
                    var v = "";
                    switch (mod)
                    {
                        case __modifiers.__long:
                            v = va_arg<long>(&ap).
                                ToString(ff, CultureInfo.InvariantCulture);
                            break;
                        default:
                            v = va_arg<int>(&ap).
                                ToString(ff, CultureInfo.InvariantCulture);
                            break;
                    }
                    var bytes = Encoding.UTF8.GetBytes(v);
                    len = (nuint)bytes.Length;
                    if (!after_pads)
                    {
                        output_pads(wr, field_width, len, zero_pads);
                    }
                    fixed (void* p = &bytes[0])
                    {
                        wr((sbyte*)p, len);
                    }
                    if (after_pads)
                    {
                        output_pads(wr, field_width, len, zero_pads);
                    }
                    fmt++;
                }
                else if (*fmt == __unsigned || *fmt == __hexl || *fmt == __hexu || *fmt == __pointer)
                {
                    string ff;
                    var prefix = "";
                    switch (*fmt)
                    {
                        case __hexl:
                        case __hexu:
                            ff = fraction_width >= 1 ? $"{(char)*fmt}{fraction_width}" : $"{(char)*fmt}";
                            break;
                        case __pointer:
                            ff = fraction_width >= 1 ? $"x{fraction_width}" : $"x";
                            prefix = "0x";
                            break;
                        default:
                            ff = fraction_width >= 1 ? $"G{fraction_width}" : "";
                            break;
                    }
                    var v = "";
                    switch (mod)
                    {
                        case __modifiers.__long:
                        case __modifiers.__none when *fmt == __pointer && sizeof(nint) == 8:
                            v = string.Format(
                                CultureInfo.InvariantCulture,
                                $"{prefix}{{0:{ff}}}",
                                va_arg<ulong>(&ap));
                            break;
                        default:
                            v = string.Format(
                                CultureInfo.InvariantCulture,
                                $"{prefix}{{0:{ff}}}",
                                va_arg<uint>(&ap));
                            break;
                    }
                    var bytes = Encoding.UTF8.GetBytes(v);
                    len = (nuint)bytes.Length;
                    if (!after_pads)
                    {
                        output_pads(wr, field_width, len, zero_pads);
                    }
                    fixed (void* p = &bytes[0])
                    {
                        wr((sbyte*)p, len);
                    }
                    if (after_pads)
                    {
                        output_pads(wr, field_width, len, zero_pads);
                    }
                    fmt++;
                }
                else if (*fmt == __float || *fmt == __exp)
                {
                    var ff = fraction_width >= 1 ?
                        $"{(char)*fmt}{fraction_width}" :
                        *fmt == __float ? $"g7" : $"e1";
                    var v = "";
                    switch (mod)
                    {
                        case __modifiers.__tiny:
                        case __modifiers.__half:
                            v = va_arg<float>(&ap).
                                ToString(ff, CultureInfo.InvariantCulture);
                            break;
                        default:
                            v = va_arg<double>(&ap).
                                ToString(ff, CultureInfo.InvariantCulture);
                            break;
                    }
                    var bytes = Encoding.UTF8.GetBytes(v);
                    len = (nuint)bytes.Length;
                    if (!after_pads)
                    {
                        output_pads(wr, field_width, len, zero_pads);
                    }
                    fixed (void* p = &bytes[0])
                    {
                        wr((sbyte*)p, len);
                    }
                    if (after_pads)
                    {
                        output_pads(wr, field_width, len, zero_pads);
                    }
                    fmt++;
                }
                else
                {
                    wr(fmt, len);
                    fmt++;
                }
            }
        }

        public static unsafe int isprintf(
            out string str, sbyte* fmt, va_list ap)
        {
            var buffer = new List<byte>();
            vwrprintf((p, l) =>
            {
                while (l > 0)
                {
                    buffer.Add((byte)*p);
                    p++;
                    l--;
                }
            },
            fmt, ap);
            str = Encoding.UTF8.GetString(buffer.ToArray());
            return buffer.Count;
        }
    }
}