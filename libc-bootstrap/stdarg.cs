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
using System.IO;
using System.Runtime.InteropServices;
using C.type;

namespace C
{
    namespace type
    {
        public struct __va_arglist
        {
            private object[] args;
            private int index;

            internal __va_arglist(object[] args)
            {
                this.args = args;
                this.index = args.Length;
            }

            internal __va_arglist(int count) =>
                this.args = new object[count];

            internal void add(object value) =>
                this.args[this.index++] = value;

            internal unsafe va_list start()
            {
                var handle = GCHandle.Alloc(this.args, GCHandleType.Weak);
                return new(GCHandle.ToIntPtr(handle));
            }
        }

        public struct va_list
        {
            private readonly nint handle;
            private int index;

            internal va_list(nint handle) =>
                this.handle = handle;

            internal object get()
            {
                var handle = GCHandle.FromIntPtr(this.handle);
                var args = (object[])handle.Target!;
                return args[this.index++];
            }

            internal void end() =>
                this.index = 0;
        }
    }

    public static partial class text
    {
        public static __va_arglist va_arglist(params object[] args) =>
            new __va_arglist(args);

        public static void va_start(out va_list ap, object[] args) =>
            ap = new __va_arglist(args).start();
        public static void va_start(out va_list ap, __va_arglist args) =>
            ap = args.start();

        ///////////////////////////////////////////////////////////////////////

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static __va_arglist __va_arglist_new(int count) =>
            new(count);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void __va_arglist_add(__va_arglist ap, object value) =>
            ap.add(value);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe void __va_start(va_list* ap, __va_arglist args) =>
            *ap = args.start();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe object __va_arg(va_list* ap) =>
            ap->get();

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe bool __va_arg_bool(va_list* ap) =>
            ap->get() switch
            {
                byte u8 => u8 != 0,
                ushort u16 => u16 != 0,
                uint u32 => u32 != 0,
                sbyte i8 => i8 != 0,
                short i16 => i16 != 0,
                int i32 => i32 != 0,
                bool b => b,
                nint ni when sizeof(nuint) == 4 => ni != 0,
                nuint nu when sizeof(nuint) == 4 => nu != 0,
                _ => false,
            };

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe sbyte __va_arg_int8(va_list* ap) =>
            ap->get() switch
            {
                sbyte i8 => i8,
                short i16 => (sbyte)i16,
                int i32 => (sbyte)i32,
                byte u8 => (sbyte)u8,
                ushort u16 => (sbyte)u16,
                uint u32 => (sbyte)u32,
                bool b => b ? (sbyte)1 : (sbyte)0,
                nint ni when sizeof(nuint) == 4 => (sbyte)ni,
                nuint nu when sizeof(nuint) == 4 => (sbyte)nu,
                _ => 0,
            };

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe short __va_arg_int16(va_list* ap) =>
            ap->get() switch
            {
                sbyte i8 => i8,
                short i16 => i16,
                int i32 => (short)i32,
                byte u8 => u8,
                ushort u16 => (short)u16,
                uint u32 => (short)u32,
                bool b => b ? (short)1 : (short)0,
                nint ni when sizeof(nuint) == 4 => (short)ni,
                nuint nu when sizeof(nuint) == 4 => (short)nu,
                _ => 0,
            };

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe int __va_arg_int32(va_list* ap) =>
            ap->get() switch
            {
                sbyte i8 => i8,
                short i16 => i16,
                int i32 => i32,
                byte u8 => u8,
                ushort u16 => u16,
                uint u32 => (int)u32,
                bool b => b ? 1 : 0,
                nint ni when sizeof(nuint) == 4 => (int)ni,
                nuint nu when sizeof(nuint) == 4 => (int)nu,
                _ => 0,
            };

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe long __va_arg_int64(va_list* ap) =>
            ap->get() switch
            {
                long i64 => i64,
                ulong u64 => (long)u64,
                nint ni when sizeof(nint) == 8 => ni,
                nuint nu when sizeof(nint) == 8 => (long)nu,
                double f64 => *(long*)&f64,
                _ => 0,
            };

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe byte __va_arg_uint8(va_list* ap) =>
            ap->get() switch
            {
                byte u8 => u8,
                ushort u16 => (byte)u16,
                uint u32 => (byte)u32,
                sbyte i8 => (byte)i8,
                short i16 => (byte)i16,
                int i32 => (byte)i32,
                bool b => b ? (byte)1 : (byte)0,
                nint ni when sizeof(nuint) == 4 => (byte)ni,
                nuint nu when sizeof(nuint) == 4 => (byte)nu,
                _ => 0,
            };

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe ushort __va_arg_uint16(va_list* ap) =>
            ap->get() switch
            {
                byte u8 => u8,
                ushort u16 => u16,
                uint u32 => (ushort)u32,
                sbyte i8 => (ushort)i8,
                short i16 => (ushort)i16,
                int i32 => (ushort)i32,
                bool b => b ? (ushort)1 : (ushort)0,
                nint ni when sizeof(nuint) == 4 => (ushort)ni,
                nuint nu when sizeof(nuint) == 4 => (ushort)nu,
                _ => 0,
            };

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe uint __va_arg_uint32(va_list* ap) =>
            ap->get() switch
            {
                byte u8 => u8,
                ushort u16 => u16,
                uint u32 => u32,
                sbyte i8 => (uint)i8,
                short i16 => (uint)i16,
                int i32 => (uint)i32,
                bool b => b ? (uint)1 : (uint)0,
                nint ni when sizeof(nuint) == 4 => (uint)ni,
                nuint nu when sizeof(nuint) == 4 => (uint)nu,
                _ => 0,
            };

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe ulong __va_arg_uint64(va_list* ap) =>
            ap->get() switch
            {
                ulong u64 => u64,
                long i64 => (ulong)i64,
                nuint nu when sizeof(nuint) == 8 => nu,
                nint ni when sizeof(nuint) == 8 => (ulong)ni,
                double f64 => *(ulong*)&f64,
                _ => 0,
            };

