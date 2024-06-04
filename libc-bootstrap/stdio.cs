/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using C.type;

namespace C
{
    namespace type
    {
        [StructLayout(LayoutKind.Sequential, Size = 0, Pack = 8)]
        public readonly struct FILE
        {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public unsafe delegate void __writer(sbyte* p, nuint len);
    }

    ///////////////////////////////////////////////////////////////////////

    public static partial class data
    {
        public static unsafe readonly FILE** stdin =
            text.__alloc_and_set_field(text.to_fileptr(0));
        public static unsafe readonly FILE** stdout =
            text.__alloc_and_set_field(text.to_fileptr(1));
        public static unsafe readonly FILE** stderr =
            text.__alloc_and_set_field(text.to_fileptr(2));
    }

    ///////////////////////////////////////////////////////////////////////

    public static partial class text
    {
        internal static unsafe FILE* to_fileptr(int fd) =>
            (FILE*)(0x80000000U | fd);

        private static unsafe int to_fd(FILE* fp) =>
            (int)((nuint)fp & 0x7fffffffU);
        
        private static unsafe Stream? to_stream(FILE* fp) =>
            fileio.get_stream(to_fd(fp));
        
        // int printf(char *fmt, ...);
        public static unsafe int printf(sbyte* fmt, __va_arglist args)
        {
            var ap = va_start(args);
            var len = stdio.isprintf(out var str, fmt, ap);
            Console.Out.Write(str);
            Console.Out.Flush();
            return len;
        }

        // int sprintf(char *buf, char *fmt, ...);
        public static unsafe int sprintf(sbyte* buf, sbyte* fmt, __va_arglist args)
        {
            var ap = va_start(args);
            return vsprintf(buf, fmt, ap);
        }

        // int fprintf(FILE *fp, char *fmt, ...);
        public static unsafe int fprintf(FILE* fp, sbyte* fmt, __va_arglist args)
        {
            var ap = va_start(args);
            return vfprintf(fp, fmt, ap);
        }

        ///////////////////////////////////////////////////////////////////////

        // FILE *fopen(char *pathname, char *mode);
        public static unsafe FILE* fopen(sbyte* pathname, sbyte* mode)
        {
            try
            {
                var path = __ngetstr(pathname)!;
                var fd = __ngetstr(mode) == "w" ?
                    fileio.force_create(path) : fileio.open(path);
                return to_fileptr(fd);
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return (FILE*)0;
            }
        }

        // FILE *open_memstream(char **ptr, size_t *sizeloc);
        public static unsafe FILE* open_memstream(sbyte** ptr, nuint* sizeloc)
        {
            try
            {
                var ms = new __memory_stream(ptr, sizeloc);
                var fd = fileio.register_stream(ms);
                return to_fileptr(fd);
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return (FILE*)0;
            }
        }

        // size_t fread(void *ptr, size_t size, size_t nmemb, FILE *fp);
        public static unsafe nuint fread(void* ptr, nuint size, nuint nmemb, FILE* fp)
        {
            try
            {
                var s = to_stream(fp)!;
                var buf = new byte[size * nmemb];
                var read = s.Read(buf, 0, buf.Length);
                Marshal.Copy(buf, 0, (nint)ptr, read);
                return (nuint)read;
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return 0;
            }
        }

        // size_t fwrite(void *ptr, size_t size, size_t nmemb, FILE *fp);
        public static unsafe nuint fwrite(void* ptr, nuint size, nuint nmemb, FILE* fp)
        {
            try
            {
                var s = to_stream(fp)!;
                var buf = new byte[size * nmemb];
                Marshal.Copy((nint)ptr, buf, 0, buf.Length);
                s.Write(buf, 0, buf.Length);
                return nmemb;
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return 0;
            }
        }

        // int fflush(FILE *fp);
        public static unsafe int fflush(FILE* fp)
        {
            try
            {
                var s = to_stream(fp)!;
                s.Flush();
                return 0;
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return -1;
            }
        }

        // int fclose(FILE *fp);
        public static unsafe int fclose(FILE* fp)
        {
            try
            {
                var fd = to_fd(fp);
                fileio.close(fd);
                return 0;
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return -1;
            }
        }
        
        // int ferror(FILE *fp);
        public static unsafe int ferror(FILE* fp)
        {
            try
            {
                // Dirty hack:
                if (__errno == 0)
                {
                    if (to_stream(fp) != null)
                    {
                        return 0;
                    }
                }
                return -1;
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return -1;
            }
        }

        // int fputc(int c, FILE *fp);
        public static unsafe int fputc(int c, FILE* fp)
        {
            try
            {
                var s = to_stream(fp)!;
                s.WriteByte((byte)c);
                return 0;
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return -1;
            }
        }

        ///////////////////////////////////////////////////////////////////////

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static unsafe void __vwrprintf(
            __writer wr, sbyte* fmt, va_list ap) =>
            stdio.vwrprintf(wr, fmt, ap);

        ///////////////////////////////////////////////////////////////////////

        // int vsprintf(char *buf, char *fmt, va_list ap);
        public static unsafe int vsprintf(sbyte* buf, sbyte* fmt, va_list ap)
        {
            var pbuf = buf;
            nuint len = 0;
            stdio.vwrprintf((p, l) =>
            {
                memcpy(pbuf, p, l);
                pbuf += l;
                len += l;
            },
            fmt, ap);
            *pbuf = 0;
            return (int)len;
        }

        // int vfprintf(FILE *fp, char *fmt, va_list ap);
        public static unsafe int vfprintf(FILE* fp, sbyte* fmt, va_list ap)
        {
            try
            {
                var s = to_stream(fp)!;
                var len = 0;
                stdio.vwrprintf((p, l) =>
                {
                    while (l > 0)
                    {
                        s.WriteByte((byte)*p);
                        p++;
                        l--;
                        len++;
                    }
                },
                fmt, ap);
                return len;
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return -1;
            }
        }
    }
}
