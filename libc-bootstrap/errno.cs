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
using C.type;

namespace C
{
    public static partial class data
    {
        public const int ENOMEM = 12;
        public const int EINVAL = 22;
        public const int ERANGE = 34;
    }

    public static partial class text
    {
        [ThreadStatic]
        private static Exception? ex_errno;
        [ThreadStatic]
        private static int internal_errno;
        [ThreadStatic]
        private static __obj_holder? error_message;

        private static void __set_exception_to_errno(Exception ex)
        {
            ex_errno = ex;
            internal_errno = ex_errno switch
            {
                ArgumentException _ => data.EINVAL,
                ArithmeticException _ => data.ERANGE,
                OutOfMemoryException _ => data.ENOMEM,
                _ => Marshal.GetHRForException(ex_errno),
            };
        }

        // int __get_errno();
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static int __get_errno() =>
            internal_errno;

        private static Exception __to_exception(int code) =>
            code switch
            {
                data.EINVAL => new ArgumentException(),
                data.ERANGE => new ArithmeticException(),
                data.ENOMEM => new OutOfMemoryException(),
                _ => new __unknown_errno_exception(internal_errno),
            };

        // void __set_errno(int code);
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void __set_errno(int code)
        {
            internal_errno = code;
            ex_errno = internal_errno != 0 ?
                __to_exception(internal_errno) : null;
        }

        // int errno;
        public static int errno
        {
            get => __get_errno();
            set => __set_errno(value);
        }

        // char *strerror(int errnum);
        public static unsafe sbyte* strerror(int errnum)
        {
            var ex = __to_exception(errnum);
            error_message = ex.Message;
            return error_message;
        }
    }
}