        [EditorBrowsable(EditorBrowsableState.Never)]
        private static unsafe float to_float32(nint ni)
        {
            var v = (int)ni;
            return *(float*)&v;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        private static unsafe float to_float32(nuint ni)
        {
            var v = (uint)ni;
            return *(float*)&v;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe float __va_arg_float32(va_list* ap) =>
            ap->get() switch
            {
                float f32 => f32,
                double f64 => (float)f64,
                int i32 => *(float*)&i32,
                uint u32 => *(float*)&u32,
                nint ni when sizeof(nuint) == 4 => to_float32(ni),
                nuint nu when sizeof(nuint) == 4 => to_float32(nu),
                _ => 0,
            };

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe double __va_arg_float64(va_list* ap) =>
            ap->get() switch
            {
                double f64 => f64,
                float f32 => f32,
                long i64 => *(double*)&i64,
                ulong u64 => *(double*)&u64,
                nint ni when sizeof(nuint) == 8 => *(double*)&ni,
                nuint nu when sizeof(nuint) == 8 => *(double*)&nu,
                _ => 0,
            };

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe char __va_arg_char16(va_list* ap) =>
            ap->get() switch
            {
                byte u8 => (char)u8,
                ushort u16 => (char)u16,
                uint u32 => (char)u32,
                sbyte i8 => (char)i8,
                short i16 => (char)i16,
                int i32 => (char)i32,
                bool b => b ? (char)1 : (char)0,
                nint ni when sizeof(nuint) == 4 => (char)ni,
                nuint nu when sizeof(nuint) == 4 => (char)nu,
                _ => (char)0,
            };

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe nint __va_arg_nint(va_list* ap) =>
            sizeof(nint) == 8 ?
                ap->get() switch
                {
                    ulong u64 => (nint)u64,
                    long i64 => (nint)i64,
                    nuint nu => (nint)nu,
                    nint ni => ni,
                    double f64 => (nint)(*(long*)&f64),
                    _ => 0,
                } :
                ap->get() switch
                {
                    uint u32 => (nint)u32,
                    int i32 => i32,
                    nuint nu => (nint)nu,
                    nint ni => ni,
                    float f32 => *(int*)&f32,
                    _ => 0,
                };

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe nuint __va_arg_nuint(va_list* ap) =>
            sizeof(nuint) == 8 ?
                ap->get() switch
                {
                    ulong u64 => (nuint)u64,
                    long i64 => (nuint)i64,
                    nuint nu => nu,
                    nint ni => (nuint)ni,
                    double f64 => (nuint)(*(ulong*)&f64),
                    _ => 0,
                } :
                ap->get() switch
                {
                    uint u32 => u32,
                    int i32 => (nuint)i32,
                    nuint nu => nu,
                    nint ni => (nuint)ni,
                    float f32 => *(uint*)&f32,
                    _ => 0,
                };

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe void* __va_arg_ptr(va_list* ap) =>
            sizeof(nint) == 8 ?
                ap->get() switch
                {
                    ulong u64 => (void*)u64,
                    long i64 => (void*)i64,
                    nuint nu => (void*)nu,
                    nint ni => (void*)ni,
                    double f64 => (void*)*(ulong*)&f64,
                    _ => null,
                } :
                ap->get() switch
                {
                    uint u32 => (void*)u32,
                    int i32 => (void*)i32,
                    nuint nu => (void*)(uint)nu,
                    nint ni => (void*)(int)ni,
                    float f32 => (void*)*(uint*)&f32,
                    _ => null,
                };

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
