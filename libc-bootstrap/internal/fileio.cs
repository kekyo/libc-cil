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
        }
                
        private static unsafe int Register(Stream s)
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

        public static unsafe int force_create(string path)
        {
            var f = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
            return Register(f);
        }
    
        public static unsafe int create(string path)
        {
            var f = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
            return Register(f);
        }
      
        public static unsafe int open(string path)
        {
            var f = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            return Register(f);
        }
      
        public static bool close(int fd)
        {
            lock (files)
            {
                if (files.TryGetValue(fd, out var f))
                {
                    files.Remove(fd);
                    f.Dispose();
                    return true;
                }
            }
            return false;
        }
    }
}
