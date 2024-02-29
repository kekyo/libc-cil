/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using System;

namespace C;

public static partial class text
{
    // void *memcpy(void *dst, const void *src, size_t n);
    public static unsafe void memcpy(void* dst, void* src, nuint n)
    {
        var r = n;
        var d = (byte*)dst;
        var s = (byte*)src;
        while (r > 0)
        {
            *d = *s;
            d++;
            s++;
            r--;
        }
    }

    // int memcmp(const void *s1, const void *s2, size_t n);
    public static unsafe int memcmp(void* s1, void* s2, nuint n)
    {
        var r = n;
        var s1_ = (byte*)s1;
        var s2_ = (byte*)s2;
        while (r > 0)
        {
            var num = *s1_ - *s2_;
            if (num != 0)
            {
                return num;
            }
            s1_++;
            s2_++;
            r--;
        }
        return 0;
    }

    // void *memset(void *s, int c, size_t n);
    public static unsafe void* memset(void* s, int c, nuint n)
    {
        var r = n;
        var s_ = (byte*)s;
        var c_ = (byte)c;
        while (r > 0)
        {
            *s_ = c_;
            s_++;
            r--;
        }
        return s;
    }

    ///////////////////////////////////////////////////////////////////////

    // char *strncpy(char *dst, char void *src, size_t n);
    public static unsafe sbyte* strncpy(sbyte* dst, sbyte* src, nuint n)
    {
        var r = n;
        var d = (byte*)dst;
        var s = (byte*)src;
        while (r > 0)
        {
            r--;
            *d = *s;
            if (*s == 0)
            {
                d++;
                memset(d, 0, r);
                break;
            }
            d++;
            s++;
        }
        return dst;
    }

    // int strcmp(const char *s1, const char *s2);
    public static unsafe int strcmp(sbyte* s1, sbyte* s2)
    {
        while (true)
        {
            var num = (byte)*s1 - (byte)*s2;
            if (num != 0)
            {
                return num;
            }
            if (*s1 == 0)
            {
                break;
            }
            s1++;
            s2++;
        }
        return 0;
    }

    // int strncmp(const char *s1, const char *s2, size_t n);
    public static unsafe int strncmp(sbyte* s1, sbyte* s2, nuint n)
    {
        var r = n;
        while (r > 0)
        {
            var num = (byte)*s1 - (byte)*s2;
            if (num != 0)
            {
                return num;
            }
            if (*s1 == 0)
            {
                break;
            }
            s1++;
            s2++;
            r--;
        }
        return 0;
    }

    // size_t strlen(const char *p);
    public static unsafe nuint strlen(sbyte* p)
    {
        nuint len = 0;
        while (*p != 0)
        {
            p++;
            len++;
        }
        return len;
    }

    // char *strdup(char *p);
    public static unsafe sbyte* strdup(sbyte* p)
    {
        var len = strlen(p);
        var buf = (sbyte*)malloc(len + 1);
        memcpy(buf, p, len);
        *(buf + len) = 0;
        return buf;
    }

    // char *strndup(const char *p, size_t n);
    public static unsafe sbyte* strndup(sbyte* p, nuint n)
    {
        var len = strlen(p);
        if (len > n)
        {
            len = n;
        }
        var buf = (sbyte*)malloc(len + 1);
        memcpy(buf, p, len);
        *(buf + len) = 0;
        return buf;
    }

    // char *strchr(const char *s, int c);
    public static unsafe sbyte* strchr(sbyte* s, int c)
    {
        var c_ = (sbyte)c;
        while (*s != 0)
        {
            if (*s == c_)
            {
                return s;
            }
            s++;
        }
        return null;
    }

    // char *strrchr(const char *s, int c);
    public static unsafe sbyte* strrchr(sbyte* s, int c)
    {
        var c_ = (sbyte)c;
        sbyte* ls = null;
        while (*s != 0)
        {
            if (*s == c_)
            {
                ls = s;
            }
            s++;
        }
        return ls;
    }

    // char *strstr(const char *haystack, const char *needle);
    public static unsafe sbyte* strstr(sbyte* haystack, sbyte* needle)
    {
        var needle_len = strlen(needle);
        if (needle_len == 0)
        {
            return haystack;
        }
        while (true)
        {
            haystack = strchr(haystack, needle[0]);
            if (haystack == null)
            {
                break;
            }
            if (strncmp(haystack, needle, needle_len) == 0)
            {
                return haystack;
            }
            haystack = haystack + 1;
        }
        return null;
    }

    // int strcasecmp(const char *s1, const char *s2);
    public static unsafe int strcasecmp(sbyte* s1, sbyte* s2)
    {
        while (true)
        {
            var num = (byte)tolower(*s1) - (byte)tolower(*s2);
            if (num != 0)
            {
                return num;
            }
            if (*s1 == 0)
            {
                break;
            }
            s1++;
            s2++;
        }
        return 0;
    }

    // int strncasecmp(const char *s1, const char *s2, size_t n);
    public static unsafe int strncasecmp(sbyte* s1, sbyte* s2, nuint n)
    {
        var r = n;
        while (r > 0)
        {
            var num = (byte)tolower(*s1) - (byte)tolower(*s2);
            if (num != 0)
            {
                return num;
            }
            if (*s1 == 0)
            {
                break;
            }
            s1++;
            s2++;
            r--;
        }
        return 0;
    }
}
