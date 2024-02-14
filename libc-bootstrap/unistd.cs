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

namespace C;

public static partial class text
{
    // int unlink(char *pathname);
    public static unsafe int unlink(sbyte* pathname)
    {
        try
        {
            var pn = __ngetstr(pathname)!;
            File.Delete(pn);
        }
        catch (Exception ex)
        {
            __set_exception_to_errno(ex);
            return -1;
        }
        return 0;
    }

    // int close(int fd);
    public static int close(int fd)
    {
        try
        {
            if (!fileio.close(fd))
            {
                errno = data.EBADF;
                return -1;
            }
            return 0;
        }
        catch (Exception ex)
        {
            __set_exception_to_errno(ex);
            return -1;
        }
    }
    
    // int stat(char *pathname, struct stat *statbuf);
    public static unsafe int stat(sbyte* pathname, void* statbuf)
    {
        try
        {
            // HACK: Only checking exists a file.
            var pn = __ngetstr(pathname);
            if (File.Exists(pn))
            {
                return 0;
            }
            else
            {
                errno = data.ENOENT;
                return -1;
            }
        }
        catch (Exception ex)
        {
            __set_exception_to_errno(ex);
            return -1;
        }
    }
}
