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

namespace C
{
    namespace type
    {
        public struct timespec
        {
            public long tv_sec;
            public long tv_nsec;
        }
        
        public struct stat
        {
            public long st_size;
            public timespec st_atim;
            public timespec st_mtim;
        }
    }
    
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
        public static unsafe int stat(sbyte* pathname, type.stat* statbuf)
        {
            try
            {
                var pn = __ngetstr(pathname);
                var file = new FileInfo(pn!);
                if (file.Exists)
                {
                    memset(statbuf, 0, (nuint)sizeof(type.stat));
                    statbuf->st_size = file.Length;
                    __to_timespec(file.LastAccessTime, &statbuf->st_atim);
                    __to_timespec(file.LastWriteTime, &statbuf->st_mtim);
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
}
