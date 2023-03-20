/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using C.type;
using System;
using System.ComponentModel;
using System.IO;

namespace C
{
    public static partial class text
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static va_list __va_list_new(RuntimeArgumentHandle handle)
        {
            return new __va_list_runtime_impl(handle);
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static unsafe void* __va_arg_ptr(va_list ap)
        {
            var p = __refvalue(ap.__get(), object);
            if (p is nint n)
            {
                return (void*)n;
            }
            else if (p is uint nu)
            {
                return (void*)nu;
            }
            // Sensitive verification
            else if (sizeof(nint) == 4)
            {
                if (p is int i32)
                {
                    return (void*)i32;
                }
                else if (p is sbyte i8)
                {
                    return (void*)i8;
                }
                else if (p is short i16)
                {
                    return (void*)i16;
                }
                else if (p is uint u32)
                {
                    return (void*)u32;
                }
                else if (p is byte ui8)
                {
                    return (void*)ui8;
                }
                else if (p is ushort u16)
                {
                    return (void*)u16;
                }
            }
            else
            {
                if (p is long i64)
                {
                    return (void*)i64;
                }
                else if (p is ulong u64)
                {
                    return (void*)u64;
                }
            }
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static unsafe sbyte __va_arg_int8(va_list ap)
        {
            var v = __refvalue(ap.__get(), object);
            if (v is sbyte i8)
            {
                return i8;
            }
            else if (v is int i32)
            {
                return (sbyte)i32;
            }
            else if (v is short i16)
            {
                return (sbyte)i16;
            }
            else if (v is byte u8)
            {
                return (sbyte)u8;
            }
            else if (v is ushort u16)
            {
                return (sbyte)u16;
            }
            else if (v is uint u32)
            {
                return (sbyte)u32;
            }
            // Sensitive verification
            else if (sizeof(nint) == 4)
            {
                if (v is nint ni)
                {
                    return (sbyte)ni;
                }
                else if (v is nuint nu)
                {
                    return (sbyte)nu;
                }
            }
            return 0;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static unsafe short __va_arg_int16(va_list ap)
        {
            var v = __refvalue(ap.__get(), object);
            if (v is short i16)
            {
                return i16;
            }
            else if (v is int i32)
            {
                return (short)i32;
            }
            else if (v is sbyte i8)
            {
                return i8;
            }
            else if (v is byte u8)
            {
                return u8;
            }
            else if (v is ushort u16)
            {
                return (short)u16;
            }
            else if (v is uint u32)
            {
                return (short)u32;
            }
            // Sensitive verification
            else if (sizeof(nint) == 4)
            {
                if (v is nint ni)
                {
                    return (short)ni;
                }
                else if (v is nuint nu)
                {
                    return (short)nu;
                }
            }
            return 0;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static unsafe int __va_arg_int32(va_list ap)
        {
            var v = __refvalue(ap.__get(), object);
            if (v is int i32)
            {
                return i32;
            }
            else if (v is sbyte i8)
            {
                return i8;
            }
            else if (v is short i16)
            {
                return i16;
            }
            else if (v is byte u8)
            {
                return u8;
            }
            else if (v is ushort u16)
            {
                return u16;
            }
            else if (v is uint u32)
            {
                return (int)u32;
            }
            // Sensitive verification
            else if (sizeof(nint) == 4)
            {
                if (v is nint ni)
                {
                    return (int)ni;
                }
                else if (v is nuint nu)
                {
                    return (int)nu;
                }
            }
            return 0;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static unsafe long __va_arg_int64(va_list ap)
        {
            var v = __refvalue(ap.__get(), object);
            if (v is long i64)
            {
                return i64;
            }
            else if (v is ulong u64)
            {
                return (long)u64;
            }
            // Sensitive verification
            else if (sizeof(nint) == 8)
            {
                if (v is nint ni)
                {
                    return ni;
                }
                else if (v is nuint nu)
                {
                    return (long)nu;
                }
            }
            return 0;
        }


        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static unsafe byte __va_arg_uint8(va_list ap)
        {
            var v = __refvalue(ap.__get(), object);
            if (v is byte u8)
            {
                return u8;
            }
            else if (v is ushort u16)
            {
                return (byte)u16;
            }
            else if (v is uint u32)
            {
                return (byte)u32;
            }
            else if (v is sbyte i8)
            {
                return (byte)i8;
            }
            else if (v is int i32)
            {
                return (byte)i32;
            }
            else if (v is short i16)
            {
                return (byte)i16;
            }
            // Sensitive verification
            else if (sizeof(nint) == 4)
            {
                if (v is nuint nu)
                {
                    return (byte)nu;
                }
                else if (v is nint ni)
                {
                    return (byte)ni;
                }
            }
            return 0;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static unsafe ushort __va_arg_uint16(va_list ap)
        {
            var v = __refvalue(ap.__get(), object);
            if (v is byte u8)
            {
                return u8;
            }
            else if (v is ushort u16)
            {
                return u16;
            }
            else if (v is uint u32)
            {
                return (ushort)u32;
            }
            else if (v is sbyte i8)
            {
                return (ushort)i8;
            }
            else if (v is short i16)
            {
                return (ushort)i16;
            }
            else if (v is int i32)
            {
                return (ushort)i32;
            }
            // Sensitive verification
            else if (sizeof(nint) == 4)
            {
                if (v is nuint nu)
                {
                    return (ushort)nu;
                }
                else if (v is nint ni)
                {
                    return (ushort)ni;
                }
            }
            return 0;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static unsafe uint __va_arg_uint32(va_list ap)
        {
            var v = __refvalue(ap.__get(), object);
            if (v is byte u8)
            {
                return u8;
            }
            else if (v is ushort u16)
            {
                return u16;
            }
            else if (v is uint u32)
            {
                return u32;
            }
            else if (v is int i32)
            {
                return (uint)i32;
            }
            else if (v is sbyte i8)
            {
                return (uint)i8;
            }
            else if (v is short i16)
            {
                return (uint)i16;
            }
            // Sensitive verification
            else if (sizeof(nint) == 4)
            {
                if (v is nuint nu)
                {
                    return (uint)nu;
                }
                else if (v is nint ni)
                {
                    return (uint)ni;
                }
            }
            return 0;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static unsafe ulong __va_arg_uint64(va_list ap)
        {
            var v = __refvalue(ap.__get(), object);
            if (v is ulong u64)
            {
                return u64;
            }
            else if (v is long i64)
            {
                return (ulong)i64;
            }
            // Sensitive verification
            else if (sizeof(nint) == 8)
            {
                if (v is nuint nu)
                {
                    return nu;
                }
                else if (v is nint ni)
                {
                    return (ulong)ni;
                }
            }
            return 0;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static unsafe float __va_arg_float32(va_list ap)
        {
            var v = __refvalue(ap.__get(), object);
            if (v is float f32)
            {
                return f32;
            }
            else if (v is double f64)
            {
                return (float)f64;
            }
            return 0;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static unsafe double __va_arg_float64(va_list ap)
        {
            var v = __refvalue(ap.__get(), object);
            if (v is double f64)
            {
                return f64;
            }
            else if (v is float f32)
            {
                return f32;
            }
            return 0;
        }

        ///////////////////////////////////////////////////////////////////////

        // int vsprintf(char *buf, char *fmt, va_list ap);
        public static unsafe int vsprintf(sbyte* buf, sbyte* fmt, va_list ap)
        {
            var pbuf = buf;
            nuint len = 0;
            stdio_impl.vwrprintf((p, l) =>
            {
                memcpy(pbuf, p, l);
                pbuf += l;
                len += l;
            },
            fmt, ap);
            *pbuf = 0;
            return (int)len;
        }

        // int vfprintf(FILE *fp, char *fmt, va_list ap);
        public static unsafe int vfprintf(FILE* fp, sbyte* fmt, va_list ap)
        {
            try
            {
                var s = (Stream)__get_obj(fp)!;
                var len = 0;
                stdio_impl.vwrprintf((p, l) =>
                {
                    while (l > 0)
                    {
                        s.WriteByte((byte)*p);
                        p++;
                        l--;
                        len++;
                    }
                },
                fmt, ap);
                return len;
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return -1;
            }
        }
    }
}
