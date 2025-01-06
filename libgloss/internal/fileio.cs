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
using System.IO;

namespace C;

public static partial class text
{
    internal static class fileio
    {
        private static readonly Dictionary<int, Stream> files = new();
        private static readonly Stack<int> descriptors = new();

        static fileio()
        {
            lock (files)
            {
                files.Add(0, Console.OpenStandardInput());
                files.Add(1, Console.OpenStandardOutput());
                files.Add(2, Console.OpenStandardError());
            }

            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                lock (files)
                {
                    foreach (var stream in files.Values)
                    {
                        if (stream.CanWrite)
                        {
                            stream.Flush();
                        }
                    }
                }
            };
        }

        public static Stream? get_stream(int fd)
        {
            lock (files)
            {
                return files.TryGetValue(fd, out var s) ? s : null;
            }
        }
                
        public static int register_stream(Stream s)
        {
            int fd;
            lock (files)
            {
                if (descriptors.Count >= 1)
                {
                    fd = descriptors.Pop();
                }
                else
                {
                    fd = files.Count;
                }
                files.Add(fd, s);
            }
            return fd;
        }

        public static int force_create(string path)
        {
            var f = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            return register_stream(f);
        }

        public static int create(string path)
        {
            var f = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
            return register_stream(f);
        }

        public static int open(string path)
        {
            var f = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            return register_stream(f);
        }
      
        public static bool close(int fd)
        {
            Stream? f = null;
            lock (files)
            {
                if (files.TryGetValue(fd, out f))
                {
                    files.Remove(fd);
                    descriptors.Push(fd);
                }
            }

            if (f != null)
            {
                f.Flush();
                f.Dispose();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
