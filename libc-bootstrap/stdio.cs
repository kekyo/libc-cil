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
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using C.type;

namespace C
{
    namespace type
    {
        [StructLayout(LayoutKind.Sequential, Size = 0, Pack = 8)]
        public readonly struct FILE
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public unsafe delegate void __writer(sbyte* p, nuint len);
    }

    ///////////////////////////////////////////////////////////////////////

    public static partial class data
    {
        public static unsafe readonly FILE* stdin = (FILE*)text.__alloc_obj(Console.OpenStandardInput());
        public static unsafe readonly FILE* stdout = (FILE*)text.__alloc_obj(Console.OpenStandardOutput());
        public static unsafe readonly FILE* stderr = (FILE*)text.__alloc_obj(Console.OpenStandardError());
    }

    ///////////////////////////////////////////////////////////////////////

    public static partial class text
    {
        // int printf(char *fmt, ...);
        public static unsafe int printf(sbyte* fmt, __va_arglist args)
        {
            va_start(out var ap, args);
            var len = stdio_impl.isprintf(out var str, fmt, ap);
            Console.Write(str);
            return len;
        }

        // int sprintf(char *buf, char *fmt, ...);
        public static unsafe int sprintf(sbyte* buf, sbyte* fmt, __va_arglist args)
        {
            va_start(out var ap, args);
            return vsprintf(buf, fmt, ap);
        }

        // int fprintf(FILE *fp, char *fmt, ...);
        public static unsafe int fprintf(FILE* fp, sbyte* fmt, __va_arglist args)
        {
            va_start(out var ap, args);
            return vfprintf(fp, fmt, ap);
        }

        // FILE *fopen(char *pathname, char *mode);
        public static unsafe FILE* fopen(sbyte* pathname, sbyte* mode)
        {
            try
            {
                FileMode m;
                FileAccess a;
                FileShare s;
                switch (__ngetstr(mode))
                {
                    case "w":
                        m = FileMode.Create;
                        a = FileAccess.ReadWrite;
                        s = FileShare.None;
                        break;
                    default:
                        m = FileMode.Open;
                        a = FileAccess.Read;
                        s = FileShare.Read;
                        break;
                }
                var path = __ngetstr(pathname);
                var fs = new FileStream(path, m, a, s);
                return (FILE*)__alloc_obj(fs);
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return (FILE*)0;
            }
        }

        // FILE *open_memstream(char **ptr, size_t *sizeloc);
        public static unsafe FILE* open_memstream(sbyte** ptr, UIntPtr* sizeloc)
        {
            try
            {
                var ms = new __memory_stream(ptr, sizeloc);
                return (FILE*)__alloc_obj(ms);
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return (FILE*)0;
            }
        }

        // long fread(void *ptr, long size, long nmemb, FILE *fp);
        public static unsafe UIntPtr fread(void* ptr, UIntPtr size, UIntPtr nmemb, FILE* fp)
        {
            try
            {
                var s = (Stream)__get_obj(fp)!;
                var buf = new byte[size.ToUInt64() * nmemb.ToUInt64()];
                var read = s.Read(buf, 0, buf.Length);
                Marshal.Copy(buf, 0, (IntPtr)ptr, read);
                return (UIntPtr)read;
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return UIntPtr.Zero;
            }
        }

        // size_t fwrite(void *ptr, size_t size, size_t nmemb, FILE *fp);
        public static unsafe UIntPtr fwrite(void* ptr, UIntPtr size, UIntPtr nmemb, FILE* fp)
        {
            try
            {
                var s = (Stream)__get_obj(fp)!;
                var buf = new byte[size.ToUInt64() * nmemb.ToUInt64()];
                Marshal.Copy((IntPtr)ptr, buf, 0, buf.Length);
                s.Write(buf, 0, buf.Length);
                return nmemb;
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return UIntPtr.Zero;
            }
        }

        // int fflush(FILE *fp);
        public static unsafe int fflush(FILE* fp)
        {
            try
            {
                var s = (Stream)__get_obj(fp)!;
                s.Flush();
                return 0;
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return -1;
            }
        }

        // int fclose(FILE *fp);
        public static unsafe int fclose(FILE* fp)
        {
            try
            {
                var s = (Stream)__get_obj(fp)!;
                s.Close();
                __release_obj(fp);
                return 0;
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return -1;
            }
        }

        // int fputc(int c, FILE *fp);
        public static unsafe int fputc(int c, FILE* fp)
        {
            try
            {
                var s = (Stream)__get_obj(fp)!;
                s.WriteByte((byte)c);
                return 0;
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return -1;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static unsafe void __vwrprintf(
            __writer wr, sbyte* fmt, va_list ap) =>
            stdio_impl.vwrprintf(wr, fmt, ap);

        private static partial class stdio_impl
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

                    // Precision: %- %1 %.
                    nuint field_width = 0;
                    nuint fraction_width = 0;
                    var after_pads = false;
                    var zero_pads = false;
                    if (isdigit(*fmt) != 0 || *fmt == __dot || *fmt == __minus)
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
                        // %.
                        if (*fmt == __dot)
                        {
                            fmt++;
                            // %.12
                            while (isdigit(*fmt) != 0)
                            {
                                fraction_width = fraction_width * 10 + (byte)*fmt - 0x30;
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
                        var ps = (sbyte*)__va_arg_ptr(&ap);
                        if (ps != null)
                        {
                            var p = ps;
                            len = 0;
                            while (*p != 0)
                            {
                                p++;
                                len++;
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
                        fmt++;
                    }
                    else if (*fmt == __char)
                    {
                        var v = ((char)__va_arg_int8(&ap)).
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
                                v = __va_arg_int64(&ap).
                                    ToString(ff, CultureInfo.InvariantCulture);
                                break;
                            default:
                                v = __va_arg_int32(&ap).
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
                                    __va_arg_uint64(&ap));
                                break;
                            default:
                                v = string.Format(
                                    CultureInfo.InvariantCulture,
                                    $"{prefix}{{0:{ff}}}",
                                    __va_arg_uint32(&ap));
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
                            *fmt == __float ?
                                $"{(char)*fmt}6" :
                                $"{(char)*fmt}1";
                        var v = "";
                        switch (mod)
                        {
                            case __modifiers.__tiny:
                            case __modifiers.__half:
                                v = __va_arg_float32(&ap).
                                    ToString(ff, CultureInfo.InvariantCulture);
                                break;
                            default:
                                v = __va_arg_float64(&ap).
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
}
