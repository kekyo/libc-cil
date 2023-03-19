/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using C.type;
using NUnit.Framework;
using System;

namespace C;

public sealed class string_test
{
    [Test]
    public unsafe void memcpy()
    {
        var arr = new __array_holder<byte>(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        var actual = new __array_holder<byte>(11);

        text.memcpy(actual, arr, 10);

        for (var index = 0; index < 10; index++)
        {
            Assert.AreEqual(arr[index], actual[index]);
        }
        Assert.AreEqual((byte)0, actual[10]);
    }

    [Test]
    public unsafe void memcmp1()
    {
        var arr1 = new __array_holder<byte>(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
        var arr2 = new __array_holder<byte>(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);

        var actual = text.memcmp(arr1, arr2, 10);

        Assert.AreEqual(0, actual);
    }

    [Test]
    public unsafe void memcmp2()
    {
        var arr1 = new __array_holder<byte>(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);
        var arr2 = new __array_holder<byte>(0, 1, 2, 3, 4, 10, 6, 7, 8, 9);

        var actual = text.memcmp(arr1, arr2, 10);

        Assert.AreEqual(-5, actual);
    }

    [Test]
    public unsafe void memcmp3()
    {
        var arr1 = new __array_holder<byte>(0, 1, 2, 3, 4, 10, 6, 7, 8, 9);
        var arr2 = new __array_holder<byte>(0, 1, 2, 3, 4, 5, 6, 7, 8, 9);

        var actual = text.memcmp(arr1, arr2, 10);

        Assert.AreEqual(5, actual);
    }

    [Test]
    public unsafe void memset()
    {
        var arr1 = new __array_holder<byte>(4, 4, 4, 4, 4, 4, 4, 4, 4, 4);
        var arr2 = new __array_holder<byte>(10);

        var r = text.memset(arr2, 4, 10);

        var actual = text.memcmp(arr1, arr2, 10);

        Assert.AreEqual(0, actual);
        Assert.AreEqual((nint)(void*)arr2, (nint)r);
    }

    [Test]
    public unsafe void strcmp1()
    {
        var str1 = new __obj_holder("ABC");
        var str2 = new __obj_holder("ABC");

        var actual = text.strcmp(str1, str2);

        Assert.AreEqual(0, actual);
    }

    [Test]
    public unsafe void strcmp2()
    {
        var str1 = new __obj_holder("ABC");
        var str2 = new __obj_holder("AXC");

        var actual = text.strcmp(str1, str2);

        Assert.AreEqual(-22, actual);
    }

    [Test]
    public unsafe void strcmp3()
    {
        var str1 = new __obj_holder("AXC");
        var str2 = new __obj_holder("ABC");

        var actual = text.strcmp(str1, str2);

        Assert.AreEqual(22, actual);
    }

    [Test]
    public unsafe void strcmp4()
    {
        var str1 = new __obj_holder("ABCD");
        var str2 = new __obj_holder("ABC");

        var actual = text.strcmp(str1, str2);

        Assert.AreEqual(0x44, actual);
    }

    [Test]
    public unsafe void strcmp5()
    {
        var str1 = new __obj_holder("ABC");
        var str2 = new __obj_holder("ABCD");

        var actual = text.strcmp(str1, str2);

        Assert.AreEqual(-0x44, actual);
    }

    [Test]
    public unsafe void strncmp1()
    {
        var str1 = new __obj_holder("ABC");
        var str2 = new __obj_holder("ABC");

        var actual = text.strncmp(str1, str2, 3);

        Assert.AreEqual(0, actual);
    }

    [Test]
    public unsafe void strncmp2()
    {
        var str1 = new __obj_holder("ABC");
        var str2 = new __obj_holder("AXC");

        var actual = text.strncmp(str1, str2, 3);

        Assert.AreEqual(-22, actual);
    }

    [Test]
    public unsafe void strncmp3()
    {
        var str1 = new __obj_holder("AXC");
        var str2 = new __obj_holder("ABC");

        var actual = text.strncmp(str1, str2, 3);

        Assert.AreEqual(22, actual);
    }

    [Test]
    public unsafe void strncmp4()
    {
        var str1 = new __obj_holder("ABCD");
        var str2 = new __obj_holder("ABC");

        var actual = text.strncmp(str1, str2, 4);

        Assert.AreEqual(0x44, actual);
    }

    [Test]
    public unsafe void strncmp5()
    {
        var str1 = new __obj_holder("ABC");
        var str2 = new __obj_holder("ABCD");

        var actual = text.strncmp(str1, str2, 4);

        Assert.AreEqual(-0x44, actual);
    }

    [Test]
    public unsafe void strncmp6()
    {
        var str1 = new __obj_holder("ABCD");
        var str2 = new __obj_holder("ABC");

        var actual = text.strncmp(str1, str2, 3);

        Assert.AreEqual(0, actual);
    }

    [Test]
    public unsafe void strncmp7()
    {
        var str1 = new __obj_holder("ABC");
        var str2 = new __obj_holder("ABCD");

        var actual = text.strncmp(str1, str2, 3);

        Assert.AreEqual(0, actual);
    }

    [Test]
    public unsafe void strlen()
    {
        var str = new __obj_holder("ABC");

        var actual = text.strlen(str);

        Assert.AreEqual((nuint)3, actual);
    }

    [Test]
    public unsafe void strndup()
    {
        var str = new __obj_holder("ABCDE");

        __obj_holder actual = text.strndup(str, 3);

        Assert.AreEqual("ABC", text.__ngetstr(actual));
    }

    [Test]
    public unsafe void strchr1()
    {
        var str = new __obj_holder("ABCDE");

        var p = text.strchr(str, 'C');

        Assert.AreEqual(((nint)(void*)str) + 2, (nint)p);
    }

    [Test]
    public unsafe void strchr2()
    {
        var str = new __obj_holder("ABCDE");

        var p = text.strchr(str, 'X');

        Assert.AreEqual((nint)0, (nint)p);
    }

    [Test]
    public unsafe void strstr1()
    {
        var str = new __obj_holder("ABCDE");
        var a = new __obj_holder("CD");

        var p = text.strstr(str, a);

        Assert.AreEqual(((nint)(void*)str) + 2, (nint)p);
    }

    [Test]
    public unsafe void strstr2()
    {
        var str = new __obj_holder("ABCDE");
        var a = new __obj_holder("AB");

        var p = text.strstr(str, a);

        Assert.AreEqual(((nint)(void*)str), (nint)p);
    }

    [Test]
    public unsafe void strstr3()
    {
        var str = new __obj_holder("ABCDE");
        var a = new __obj_holder("DE");

        var p = text.strstr(str, a);

        Assert.AreEqual(((nint)(void*)str) + 3, (nint)p);
    }

    [Test]
    public unsafe void strstr4()
    {
        var str = new __obj_holder("ABCDE");
        var a = new __obj_holder("ABCDE");

        var p = text.strstr(str, a);

        Assert.AreEqual(((nint)(void*)str), (nint)p);
    }

    [Test]
    public unsafe void strstr5()
    {
        var str = new __obj_holder("ABCDE");
        var a = new __obj_holder("ABCDEF");

        var p = text.strstr(str, a);

        Assert.AreEqual((nint)0, (nint)p);
    }

    [Test]
    public unsafe void strstr6()
    {
        var str = new __obj_holder("ABCDE");
        var a = new __obj_holder("XY");

        var p = text.strstr(str, a);

        Assert.AreEqual((nint)0, (nint)p);
    }

    [Test]
    public unsafe void strcasecmp1()
    {
        var str1 = new __obj_holder("ABC");
        var str2 = new __obj_holder("ABC");

        var actual = text.strcasecmp(str1, str2);

        Assert.AreEqual(0, actual);
    }

    [Test]
    public unsafe void strcasecmp2()
    {
        var str1 = new __obj_holder("ABC");
        var str2 = new __obj_holder("AXC");

        var actual = text.strcasecmp(str1, str2);

        Assert.AreEqual(-22, actual);
    }

    [Test]
    public unsafe void strcasecmp3()
    {
        var str1 = new __obj_holder("AXC");
        var str2 = new __obj_holder("ABC");

        var actual = text.strcasecmp(str1, str2);

        Assert.AreEqual(22, actual);
    }

    [Test]
    public unsafe void strcasecmp4()
    {
        var str1 = new __obj_holder("ABCD");
        var str2 = new __obj_holder("ABC");

        var actual = text.strcasecmp(str1, str2);

        Assert.AreEqual(0x64, actual);
    }

    [Test]
    public unsafe void strcasecmp5()
    {
        var str1 = new __obj_holder("ABC");
        var str2 = new __obj_holder("ABCD");

        var actual = text.strcasecmp(str1, str2);

        Assert.AreEqual(-0x64, actual);
    }

    [Test]
    public unsafe void strcasecmp6()
    {
        var str1 = new __obj_holder("ABC");
        var str2 = new __obj_holder("abc");

        var actual = text.strcasecmp(str1, str2);

        Assert.AreEqual(0, actual);
    }

    [Test]
    public unsafe void strcasecmp7()
    {
        var str1 = new __obj_holder("AbC");
        var str2 = new __obj_holder("aBc");

        var actual = text.strcasecmp(str1, str2);

        Assert.AreEqual(0, actual);
    }

    [Test]
    public unsafe void strncasecmp1()
    {
        var str1 = new __obj_holder("ABC");
        var str2 = new __obj_holder("ABC");

        var actual = text.strncasecmp(str1, str2, 3);

        Assert.AreEqual(0, actual);
    }

    [Test]
    public unsafe void strncasecmp2()
    {
        var str1 = new __obj_holder("ABC");
        var str2 = new __obj_holder("AXC");

        var actual = text.strncasecmp(str1, str2, 3);

        Assert.AreEqual(-22, actual);
    }

    [Test]
    public unsafe void strncasecmp3()
    {
        var str1 = new __obj_holder("AXC");
        var str2 = new __obj_holder("ABC");

        var actual = text.strncasecmp(str1, str2, 3);

        Assert.AreEqual(22, actual);
    }

    [Test]
    public unsafe void strncasecmp4()
    {
        var str1 = new __obj_holder("ABCD");
        var str2 = new __obj_holder("ABC");

        var actual = text.strncasecmp(str1, str2, 4);

        Assert.AreEqual(0x64, actual);
    }

    [Test]
    public unsafe void strncasecmp5()
    {
        var str1 = new __obj_holder("ABC");
        var str2 = new __obj_holder("ABCD");

        var actual = text.strncasecmp(str1, str2, 4);

        Assert.AreEqual(-0x64, actual);
    }

    [Test]
    public unsafe void strncasecmp6()
    {
        var str1 = new __obj_holder("ABCD");
        var str2 = new __obj_holder("ABC");

        var actual = text.strncasecmp(str1, str2, 3);

        Assert.AreEqual(0, actual);
    }

    [Test]
    public unsafe void strncasecmp7()
    {
        var str1 = new __obj_holder("ABC");
        var str2 = new __obj_holder("ABCD");

        var actual = text.strncasecmp(str1, str2, 3);

        Assert.AreEqual(0, actual);
    }

    [Test]
    public unsafe void strncasecmp8()
    {
        var str1 = new __obj_holder("ABC");
        var str2 = new __obj_holder("abc");

        var actual = text.strncasecmp(str1, str2, 3);

        Assert.AreEqual(0, actual);
    }

    [Test]
    public unsafe void strncasecmp9()
    {
        var str1 = new __obj_holder("AbC");
        var str2 = new __obj_holder("aBc");

        var actual = text.strncasecmp(str1, str2, 3);

        Assert.AreEqual(0, actual);
    }
}
