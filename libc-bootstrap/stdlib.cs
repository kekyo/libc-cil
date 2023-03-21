/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace C;

public static partial class text
{
    // We use HGlobal family for allocator instead of COM allocator.
    // Because they can receive size parameter by intptr type.

    // void *malloc(size_t size);
    public static unsafe void* malloc(nuint size) =>
        (void*)Marshal.AllocHGlobal((nint)size);

    // void *calloc(size_t nmemb, size_t size);
    public static unsafe void* calloc(nuint nmemb, nuint size)
    {
        var s = nmemb * size;
        var p = (void*)Marshal.AllocHGlobal((nint)s);
        // AllocHGlobal is returned uninitialized memory.
        memset(p, 0, s);
        return p;
    }

    // void free(void *p);
    public static unsafe void free(void* p) =>
        Marshal.FreeHGlobal((nint)p);

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
         * Note that this is pessimistic if sizeof(long) > 4.
         */
        private static unsafe readonly int[] __digit_limit = sizeof(void*) == 4 ?
            new int[] {
                0,  0, 32, 20, 16, 13, 12, 11, 10, 10,    /*  0 -  9 */
                9,  9,  8,  8,  8,  8,  8,  7,  7,  7,    /* 10 - 19 */
                7,  7,  7,  7,  6,  6,  6,  6,  6,  6,    /* 20 - 29 */
                6,  6,  6,  6,  6,  6,  6} :              /* 30 - 36 */
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

            if (isdigit(*nptr) != 0)
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
