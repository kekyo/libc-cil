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

public sealed class strtod_test
{
    private unsafe double strtod(string str, out int endindex)
    {
        __obj_holder p = str;

        sbyte* endptr;
        var result = text.strtod(p, &endptr);

        endindex = (int)(endptr - p.c_str());
        return result;
    }

    [Test]
    public void decimal_()
    {
        var actual = strtod("1", out var endindex);

        Assert.AreEqual(1.0, actual);
        Assert.AreEqual(1, endindex);
    }

    [Test]
    public void decimal_not()
    {
        var actual = strtod("12a", out var endindex);

        Assert.AreEqual(12.0, actual);
        Assert.AreEqual(2, endindex);
    }

    [Test]
    public void decimal_multiple3()
    {
        var actual = strtod("123", out var endindex);

        Assert.AreEqual(123.0, actual);
        Assert.AreEqual(3, endindex);
    }

    [Test]
    public void float_1()
    {
        var actual = strtod("1.23", out var endindex);

        Assert.AreEqual(1.23, actual);
        Assert.AreEqual(4, endindex);
    }

    [Test]
    public void float_2()
    {
        var actual = strtod(".23", out var endindex);

        Assert.AreEqual(0.23, actual);
        Assert.AreEqual(3, endindex);
    }

    [Test]
    public void float_3()
    {
        var actual = strtod("1.", out var endindex);

        Assert.AreEqual(1.0, actual);
        Assert.AreEqual(2, endindex);
    }

    [Test]
    public void float_exp1()
    {
        var actual = strtod("1.23e+4", out var endindex);

        Assert.AreEqual(1.23e+4, actual);
        Assert.AreEqual(7, endindex);
    }

    [Test]
    public void float_exp2()
    {
        var actual = strtod("1.23E-4", out var endindex);

        Assert.AreEqual(1.23e-4, actual);
        Assert.AreEqual(7, endindex);
    }

    [Test]
    public void float_exp3()
    {
        var actual = strtod("-1.23e4", out var endindex);

        Assert.AreEqual(-1.23e4, actual);
        Assert.AreEqual(7, endindex);
    }

    [Test]
    public void float_exp4()
    {
        var actual = strtod("5E5", out var endindex);

        Assert.AreEqual(5e5, actual);
        Assert.AreEqual(3, endindex);
    }
}
