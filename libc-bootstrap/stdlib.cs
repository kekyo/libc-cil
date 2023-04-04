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
using System.Threading;

namespace C;

public static partial class text
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static void _CrtSetBreakAlloc(long lBreakAlloc) =>
        stdlib_impl.set_break_allocation(lBreakAlloc);

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static unsafe int _CrtCheckMemory() =>
        stdlib_impl.verify_heap() ? 1 : 0;

    // void *malloc(size_t size);
    public static unsafe void* malloc(
        nuint size) =>
        stdlib_impl.malloc(size, null, 0);

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static unsafe void* _malloc_dbg(
        nuint size, sbyte* filename, int linenumber) =>
        stdlib_impl.malloc(size, filename, linenumber);

    // void *calloc(size_t nmemb, size_t size);
    public static unsafe void* calloc(
        nuint nmemb, nuint size) =>
        stdlib_impl.calloc(nmemb, size, null, 0);

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static unsafe void* _calloc_dbg(
        nuint nmemb, nuint size, sbyte* filename, int linenumber) =>
        stdlib_impl.calloc(nmemb, size, filename, linenumber);

    // void free(void *p);
    public static unsafe void free(
        void* p) =>
        stdlib_impl.free(p);

    ///////////////////////////////////////////////////////////////////////

    // unsigned long strtoul(const char *nptr, char **endptr, int base);
    public static unsafe ulong strtoul(sbyte* nptr, sbyte** endptr, int @base) =>
        stdlib_impl.strtoul(nptr, endptr, @base);

    // double strtod(const char *nptr, char **endptr);
    public static unsafe double strtod(sbyte* nptr, sbyte** endptr) =>
        stdlib_impl.strtod(nptr, endptr);

    ///////////////////////////////////////////////////////////////////////

    private static class stdlib_impl
    {
        // https://learn.microsoft.com/en-us/cpp/c-runtime-library/crt-debug-heap-details

        private static readonly ulong no_mans_land_bytes = 0xfdfdfdfdfdfdfdfdUL;
        private static readonly ulong dead_bytes = 0xdeadbeefdeadbeefUL;

        private enum HeapCheckModes
        {
            None,
            Check,
            Trap,
        }

        private static readonly HeapCheckModes heap_check_mode =
            get_debugging_switch("LIBC_CIL_DBG_HEAP",
#if DEBUG
                HeapCheckModes.Trap
#else
                HeapCheckModes.None
#endif
                );

        private static readonly bool heap_check_always =
            get_debugging_switch("LIBC_CIL_DBG_HEAP_ALWAYS");

        private unsafe struct CrtMemBlockHeader
        {
            private static unsafe CrtMemBlockHeader _head;
            public static unsafe CrtMemBlockHeader* head;

            static CrtMemBlockHeader()
            {
                fixed (CrtMemBlockHeader* p = &_head)
                {
                    head = p;
                    _head._block_header_next = p;
                    _head._block_header_prev = p;
                }
            }

            public CrtMemBlockHeader* _block_header_next;
            public CrtMemBlockHeader* _block_header_prev;
            public sbyte* _filename;
            public int _line_number;
            public nuint _data_size;
            public long _request_number;
            public ulong _gap;
        }

        private static long _request_number;
        private static long _break_number;
        private static readonly object heap_locker = new object();

        [DebuggerStepperBoundary]
        private static void try_trap_heap(bool force)
        {
            if (force || heap_check_mode == HeapCheckModes.Trap)
            {
                __force_trap();
            }
        }

        public static void set_break_allocation(long number) =>
            _break_number = number;

        private static unsafe bool verify_heap(bool force)
        {
            var header = CrtMemBlockHeader.head->_block_header_next;
            var count = 0;

            while (header != CrtMemBlockHeader.head)
            {
                if (header->_gap != no_mans_land_bytes)
                {
                    try_trap_heap(force);
                    return false;
                }

                if (header->_data_size == 0)
                {
                    try_trap_heap(force);
                    return false;
                }

                if (header->_block_header_prev->_block_header_next != header)
                {
                    try_trap_heap(force);
                    return false;
                }
                if (header->_block_header_next->_block_header_prev != header)
                {
                    try_trap_heap(force);
                    return false;
                }

                var _data = (byte*)(header + 1);
                var _another_gap = _data + header->_data_size;

                fixed (void* ap = &no_mans_land_bytes)
                {
                    if (memcmp(_another_gap, ap, sizeof(ulong)) != 0)
                    {
                        try_trap_heap(force);
                        return false;
                    }
                }

                header = header->_block_header_next;
                count++;
            }

            return true;
        }

        public static bool verify_heap()
        {
            if (heap_check_mode == HeapCheckModes.None)
            {
                return true;
            }

            lock (heap_locker)
            {
                return verify_heap(false);
            }
        }

        // We use HGlobal family for allocator instead of COM allocator.
        // Because they can receive size parameter by intptr type.

        public static unsafe void* malloc(
            nuint size, sbyte* filename, int linenumber)
        {
            if (size == 0)
            {
                __trap();
                return null;
            }

            if (heap_check_mode != HeapCheckModes.None)
            {
                var number = Interlocked.Increment(ref _request_number);
                if (number == _break_number)
                {
                    __force_trap();
                }

                var total_size =
                    (nuint)sizeof(CrtMemBlockHeader) + size +
                    sizeof(ulong);

                var header = (CrtMemBlockHeader*)Marshal.AllocHGlobal((nint)total_size);
                if (header == null)
                {
                    try_trap_heap(false);
                    return null;
                }

                var _data = (byte*)(header + 1);

                lock (heap_locker)
                {
                    if (heap_check_always)
                    {
                        verify_heap(true);
                    }

                    if (CrtMemBlockHeader.head->_block_header_next->_block_header_prev != CrtMemBlockHeader.head)
                    {
                        try_trap_heap(true);
                        return null;
                    }

                    header->_block_header_next = CrtMemBlockHeader.head->_block_header_next;
                    CrtMemBlockHeader.head->_block_header_next = header;

                    header->_block_header_prev = CrtMemBlockHeader.head;
                    header->_block_header_next->_block_header_prev = header;

                    header->_filename = filename;
                    header->_line_number = linenumber;
                    header->_data_size = size;
                    header->_request_number = number;
                    header->_gap = no_mans_land_bytes;

                    var _another_gap = _data + size;
                    fixed (void* ap = &no_mans_land_bytes)
                    {
                        memcpy(_another_gap, ap, sizeof(ulong));
                    }
                }

                memset(_data, 0xcd, size);

                return _data;
            }
            else
            {
                return (void*)Marshal.AllocHGlobal((nint)size);
            }
        }

        public static unsafe void* calloc(
            nuint nmemb, nuint size, sbyte* filename, int linenumber)
        {
            var s = nmemb * size;
            var p = malloc(s, filename, linenumber);
            if (p == null)
            {
                return null;
            }

            // malloc is returned uninitialized memory.
            memset(p, 0, s);
            return p;
        }

        public static unsafe void free(void* _data)
        {
            if (_data == null)
            {
                return;
            }

            if (heap_check_mode != HeapCheckModes.None)
            {
                var header = ((CrtMemBlockHeader*)_data) - 1;

                lock (heap_locker)
                {
                    if (heap_check_always)
                    {
                        verify_heap(true);
                    }

                    var next = header->_block_header_next;
                    var prev = header->_block_header_prev;

                    if (next->_block_header_prev != header)
                    {
                        try_trap_heap(true);
                        return;
                    }

                    if (prev->_block_header_next != header)
                    {
                        try_trap_heap(true);
                        return;
                    }

                    next->_block_header_prev = prev;
                    prev->_block_header_next = next;
                }

                header->_block_header_next = null;
                header->_block_header_prev = null;
                header->_gap = dead_bytes;

                memset(_data, 0xdd, header->_data_size);

                var _another_gap = ((byte*)_data) + header->_data_size;
                fixed (void* ap = &dead_bytes)
                {
                    memcpy(_another_gap, ap, sizeof(ulong));
                }

                Marshal.FreeHGlobal((nint)header);
            }
            else
            {
                Marshal.FreeHGlobal((nint)_data);
            }
        }

        ///////////////////////////////////////////////////////////////////////

        // strtoul ported from cpython: https://github.com/python/cpython/blob/main/Python/mystrtoul.c

        /* Table of digit values for 8-bit string -> integer conversion.
         * '0' maps to 0, ..., '9' maps to 9.
         * 'a' and 'A' map to 10, ..., 'z' and 'Z' map to 35.
         * All other indices map to 37.
         * Note that when converting a base B string, a char c is a legitimate
         * base B digit iff __digit_value[__char_mask(c)] < B.
         */
        private static readonly byte[] __digit_value = {
            37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37,
            37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37,
            37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37,
            0,  1,  2,  3,  4,  5,  6,  7,  8,  9,  37, 37, 37, 37, 37, 37,
            37, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
            25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 37, 37, 37, 37, 37,
            37, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24,
            25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 37, 37, 37, 37, 37,
            37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37,
            37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37,
            37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37,
            37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37,
            37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37,
            37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37,
            37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37,
            37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37, 37,
        };

        private static int __char_mask(int c) =>
            (byte)(c & 0xff);

        /* Static overflow check values for bases 2 through 36.
         * __small_max[base] is the largest unsigned long i such that
         * i * base doesn't overflow unsigned long.
         */
        private static readonly ulong[] __small_max = {
            0, /* bases 0 and 1 are invalid */
            0,
            ulong.MaxValue / 2,
            ulong.MaxValue / 3,
            ulong.MaxValue / 4,
            ulong.MaxValue / 5,
            ulong.MaxValue / 6,
            ulong.MaxValue / 7,
            ulong.MaxValue / 8,
            ulong.MaxValue / 9,
            ulong.MaxValue / 10,
            ulong.MaxValue / 11,
            ulong.MaxValue / 12,
            ulong.MaxValue / 13,
            ulong.MaxValue / 14,
            ulong.MaxValue / 15,
            ulong.MaxValue / 16,
            ulong.MaxValue / 17,
            ulong.MaxValue / 18,
            ulong.MaxValue / 19,
            ulong.MaxValue / 20,
            ulong.MaxValue / 21,
            ulong.MaxValue / 22,
            ulong.MaxValue / 23,
            ulong.MaxValue / 24,
            ulong.MaxValue / 25,
            ulong.MaxValue / 26,
            ulong.MaxValue / 27,
            ulong.MaxValue / 28,
            ulong.MaxValue / 29,
            ulong.MaxValue / 30,
            ulong.MaxValue / 31,
            ulong.MaxValue / 32,
            ulong.MaxValue / 33,
            ulong.MaxValue / 34,
            ulong.MaxValue / 35,
            ulong.MaxValue / 36,
        };

        /* maximum digits that can't ever overflow for bases 2 through 36,
         * calculated by [int(math.floor(math.log(2**32, i))) for i in range(2, 37)].
         */
        private static unsafe readonly int[] __digit_limit =
            new int[] {
                0,   0, 64, 40, 32, 27, 24, 22, 21, 20,   /*  0 -  9 */
                19,  18, 17, 17, 16, 16, 16, 15, 15, 15,  /* 10 - 19 */
                14,  14, 14, 14, 13, 13, 13, 13, 13, 13,  /* 20 - 29 */
                13,  12, 12, 12, 12, 12, 12};             /* 30 - 36 */

        // uint64_t strtoul(char *nptr, char **endptr, int base);
        public static unsafe ulong strtoul(sbyte* str, sbyte** ptr, int @base)
        {
            ulong result = 0;  /* return value of the function */
            int c;             /* current input character */
            int ovlimit;       /* required digits to overflow */

            /* skip leading white space */
            while (*str != 0 && isspace(*str) != 0)
                ++str;

            /* check for leading 0b, 0o or 0x for auto-base or base 16 */
            switch (@base)
            {
                case 0:             /* look for leading 0b, 0o or 0x */
                    if (*str == 0x30)   // 0
                    {
                        ++str;
                        if (*str == 0x58 || *str == 0x78)    // X, x
                        {
                            /* there must be at least one digit after 0x */
                            if (__digit_value[__char_mask(str[1])] >= 16)
                            {
                                if (ptr != null)
                                    *ptr = (sbyte*)str;
                                return 0;
                            }
                            ++str;
                            @base = 16;
                        }
                        else if (*str == 0x4f || *str == 0x6f)    // O, o
                        {
                            /* there must be at least one digit after 0o */
                            if (__digit_value[__char_mask(str[1])] >= 8)
                            {
                                if (ptr != null)
                                    *ptr = (sbyte*)str;
                                return 0;
                            }
                            ++str;
                            @base = 8;
                        }
                        else if (*str == 0x42 || *str == 0x62)    // B, b
                        {
                            /* there must be at least one digit after 0b */
                            if (__digit_value[__char_mask(str[1])] >= 2)
                            {
                                if (ptr != null)
                                    *ptr = (sbyte*)str;
                                return 0;
                            }
                            ++str;
                            @base = 2;
                        }
                        else
                        {
                            /* there must be at least one digit after 0 */
                            if (__digit_value[__char_mask(str[0])] >= 8)
                            {
                                if (ptr != null)
                                    *ptr = (sbyte*)str;
                                return 0;
                            }
                            @base = 8;
                        }
                    }
                    else
                        @base = 10;
                    break;

                /* even with explicit base, skip leading 0? prefix */
                case 16:
                    if (*str == 0x30)   // 0
                    {
                        ++str;
                        if (*str == 0x58 || *str == 0x78)    // X, x
                        {
                            /* there must be at least one digit after 0x */
                            if (__digit_value[__char_mask(str[1])] >= 16)
                            {
                                if (ptr != null)
                                    *ptr = (sbyte*)str;
                                return 0;
                            }
                            ++str;
                        }
                    }
                    break;
                case 8:
                    if (*str == 0x30)
                    {
                        ++str;
                        if (*str == 0x4f || *str == 0x6f)    // O, o
                        {
                            /* there must be at least one digit after 0o */
                            if (__digit_value[__char_mask(str[1])] >= 8)
                            {
                                if (ptr != null)
                                    *ptr = (sbyte*)str;
                                return 0;
                            }
                            ++str;
                        }
                    }
                    break;
                case 2:
                    if (*str == 0x30)    // 0
                    {
                        ++str;
                        if (*str == 0x42 || *str == 0x62)    // B, b
                        {
                            /* there must be at least one digit after 0b */
                            if (__digit_value[__char_mask(str[1])] >= 2)
                            {
                                if (ptr != null)
                                    *ptr = (sbyte*)str;
                                return 0;
                            }
                            ++str;
                        }
                    }
                    break;
            }

            /* catch silly bases */
            if (@base < 2 || @base > 36)
            {
                if (ptr != null)
                    *ptr = (sbyte*)str;
                return 0;
            }

            /* skip leading zeroes */
            while (*str == '0')
                ++str;

            /* base is guaranteed to be in [2, 36] at this point */
            ovlimit = __digit_limit[@base];

            /* do the conversion until non-digit character encountered */
            while ((c = __digit_value[__char_mask(*str)]) < @base)
            {
                if (ovlimit > 0) /* no overflow check required */
                    result = result * (ulong)@base + (ulong)c;
                else
                { /* requires overflow check */
                    ulong temp_result;

                    if (ovlimit < 0) /* guaranteed overflow */
                        goto overflowed;

                    /* there could be an overflow */
                    /* check overflow just from shifting */
                    if (result > __small_max[@base])
                        goto overflowed;

                    result *= (ulong)@base;

                    /* check overflow from the digit's value */
                    temp_result = result + (ulong)c;
                    if (temp_result < result)
                        goto overflowed;

                    result = temp_result;
                }

                ++str;
                --ovlimit;
            }

            /* set pointer to point to the last character scanned */
            if (ptr != null)
                *ptr = (sbyte*)str;

            return result;

        overflowed:
            if (ptr != null)
            {
                /* spool through remaining digit characters */
                while (__digit_value[__char_mask(*str)] < @base)
                    ++str;
                *ptr = (sbyte*)str;
            }
            errno = data.ERANGE;
            return unchecked((ulong)-1);
        }

        ///////////////////////////////////////////////////////////////////////

        // This implementation is simple and the results contain minor errors.
        // It is only used for chibicc's stage2 bootstrap.

        //<number>   ::= (<sign>? <integer> | <integer> '.' <fraction>) <exponent>?
        //<exponent> ::= ('e' | 'E') <sign>? <integer>
        //<integer>  ::= <digit> | <digit> <integer>
        //<fraction> ::= <digit> | <digit> <fraction>
        //<sign>     ::= '+' | '-'
        //<digit>    ::= '0' | '1' | '2' | '3' | '4' | '5' | '6' | '7' | '8' | '9'

        private static unsafe double exponent(sbyte* nptr, sbyte** endptr)
        {
            var result = 1.0;
            if (*nptr == 0x45 || *nptr == 0x65)   // E, e
            {
                nptr++;
                var s = sign(nptr, endptr);
                nptr = *endptr;
                var exp = integer(nptr, endptr);
                nptr = *endptr;
                result = Math.Pow(10, exp * s);
            }
            *endptr = nptr;
            return result;
        }

        private static unsafe int sign(sbyte* nptr, sbyte** endptr)
        {
            var sign = 1;
            if (*nptr == 0x2d)   // -
            {
                sign = -1;
                nptr++;
            }
            else if (*nptr == 0x2b)   // +
            {
                nptr++;
            }
            *endptr = nptr;
            return sign;
        }

        private static unsafe int digit(sbyte* nptr, sbyte** endptr)
        {
            if (*nptr >= 0x30 || *nptr <= 0x39)   // 0, 9
            {
                *endptr = nptr + 1;
                return *nptr - 0x30;   // 0
            }
            *endptr = nptr;
            return 0;
        }

        private static unsafe double integer(sbyte* nptr, sbyte** endptr)
        {
            var result = 0.0;
            if (isdigit(*nptr) != 0)
            {
                result = strtoul(nptr, endptr, 10);
                nptr = *endptr;
            }
            *endptr = nptr;
            return result;
        }

        private static unsafe double fraction(sbyte* nptr, sbyte** endptr)
        {
            var result = 0.0;
            if (isdigit(*nptr) != 0)
            {
                var f = (double)text.strtoul(nptr, endptr, 10);
                var len = *endptr - nptr;
                result = f / Math.Pow(10, len);
                nptr = *endptr;
            }
            *endptr = nptr;
            return result;
        }

        private static unsafe double number(sbyte* nptr, sbyte** endptr)
        {
            var result = 0.0;
            var s = sign(nptr, endptr);
            nptr = *endptr;

            if (isdigit(*nptr) != 0 || *nptr == '.')
            {
                result = integer(nptr, endptr);
                nptr = *endptr;

                if (*nptr == '.')
                {
                    nptr++;
                    result += fraction(nptr, endptr);
                    nptr = *endptr;
                }
            }
            result *= s;
            *endptr = nptr;
            result *= exponent(nptr, endptr);
            return result;
        }

        public static unsafe double strtod(sbyte* nptr, sbyte** endptr)
        {
            errno = 0;
            return number(nptr, endptr);
        }
    }
}
