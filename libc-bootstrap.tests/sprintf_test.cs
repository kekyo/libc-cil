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
using System.Text;

namespace C;

public sealed class sprintf_test
{
    private unsafe string sprintf(string fmt, params object[] args)
    {
        __array_holder<sbyte> buf = new(100);
        __obj_holder fmt_ = fmt;
        text.sprintf(buf, fmt_, new(args));
        var len = text.strlen(buf);
        return Encoding.UTF8.GetString((byte*)buf.get(), (int)len);
    }

    ////////////////////////////////////////////////////////////////////////////

    [Test]
    public void digit()
    {
        var actual = sprintf("%d", 42);
        Assert.AreEqual("42", actual);
    }

    [Test]
    public unsafe void @string()
    {
        __obj_holder str = "hello";
        var actual = sprintf("%s", (nint)str.get());
        Assert.AreEqual("hello", actual);
    }

    [Test]
    public unsafe void @char()
    {
        var actual = sprintf("%c", (sbyte)'a');
        Assert.AreEqual("a", actual);
    }

    [Test]
    public unsafe void float1()
    {
        var actual = sprintf("%f", 3.1415926f);
        Assert.AreEqual("3.141593", actual);
    }

    [Test]
    public unsafe void float2()
    {
        var actual = sprintf("%.2f", 3.1415926f);
        Assert.AreEqual("3.14", actual);
    }

    [Test]
    public unsafe void float3()
    {
        var actual = sprintf("%f", 3.1415926d);
        Assert.AreEqual("3.141593", actual);
    }

    [Test]
    public unsafe void float4()
    {
        var actual = sprintf("%.2f", 3.1415926d);
        Assert.AreEqual("3.14", actual);
    }

    [Test]
    public unsafe void decimal1()
    {
        var actual = sprintf("%5d", 42);
        Assert.AreEqual("   42", actual);
    }

    [Test]
    public unsafe void decimal2()
    {
        var actual = sprintf("%-5d", 42);
        Assert.AreEqual("42   ", actual);
    }

    [Test]
    public unsafe void decimal3()
    {
        var actual = sprintf("%05d", 42);
        Assert.AreEqual("00042", actual);
    }

    [Test]
    public unsafe void hex1()
    {
        var actual = sprintf("%x", 255);
        Assert.AreEqual("ff", actual);
    }

    [Test]
    public unsafe void hex2()
    {
        var actual = sprintf("%X", 255);
        Assert.AreEqual("FF", actual);
    }

    [Test]
    public unsafe void pointer()
    {
        var actual = sprintf("%p", (nint)0x123456);
        Assert.AreEqual("0x123456", actual);
    }

    [Test]
    public unsafe void combined2()
    {
        __obj_holder str = "ABC";
        var actual = sprintf("%d, %s", 42, (nint)str.get());
        Assert.AreEqual("42, ABC", actual);
    }

    [Test]
    public unsafe void combined3()
    {
        __obj_holder str = "ABC";
        var actual = sprintf("%d, %s, %f", 42, (nint)str.get(), 123.456);
        Assert.AreEqual("42, ABC, 123.456", actual);
    }
}
