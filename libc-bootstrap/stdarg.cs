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
        public struct va_arglist
        {
            private object[] args;
            private int index;

            public va_arglist(params object[] args)
            {
                this.args = args;
                this.index = args.Length;
            }

            private va_arglist(int count) =>
                this.args = new object[count];

            [EditorBrowsable(EditorBrowsableState.Never)]
            public void add(short value) =>
                this.args[this.index++] = value;
            [EditorBrowsable(EditorBrowsableState.Never)]
            public void add(int value) =>
                this.args[this.index++] = value;
            [EditorBrowsable(EditorBrowsableState.Never)]
            public void add(long value) =>
                this.args[this.index++] = value;
            [EditorBrowsable(EditorBrowsableState.Never)]
            public void add(float value) =>
                this.args[this.index++] = value;
            [EditorBrowsable(EditorBrowsableState.Never)]
            public void add(double value) =>
                this.args[this.index++] = value;
            [EditorBrowsable(EditorBrowsableState.Never)]
            public unsafe void add(nint value) =>
                this.args[this.index++] = value;
            [EditorBrowsable(EditorBrowsableState.Never)]
            public unsafe void add(nuint value) =>
                this.args[this.index++] = value;
            [EditorBrowsable(EditorBrowsableState.Never)]
            public unsafe void add(void* value) =>
                this.args[this.index++] = (nint)value;

            [EditorBrowsable(EditorBrowsableState.Never)]
            public va_list start()
            {
                var handle = GCHandle.Alloc(this.args, GCHandleType.Weak);
                return new(GCHandle.ToIntPtr(handle));
            }

            public static implicit operator va_list(va_arglist args) =>
                args.start();

            [EditorBrowsable(EditorBrowsableState.Never)]
            public static va_arglist create(int count) =>
                new(count);
        }

        public struct va_list
        {
            private readonly nint handle;
            private int index;

            internal va_list(nint handle) =>
                this.handle = handle;

            public T arg<T>()
            {
                var handle = GCHandle.FromIntPtr(this.handle);
                var args = (object[])handle.Target!;
                return args[this.index++] is T v ? v : default!;
            }

            // TODO: Make flexible
            //   byte --> uint8
            //   byte --> uint32
            //   byte --> int32
            //   long --> long
            //   long --> ulong
            //   nint --> nint
            //   nint --> nuint
            //   nint --> void*
            //   ...

            [EditorBrowsable(EditorBrowsableState.Never)]
            public sbyte arg_int8() =>
                this.arg<sbyte>();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public short arg_int16() =>
                this.arg<short>();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public int arg_int32() =>
                this.arg<int>();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public long arg_int64() =>
                this.arg<long>();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public byte arg_uint8() =>
                this.arg<byte>();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public ushort arg_uint16() =>
                this.arg<ushort>();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public uint arg_uint32() =>
                this.arg<uint>();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public ulong arg_uint64() =>
                this.arg<ulong>();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public float arg_float32() =>
                this.arg<float>();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public double arg_float64() =>
                this.arg<double>();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public unsafe nint arg_nint() =>
                this.arg<nint>();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public unsafe nuint arg_nuint() =>
                this.arg<nuint>();
            [EditorBrowsable(EditorBrowsableState.Never)]
            public unsafe void* arg_ptr() =>
                (void*)this.arg<nint>();

            public void end() =>
                this.index = 0;
        }
    }

    public static partial class text
    {
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
