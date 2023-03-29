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
        private static int __errno;
        [ThreadStatic]
        private static __obj_holder? __error_message;

        // int* __errno_location();
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe int* __errno_location()
        {
            fixed (int* p = &__errno)
            {
                return p;
            }
        }

        // int errno;
        public static int errno
        {
            get => __errno;
            set => __errno = value;
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static void __set_exception_to_errno(Exception ex) =>
            __errno = ex switch
            {
                ArgumentException _ => data.EINVAL,
                ArithmeticException _ => data.ERANGE,
                OutOfMemoryException _ => data.ENOMEM,
                _ => Marshal.GetHRForException(ex),
            };

        private static Exception __to_exception(int code) =>
            code switch
            {
                data.EINVAL => new ArgumentException(),
                data.ERANGE => new ArithmeticException(),
                data.ENOMEM => new OutOfMemoryException(),
                _ => Marshal.GetExceptionForHR(__errno)!,
            };

        // char *strerror(int errnum);
        public static unsafe sbyte* strerror(int errnum)
        {
            var ex = __to_exception(errnum);
            __error_message = ex.Message;
            return __error_message;
        }
    }
}
