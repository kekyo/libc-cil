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
using System.IO;
using System.Runtime.InteropServices;

namespace C;

public static partial class text
{
    private static class interop
    {
        // Imported from https://github.com/kekyo/FlashCap/blob/main/FlashCap.Core/Internal/NativeMethods.cs

        // https://stackoverflow.com/questions/38790802/determine-operating-system-in-net-core
        public enum Platforms
        {
            Windows,
            Linux,
            MacOS,
            Other,
        }

        private static Platforms GetRuntimePlatform()
        {
            var windir = Environment.GetEnvironmentVariable("windir");
            if (!string.IsNullOrEmpty(windir) &&
                windir.Contains(Path.DirectorySeparatorChar.ToString()) &&
                Directory.Exists(windir))
            {
                return Platforms.Windows;
            }
            else if (File.Exists(@"/proc/sys/kernel/ostype"))
            {
                var osType = File.ReadAllText(@"/proc/sys/kernel/ostype");
                if (osType.StartsWith("Linux", StringComparison.OrdinalIgnoreCase))
                {
                    return Platforms.Linux;
                }
                else
                {
                    return Platforms.Other;
                }
            }
            else if (File.Exists(@"/System/Library/CoreServices/SystemVersion.plist"))
            {
                return Platforms.MacOS;
            }
            else
            {
                return Platforms.Other;
            }
        }

        public static readonly Platforms CurrentPlatform =
            GetRuntimePlatform();

        ////////////////////////////////////////////////////////////////////////

        // https://docs.microsoft.com/en-us/previous-versions/windows/desktop/legacy/aa366535(v=vs.85)
        [DllImport("ntdll")]
        private static extern void RtlCopyMemory(IntPtr dest, IntPtr src, IntPtr length);
        [DllImport("kernel32")]
        private static extern void RtlMoveMemory(IntPtr dest, IntPtr src, IntPtr length);
        [DllImport("kernel32")]
        private static extern void RtlFillMemory(IntPtr dest, IntPtr length, int fill);

        [DllImport("libc")]
        private static extern IntPtr memcpy(IntPtr dest, IntPtr src, IntPtr length);
        [DllImport("libc")]
        private static extern IntPtr memset(IntPtr ptr, int c, IntPtr size);

        private static IntPtr Win32CopyMemory(IntPtr dest, IntPtr src, IntPtr length)
        {
            RtlCopyMemory(dest, src, length);
            return dest;
        }

        private static IntPtr Win32MoveMemory(IntPtr dest, IntPtr src, IntPtr length)
        {
            RtlMoveMemory(dest, src, length);
            return dest;
        }

        public delegate IntPtr CopyMemoryDelegate(
            IntPtr pDestination, IntPtr pSource, IntPtr length);

        public static unsafe readonly CopyMemoryDelegate __memcpy =
            CurrentPlatform == Platforms.Windows ?
                (IntPtr.Size == 4 ? Win32MoveMemory : Win32CopyMemory) :
                memcpy;

        private static IntPtr Win32FillMemory(IntPtr ptr, int c, IntPtr size)
        {
            RtlFillMemory(ptr, size, c);
            return ptr;
        }

        public delegate IntPtr FillMemoryDelegate(
            IntPtr pDestination, int c, IntPtr length);
        
        public static unsafe readonly FillMemoryDelegate __memset =
            CurrentPlatform == Platforms.Windows ?
                Win32FillMemory :
                memset;
    }
}
