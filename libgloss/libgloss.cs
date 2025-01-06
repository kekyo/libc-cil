/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

#if LIBGLOSS

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace C;

public static partial class data
{
    public const int O_RDONLY = 0x00;
    public const int O_WRONLY = 0x01;
    public const int O_RDWR = 0x02;
    public const int O_APPEND = 0x08;
    public const int O_CREAT = 0x200;
    public const int O_TRUNC = 0x400;
    public const int O_EXCL = 0x800;

    public const int S_IFREG = 0x8000;
    public const int S_IFDIR = 0x4000;
    public const int S_IFCHR = 0x2000;
    public const int S_IRUSR = 0x100;
    public const int S_IWUSR = 0x80;
    public const int S_IXUSR = 0x40;
    public const int S_IRWXU = 0x1c0;
    public const int S_IRGRP = 0x20;
    public const int S_IWGRP = 0x10;
    public const int S_IXGRP = 0x08;
    public const int S_IRWXG = 0x38;
    public const int S_IROTH = 0x04;
    public const int S_IWOTH = 0x02;
    public const int S_IXOTH = 0x01;
    public const int S_IRWXO = 0x07;

    public const int SEEK_SET = 0;
    public const int SEEK_CUR = 1;
    public const int SEEK_END = 2;
}

public static partial class text
{
    public static void _exit(int status) =>
        Environment.Exit(status);

    public static unsafe int _open(sbyte* file, int flags, int mode)
    {
        try
        {
            var path = __ngetstr(file)!;
            var fd = ((mode & data.O_CREAT) != 0) ?
                fileio.force_create(path) : fileio.open(path);
            if ((mode & data.O_APPEND) != 0)
            {
                var s = fileio.get_stream(fd)!;
                s.Seek(0, SeekOrigin.End);
            }
            return fd;
        }
        catch (Exception ex)
        {
            __set_exception_to_errno(ex);
            return -1;
        }
    }

    public static nint _lseek(int fd, nint off, int whence)
    {
        try
        {
            if (fileio.get_stream(fd) is not { } s)
            {
                errno = data.EBADF;
                return -1;
            }
            switch (whence)
            {
                case data.SEEK_SET:
                    s.Seek(off, SeekOrigin.Begin);
                    break;
                case data.SEEK_CUR:
                    s.Seek(off, SeekOrigin.Current);
                    break;
                case data.SEEK_END:
                    s.Seek(off, SeekOrigin.End);
                    break;
                default:
                    errno = data.EINVAL;
                    return -1;
            }

            return (nint)s.Position;
        }
        catch (Exception ex)
        {
            __set_exception_to_errno(ex);
            return -1;
        }
    }

    public static int _close(int fd)
    {
        try
        {
            return fileio.close(fd) ? 0 : data.EBADF;
        }
        catch (Exception ex)
        {
            __set_exception_to_errno(ex);
            return -1;
        }
    }

    public static unsafe nint _write(int fd, void* buf, nint count)
    {
        try
        {
            if (fileio.get_stream(fd) is not { } s)
            {
                errno = data.EBADF;
                return -1;
            }
#if NETFRAMEWORK || NETSTANDARD2_0 || NETCOREAPP2_0
            var b = new byte[count];
            Marshal.Copy((nint)buf, b, 0, b.Length);
            s.Write(b, 0, b.Length);
#else
            var span = new ReadOnlySpan<byte>(buf, (int)count);
            s.Write(span);
#endif
        }
        catch (Exception ex)
        {
            __set_exception_to_errno(ex);
            return -1;
        }
        return count;
    }

    public static unsafe int _read(int fd, void* buf, nint count)
    {
        try
        {
            if (fileio.get_stream(fd) is not { } s)
            {
                errno = data.EBADF;
                return -1;
            }
#if NETFRAMEWORK || NETSTANDARD2_0 || NETCOREAPP2_0
            var b = new byte[count];
            var read = s.Read(b, 0, b.Length);
            Marshal.Copy(b, 0, (nint)buf, read);
#else
            var span = new Span<byte>(buf, (int)count);
            var read = s.Read(span);
#endif
            return read;
        }
        catch (Exception ex)
        {
            __set_exception_to_errno(ex);
            return -1;
        }
    }
    
    public static int _getpid() =>
        Process.GetCurrentProcess().Id;
}

#endif
