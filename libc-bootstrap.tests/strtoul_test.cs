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

namespace C;

public sealed class strtoul_test
{
    private unsafe ulong strtoul(string str, out int endindex, int @base)
    {
        __obj_holder p = str;

        sbyte* endptr;
        var result = text.strtoul(p, &endptr, @base);

        endindex = (int)(endptr - p.c_str());
        return result;
    }

    [Test]
    public void base10()
    {
        var actual = strtoul("12345", out var endindex, 10);

        Assert.AreEqual(12345UL, actual);
        Assert.AreEqual(5, endindex);
    }

    [Test]
    public void base16()
    {
        var actual = strtoul("0x12af", out var endindex, 16);

        Assert.AreEqual(4783UL, actual);
        Assert.AreEqual(6, endindex);
    }

    [Test]
    public void base8()
    {
        var actual = strtoul("0123", out var endindex, 8);

        Assert.AreEqual(83UL, actual);
        Assert.AreEqual(4, endindex);
    }

    [Test]
    public void base2()
    {
        var actual = strtoul("0b1101", out var endindex, 2);

        Assert.AreEqual(13UL, actual);
        Assert.AreEqual(6, endindex);
    }

    [Test]
    public void base0_10()
    {
        var actual = strtoul("12345", out var endindex, 0);

        Assert.AreEqual(12345UL, actual);
        Assert.AreEqual(5, endindex);
    }

    [Test]
    public void base0_16()
    {
        var actual = strtoul("0x12af", out var endindex, 0);

        Assert.AreEqual(4783UL, actual);
        Assert.AreEqual(6, endindex);
    }

    [Test]
    public void base0_8()
    {
        var actual = strtoul("0123", out var endindex, 0);

        Assert.AreEqual(83UL, actual);
        Assert.AreEqual(4, endindex);
    }

    [Test]
    public void base0_2()
    {
        var actual = strtoul("0b1101", out var endindex, 0);

        Assert.AreEqual(13UL, actual);
        Assert.AreEqual(6, endindex);
    }
}
