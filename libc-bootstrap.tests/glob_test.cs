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
using System.Collections.Generic;
using System.IO;

namespace C;

public sealed class glob_test
{
    [Test]
    public unsafe void glob1()
    {
        var p = Path.GetFullPath("/usr/bin/ls");            
        var po = new __obj_holder(p);

        var glob = new glob_t();
        var r = text.glob(po, 0, null, &glob);

        try
        {
            Assert.AreEqual(0, r);
            Assert.AreEqual((nuint)1, glob.gl_pathc);
            Assert.AreEqual("/usr/bin/ls", text.__ngetstr(glob.gl_pathv[0]));
        }
        finally
        {
            text.globfree(&glob);
        }
    }
    
    [Test]
    public unsafe void glob2()
    {
        var p = Path.GetFullPath("/usr/bin/*");            
        var po = new __obj_holder(p);

        var glob = new glob_t();
        var r = text.glob(po, 0, null, &glob);

        try
        {
            var files = new HashSet<string>(
                Directory.EnumerateFiles("/usr/bin", "*", SearchOption.TopDirectoryOnly));
            
            Assert.AreEqual(0, r);
            Assert.AreEqual((nuint)files.Count, glob.gl_pathc);
            for (var index = 0; index < (int)glob.gl_pathc; index++)
            {
                Assert.IsTrue(files.Contains(text.__ngetstr(glob.gl_pathv[index])!));
            }
        }
        finally
        {
            text.globfree(&glob);
        }
    }
}