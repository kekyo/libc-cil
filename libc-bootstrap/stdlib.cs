/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;

namespace C;

public static partial class text
{
    private static readonly char[] __gettemp_ch =
    {
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
    };

    // char *realpath(const char *path, char *resolved_path);
    public static unsafe sbyte* realpath(sbyte* path, sbyte* resolved_path)
    {
        if (resolved_path != null)
        {
            errno = data.ENAMETOOLONG;
            return null;
        }

        var p = __ngetstr(path);
        if (p == null)
        {
            errno = data.EINVAL;
            return null;
        }

        if (p == "-")
        {
            errno = data.ENOENT;
            return null;
        }

        try
        {
            var fullPath = Path.GetFullPath(p);
            return __nstrdup(fullPath);
        }
        catch (Exception ex)
        {
            __set_exception_to_errno(ex);
            return null;
        }
    }


    private static string __gettemp_postfix()
    {
        // "0-9a-zA-Z": 10 + 26 + 26 = 62
        var va = new byte[6];
        new Random().NextBytes(va);
        var sb = new StringBuilder();
        foreach (var v in va)
        {
            sb.Append(__gettemp_ch[v % 62]);
        }
        return sb.ToString();
    }
    
    // int mkstemp(char *template);
    public static unsafe int mkstemp(sbyte* template)
    {
        try
        {
            var tempPath = __ngetstr(template)!;
            if (!tempPath.EndsWith("XXXXXX"))
            {
                errno = data.EINVAL;
                return -1;
            }

            var path = tempPath.Substring(0, tempPath.Length - 6);

            var count = 0;
            while (true)
            {
                var postfix = __gettemp_postfix();
                try
                {
                    var fd = fileio.create(path + postfix);
                    var position = strlen(template) - (nuint)postfix.Length;
                    for (var index = 0; index < postfix.Length; index++)
                    {
                        template[position + (nuint)index] = (sbyte)postfix[index];
                    }
                    return fd;
                }
                catch (Exception ex)
                {
                    if (count >= 10)
                    {
                        __set_exception_to_errno(ex);
                        return -1;
                    }
                    count++;
                }
            }
        }
        catch (Exception ex)
        {
            __set_exception_to_errno(ex);
            return -1;
        }
    }
}
