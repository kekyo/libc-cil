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
using System.Runtime.InteropServices;
using System.Text;

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
    public static unsafe bool __verify_heap() =>
        heap.verify_heap();

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

    // void *realloc(void *buf, size_t size);
    public static unsafe void* realloc(
        void* buf, nuint size) =>
        heap.realloc(buf, size, null, 0);

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static unsafe void* __calloc_dbg(
        nuint nmemb, nuint size, sbyte* filename, int linenumber) =>
        heap.calloc(nmemb, size, filename, linenumber);

    // void free(void *p);
    public static unsafe void free(
        void* p) =>
        heap.free(p);

    ///////////////////////////////////////////////////////////////////////

    // int mkstemp(char *template);
    public static unsafe int mkstemp(sbyte* template)
    {
        var tempPath = __ngetstr(template);
        if (!tempPath.EndsWith("XXXXXX"))
        {
            errno = data.EINVAL;
            return -1;
        }
        tempPath = tempPath.Substring(0, tempPath.Length - 6);
        try
        {
            return fileio.create(tempPath);
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
            var sb = new StringBuilder();
            while (*argv != (sbyte*)0)
            {
                if (sb.Length >= 1)
                {
                    sb.Append(' ');
                }
                sb.Append(__ngetstr(*argv));
                argv++;
            }
            
            var psi = new ProcessStartInfo();
            psi.FileName = "dotnet";
            psi.Arguments = sb.ToString();
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;
            psi.UseShellExecute = true;
       
            var p = Process.Start(psi);
            if (p != null)
            {
                *pid = p.Id;
                return 0;
            }
            else
            {
                errno = data.ENOMEM;
                return -1;
            }
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
