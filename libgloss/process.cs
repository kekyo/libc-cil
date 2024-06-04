/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace C;

public static partial class text
{
    // int system(const char *command);
    public static unsafe int system(sbyte* command)
    {
        try
        {
            var psi = new ProcessStartInfo();
            psi.FileName = __ngetstr(command)!;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.CreateNoWindow = true;
            psi.UseShellExecute = true;

            using var p = Process.Start(psi)!;

            p.WaitForExit();

            return p.ExitCode;
        }
        catch (Exception ex)
        {
             __set_exception_to_errno(ex);
            return -1;
        }
    }
    
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
        var len = __strlen(name);
        var ce = data.environ;
        while (*ce != null)
        {
            if (__strncmp(*ce, name, len) == 0) {
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
}
