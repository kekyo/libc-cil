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

public sealed class libc_test
{
    [Test]
    public unsafe void unmanaged_pointer_transition()
    {
        var url = new Uri("https://github.com/kekyo/libc-cil");
        var p = text.__alloc_obj(url);

        try
        {
            var r = text.__get_obj(p);
            Assert.AreSame(url, r);
        }
        finally
        {
            text.__release_obj(p);
        }
    }

    [Test]
    public unsafe void unmanaged_string_operation()
    {
        __obj_holder p = text.__nstrdup("ABC123");
        var str = text.__ngetstr(p);

        Assert.AreEqual("ABC123", str);
    }
}
