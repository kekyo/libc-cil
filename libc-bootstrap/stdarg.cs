﻿/////////////////////////////////////////////////////////////////////////////////////
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
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using C.type;

namespace C
{
    namespace type
    {
        internal unsafe struct __va_arg_elem
        {
            public void* ptr;
            public nint handle_ptr;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public sealed unsafe class __va_arglist : CriticalFinalizerObject
        {
            internal __va_arg_elem* arg_elems;
            private int index;

            [EditorBrowsable(EditorBrowsableState.Advanced)]
            public __va_arglist(params object[] args)
            {
                if (args.Length >= 1)
                {
                    this.arg_elems = (__va_arg_elem*)text.malloc(
                        (nuint)sizeof(__va_arg_elem) * (nuint)args.Length);
                    foreach (var arg in args)
                    {
                        this.add(arg);
                    }
                }
            }

            internal __va_arglist(int count)
            {
                if (count >= 1)
                {
                    this.arg_elems = (__va_arg_elem*)text.malloc(
                        (nuint)sizeof(__va_arg_elem) * (nuint)count);
                }
            }

            ~__va_arglist()
            {
                if (this.arg_elems != null)
                {
                    try
                    {
                        for (var index = 0; index < this.index; index++)
                        {
                            var arg_elem = this.arg_elems + index;
                            if (arg_elem->handle_ptr != 0)
                            {
                                if (arg_elem->ptr == null)
                                {
                                    text.__force_trap();
                                }

                                var handle = GCHandle.FromIntPtr(arg_elem->handle_ptr);
                                if (!handle.IsAllocated)
                                {
                                    text.__force_trap();
                                }
                                else
                                {
                                    handle.Free();
                                }
                            }
                            else
                            {
                                if (arg_elem->ptr != null)
                                {
                                    text.__force_trap();
                                }
                            }
                        }
                    }
                    finally
                    {
                        text.free(this.arg_elems);
                        this.arg_elems = null;
                    }
                }
            }

            internal unsafe void add(object? value)
            {
                if (this.arg_elems == null)
                {
                    text.__force_trap();
                    return;
                }

                var arg_elem = this.arg_elems + this.index++;

                var v = value switch
                {
                    float f32 => (double)f32,   // ISO/IEC 9899 6.5.2.2 Function calls - Paragraph 6
                    Enum e => Convert.ChangeType(e, e.GetTypeCode()),
                    _ => value,
                };

                if (v != null)
                {
                    var handle = GCHandle.Alloc(v, GCHandleType.Pinned);

                    arg_elem->ptr = (void*)handle.AddrOfPinnedObject();
                    arg_elem->handle_ptr = GCHandle.ToIntPtr(handle);
                }
                else
                {
                    arg_elem->ptr = null;
                    arg_elem->handle_ptr = 0;
                }
            }
        }

        public unsafe struct va_list
        {
            private __va_arg_elem* arg_elems;

            internal va_list(__va_arg_elem* arg_elems) =>
                this.arg_elems = arg_elems;

            internal void* get()
            {
                if (this.arg_elems == null)
                {
                    text.__force_trap();
                    return null;
                }

                var arg_elem = this.arg_elems++;
                return arg_elem->ptr;
            }
        }
    }

    public static partial class text
    {
        // Public interfaces for typical languages.

        public static unsafe va_list va_start(__va_arglist args) =>
            new(args.arg_elems);

        public static unsafe T va_arg<T>(va_list* ap)
            where T : unmanaged
        {
            var p = ap->get();
            if (p != null)
            {
                return *(T*)p;
            }
            else
            {
                return default!;
            }
        }

        public static unsafe void* va_arg_ptr(va_list* ap) =>
            *(void**)ap->get();

        ///////////////////////////////////////////////////////////////////////

        // Private interfaces for C language.

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static __va_arglist __va_arglist_new(int count) =>
            new(count);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void __va_arglist_add(__va_arglist ap, object? value) =>
            ap.add(value);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe void __va_start(va_list* ap, __va_arglist args) =>
            *ap = new(args.arg_elems);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe void* __va_arg(va_list* ap) =>
            ap->get();

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
                var s = to_stream(fp)!;
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
