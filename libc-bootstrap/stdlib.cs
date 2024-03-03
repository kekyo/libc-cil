/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace C;

public static partial class data
{
    public static unsafe sbyte** environ = __getenviron();

    private static unsafe sbyte** __getenviron()
    {
        var envs = Environment.GetEnvironmentVariables();
        var penv = (sbyte**)text.heap.malloc(
            (nuint)(envs.Count * sizeof(sbyte*)), null, 0);
        var index = 0;
        foreach (var entry in envs)
        {
            var kv = (DictionaryEntry)entry!;
            penv[index++] = text.__nstrdup($"{kv.Key}={kv.Value}");
        }
        return penv;
    }
}

public static partial class text
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static void __set_break_allocation(long number) =>
        heap.set_break_allocation(number);

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static unsafe bool __verify_heap(sbyte* filename, int linenumber) =>
        heap.verify_heap(filename, linenumber);

    // void *malloc(size_t size);
    public static unsafe void* malloc(
        nuint size) =>
        heap.malloc(size, null, 0);

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static unsafe void* __malloc_dbg(
        nuint size, sbyte* filename, int linenumber) =>
        heap.malloc(size, filename, linenumber);

    // void *calloc(size_t nmemb, size_t size);
    public static unsafe void* calloc(
        nuint nmemb, nuint size) =>
        heap.calloc(nmemb, size, null, 0);

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static unsafe void* __calloc_dbg(
        nuint nmemb, nuint size, sbyte* filename, int linenumber) =>
        heap.calloc(nmemb, size, filename, linenumber);

    // void *realloc(void *buf, size_t size);
    public static unsafe void* realloc(
        void* buf, nuint size) =>
        heap.realloc(buf, size, null, 0);

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static unsafe void* __realloc_dbg(
        void* buf, nuint size, sbyte* filename, int linenumber) =>
        heap.realloc(buf, size, filename, linenumber);

    // void free(void *p);
    public static unsafe void free(
        void* p) =>
        heap.free(p, null, 0);

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static unsafe void __free_dbg(
        void* p, sbyte* filename, int linenumber) =>
        heap.free(p, filename, linenumber);

    ///////////////////////////////////////////////////////////////////////

    private static readonly char[] __gettemp_ch =
    {
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
    };
    
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
    
    ///////////////////////////////////////////////////////////////////////

    // typedef int pid_t;
    // int posix_spawnp(pid_t *pid,
    //   const char *path,
    //   const void *file_actions,
    //   const void *attrp,
    //   char *const argv[],
    //   char *const envp[]);
    public static unsafe int posix_spawnp(
        int* pid, sbyte* path, void* file_actions, void* attrp, sbyte** argv, sbyte** envp)
    {
        try
        {
            // HACK: Cursed specification...
            // https://stackoverflow.com/questions/70122718/how-to-get-separate-arguments-from-single-arguments-string-like-processstartinfo
            // https://github.com/dotnet/runtime/blob/dfcbcb450bd67e091ae697e788a8c7a88eb8cbec/src/libraries/System.Diagnostics.Process/src/System/Diagnostics/Process.Unix.cs#L853
            var sb = new StringBuilder();
            if (*argv != (sbyte*)0)
            {
                argv++;
                while (*argv != (sbyte*)0)
                {
                    if (sb.Length >= 1)
                    {
                        sb.Append(' ');
                    }

                    var arg = __ngetstr(*argv)!;
                    sb.Append($"\"{arg.Replace("\"", "\"\"")}\"");
                    argv++;
                }
            }

            var fileName = __ngetstr(path)!;
#if false            
            Console.WriteLine(
                $"posix_spawnp: {fileName} {sb}");
#endif
            var parentIn = fileio.get_stream(0);
            
            var psi = new ProcessStartInfo();
            psi.FileName = fileName;
            psi.Arguments = sb.ToString();
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardInput = parentIn != null;
            //psi.RedirectStandardOutput = true;
            //psi.RedirectStandardError = true;

            var p = Process.Start(psi);
            if (p != null)
            {
                if (parentIn != null)
                {
                    var transferThread = new Thread(() =>
                    {
                        using var exited = new ManualResetEvent(false);
                        p.EnableRaisingEvents = true;
                        p.Exited += (_, _) => exited.Set();

                        var childIn = p.StandardInput.BaseStream;

                        var parentInBuffer = new byte[4096];
                        var parentInAR = parentIn.BeginRead(
                            parentInBuffer, 0, parentInBuffer.Length, null!, null!);

                        while (true)
                        {
                            var waitHandles = new WaitHandle[]
                            {
                                exited, parentInAR.AsyncWaitHandle,
                            };
                    
                            var r = WaitHandle.WaitAny(waitHandles);
                            if (r == 0)
                            {
                                break;
                            }

                            try
                            {
                                if (parentInAR.IsCompleted)
                                {
                                    var length = parentIn.EndRead(parentInAR);
                                    if (length >= 1)
                                    {
                                        childIn.Write(parentInBuffer, 0, length);
                                        parentInAR = parentIn.BeginRead(
                                            parentInBuffer, 0, parentInBuffer.Length, null!, null!);
                                    }
                                    else
                                    {
                                        childIn.Close();
                                    }
                                }
                            }
                            catch
                            {
                            }
                        }

                        try
                        {
                            parentIn.EndRead(parentInAR);
                        }
                        catch
                        {
                        }
                    });
                    transferThread.IsBackground = true;
                    transferThread.Start();
                }
                
                *pid = p.Id;
                return 0;
            }
            else
            {
                errno = data.ENOMEM;
                return -1;
            }
        }
        // Invalid program format
        catch (Win32Exception)
        {
            errno = data.EACCES;
            return -1;
        }
        catch (Exception ex)
        {
             __set_exception_to_errno(ex);
            return -1;
        }
    }

    // pid_t waitpid(pid_t pid, int *stat_loc, int options);
    public static unsafe int waitpid(int pid, int* stat_loc, int options)
    {
        try
        {
            var p = Process.GetProcessById(pid);
            p.WaitForExit();
            *stat_loc = 0x80;
            return 0;
        }
        catch (ArgumentException)
        {
            *stat_loc = 0x80;
            return 0;
        }
        catch (Exception ex)
        {
            __set_exception_to_errno(ex);
            return -1;
        }
    }

    ///////////////////////////////////////////////////////////////////////

    // char *getenv(char *name);
    public static unsafe sbyte* getenv(sbyte* name)
    {
        var len = strlen(name);
        var ce = data.environ;
        while (*ce != null)
        {
            if (strncmp(*ce, name, len) == 0) {
                return (*ce) + len + 1;  // +1: '='
            }
            ce++;
        }
        return null;
    }

    ///////////////////////////////////////////////////////////////////////

    // int atexit(void (*)(void));
    public static unsafe int atexit(delegate*<void> function)
    {
        if (function != null)
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => function();
            return 0;
        }
        else
        {
            errno = data.EINVAL;
            return -1;
        }
    }
    
    ///////////////////////////////////////////////////////////////////////

    // unsigned long strtoul(const char *nptr, char **endptr, int base);
    public static unsafe ulong strtoul(sbyte* nptr, sbyte** endptr, int @base) =>
        strto.strtoul(nptr, endptr, @base);

    // double strtod(const char *nptr, char **endptr);
    public static unsafe double strtod(sbyte* nptr, sbyte** endptr) =>
        strto.strtod(nptr, endptr);
}
