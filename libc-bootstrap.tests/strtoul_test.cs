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

[Parallelizable(ParallelScope.All)]
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
    public void base10_post_unknown()
    {
        var actual = strtoul("12345x", out var endindex, 10);

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
    public void base16_post_unknown()
    {
        var actual = strtoul("0x12afx", out var endindex, 16);

        Assert.AreEqual(4783UL, actual);
        Assert.AreEqual(6, endindex);
    }

    [Test]
    public void base16_non_prefix()
    {
        var actual = strtoul("12af", out var endindex, 16);

        Assert.AreEqual(4783UL, actual);
        Assert.AreEqual(4, endindex);
    }

    [Test]
    public void base16_non_prefix_post_unknown()
    {
        var actual = strtoul("12afx", out var endindex, 16);

        Assert.AreEqual(4783UL, actual);
        Assert.AreEqual(4, endindex);
    }

    [Test]
    public void base16_large()
    {
        var actual = strtoul("0xfffffffffffffff2", out var endindex, 16);

        Assert.AreEqual(0xfffffffffffffff2UL, actual);
        Assert.AreEqual(18, endindex);
    }

    [Test]
    public void base16_large_post_unknown()
    {
        var actual = strtoul("0xfffffffffffffff2x", out var endindex, 16);

        Assert.AreEqual(0xfffffffffffffff2UL, actual);
        Assert.AreEqual(18, endindex);
    }

    [Test]
    public void base16_non_prefix_large()
    {
        var actual = strtoul("fffffffffffffff2", out var endindex, 16);

        Assert.AreEqual(0xfffffffffffffff2UL, actual);
        Assert.AreEqual(16, endindex);
    }

    [Test]
    public void base16_non_prefix_large_post_unknown()
    {
        var actual = strtoul("fffffffffffffff2x", out var endindex, 16);

        Assert.AreEqual(0xfffffffffffffff2UL, actual);
        Assert.AreEqual(16, endindex);
    }

    [Test]
    public void base8()
    {
        var actual = strtoul("0123", out var endindex, 8);

        Assert.AreEqual(83UL, actual);
        Assert.AreEqual(4, endindex);
    }

    [Test]
    public void base8_post_unknown()
    {
        var actual = strtoul("0123x", out var endindex, 8);

        Assert.AreEqual(83UL, actual);
        Assert.AreEqual(4, endindex);
    }

    [Test]
    public void base8_non_prefix()
    {
        var actual = strtoul("123", out var endindex, 8);

        Assert.AreEqual(83UL, actual);
        Assert.AreEqual(3, endindex);
    }

    [Test]
    public void base8_non_prefix_post_unknown()
    {
        var actual = strtoul("123x", out var endindex, 8);

        Assert.AreEqual(83UL, actual);
        Assert.AreEqual(3, endindex);
    }

    [Test]
    public void base2()
    {
        var actual = strtoul("0b1101", out var endindex, 2);

        Assert.AreEqual(13UL, actual);
        Assert.AreEqual(6, endindex);
    }

    [Test]
    public void base2_post_unknown()
    {
        var actual = strtoul("0b1101x", out var endindex, 2);

        Assert.AreEqual(13UL, actual);
        Assert.AreEqual(6, endindex);
    }

    [Test]
    public void base2_non_prefix()
    {
        var actual = strtoul("1101", out var endindex, 2);

        Assert.AreEqual(13UL, actual);
        Assert.AreEqual(4, endindex);
    }

    [Test]
    public void base2_non_prefix_post_unknown()
    {
        var actual = strtoul("1101x", out var endindex, 2);

        Assert.AreEqual(13UL, actual);
        Assert.AreEqual(4, endindex);
    }

    [Test]
    public void base0_10()
    {
        var actual = strtoul("12345", out var endindex, 0);

        Assert.AreEqual(12345UL, actual);
        Assert.AreEqual(5, endindex);
    }

    [Test]
    public void base0_10_post_unknown()
    {
        var actual = strtoul("12345x", out var endindex, 0);

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
    public void base0_16_post_unknown()
    {
        var actual = strtoul("0x12afx", out var endindex, 0);

        Assert.AreEqual(4783UL, actual);
        Assert.AreEqual(6, endindex);
    }

    [Test]
    public void base0_16_large()
    {
        var actual = strtoul("0xfffffffffffffff2", out var endindex, 0);

        Assert.AreEqual(0xfffffffffffffff2UL, actual);
        Assert.AreEqual(18, endindex);
    }

    [Test]
    public void base0_16_large_post_unknown()
    {
        var actual = strtoul("0xfffffffffffffff2x", out var endindex, 0);

        Assert.AreEqual(0xfffffffffffffff2UL, actual);
        Assert.AreEqual(18, endindex);
    }

    [Test]
    public void base0_8()
    {
        var actual = strtoul("0123", out var endindex, 0);

        Assert.AreEqual(83UL, actual);
        Assert.AreEqual(4, endindex);
    }

    [Test]
    public void base0_8_post_unknown()
    {
        var actual = strtoul("0123x", out var endindex, 0);

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

    [Test]
    public void base0_2_post_unknown()
    {
        var actual = strtoul("0b1101x", out var endindex, 0);

        Assert.AreEqual(13UL, actual);
        Assert.AreEqual(6, endindex);
    }
}
