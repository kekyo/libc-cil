/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace C;

public static partial class text
{
    internal static class heap
    {
        // https://learn.microsoft.com/en-us/cpp/c-runtime-library/crt-debug-heap-details

        private static readonly ulong no_mans_land_bytes = 0xfdfdfdfdfdfdfdfdUL;
        private static readonly ulong dead_bytes = 0xdeadbeefdeadbeefUL;

        private enum HeapCheckModes
        {
            None,
            Check,
            Trap,
        }

        private static readonly HeapCheckModes heap_check_mode =
            get_debugging_switch("LIBC_CIL_DBG_HEAP",
#if DEBUG
                HeapCheckModes.Trap
#else
                HeapCheckModes.None
#endif
                );

        private static readonly bool heap_check_always =
            get_debugging_switch("LIBC_CIL_DBG_HEAP_ALWAYS");

        private unsafe struct heap_block_header
        {
            public static readonly heap_block_header* head;

            static heap_block_header()
            {
                var p = (heap_block_header*)Marshal.AllocHGlobal(sizeof(heap_block_header));
                p->next = p;
                p->previous = p;
                p->request_number = -1;
                p->guard_bytes = no_mans_land_bytes;
                head = p;
            }

            public heap_block_header* next;
            public heap_block_header* previous;
            public sbyte* filename;
            public int line_number;
            public nuint size;
            public long request_number;
            public ulong guard_bytes;
        }

        private static long request_number;
        private static long break_number;
        private static readonly object heap_locker = new();

        [DebuggerStepperBoundary]
        private static unsafe void try_trap_heap(
            bool force, int code, sbyte* filename, int linenumber)
        {
            if (force || heap_check_mode == HeapCheckModes.Trap)
            {
                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }
                else
                {
                    Debugger.Launch();
                }
                Console.Error.WriteLine(
                    $"detected heap corruption: code={code}, location={__ngetstr(filename) ?? "unknown"}({linenumber})");
                Environment.Exit(255);
            }
        }

        public static void set_break_allocation(long number) =>
            break_number = number;

        private static unsafe bool verify_heap(
            bool force, sbyte* filename, int linenumber)
        {
            var head = heap_block_header.head;
            var header = head->next;

            while (header != head)
            {
                if (header->guard_bytes != no_mans_land_bytes)
                {
                    try_trap_heap(force, 1, filename, linenumber);
                    return false;
                }

                if (header->size == 0)
                {
                    try_trap_heap(force, 2, filename, linenumber);
                    return false;
                }

                if (header->previous->next != header)
                {
                    try_trap_heap(force, 3, filename, linenumber);
                    return false;
                }
                if (header->next->previous != header)
                {
                    try_trap_heap(force, 4, filename, linenumber);
                    return false;
                }

                var body = (byte*)(header + 1);
                var another_guard_bytes = body + header->size;

                fixed (void* ap = &no_mans_land_bytes)
                {
                    if (__memcmp(another_guard_bytes, ap, sizeof(ulong)) != 0)
                    {
                        try_trap_heap(force, 5, filename, linenumber);
                        return false;
                    }
                }

                header = header->next;
            }

            return true;
        }

        public static unsafe bool verify_heap(sbyte* filename, int linenumber)
        {
            if (heap_check_mode == HeapCheckModes.None)
            {
                return true;
            }

            lock (heap_locker)
            {
                return verify_heap(false, filename, linenumber);
            }
        }

        // We use HGlobal family for allocator instead of COM allocator.
        // Because they can receive size parameter by intptr type.

        public static unsafe void* malloc(
            nuint size, sbyte* filename, int linenumber)
        {
            if (size == 0)
            {
                __trap(filename, linenumber);
                return null;
            }

            try
            {
                if (heap_check_mode != HeapCheckModes.None)
                {
                    var number = Interlocked.Increment(ref request_number);
                    if (number == break_number)
                    {
                        __force_trap(filename, linenumber);
                    }

                    var total_size =
                        (nuint)sizeof(heap_block_header) + size +
                        sizeof(ulong);

                    var header = (heap_block_header*)Marshal.AllocHGlobal((nint)total_size);
                    if (header == null)
                    {
                        try_trap_heap(false, 6, filename, linenumber);
                        return null;
                    }

                    var body = (byte*)(header + 1);

                    lock (heap_locker)
                    {
                        if (heap_check_always)
                        {
                            verify_heap(true, filename, linenumber);
                        }

                        var head = heap_block_header.head;

                        if (head->next->previous != head)
                        {
                            try_trap_heap(true, 7, filename, linenumber);
                            return null;
                        }

                        header->next = head->next;
                        head->next = header;

                        header->previous = head;
                        header->next->previous = header;

                        header->filename = filename;
                        header->line_number = linenumber;
                        header->size = size;
                        header->request_number = number;
                        header->guard_bytes = no_mans_land_bytes;

                        var another_guard_bytes = body + size;
                        fixed (void* ap = &no_mans_land_bytes)
                        {
                            __memcpy(another_guard_bytes, ap, sizeof(ulong));
                        }
                    }

                    __memset(body, 0xcd, size);

                    return body;
                }
                else
                {
                    return (void*)Marshal.AllocHGlobal((nint)size);
                }
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return null;
            }
        }

        public static unsafe void* realloc(
            void* p, nuint size, sbyte* filename, int linenumber)
        {
            if (p == null)
            {
                return malloc(size, filename, linenumber);
            }
            
            if (size == 0)
            {
                __trap(filename, linenumber);
                return null;
            }

            try
            {
                if (heap_check_mode != HeapCheckModes.None)
                {
                    var number = Interlocked.Increment(ref request_number);
                    if (number == break_number)
                    {
                        __force_trap(filename, linenumber);
                    }

                    var total_size =
                        (nuint)sizeof(heap_block_header) + size +
                        sizeof(ulong);

                    var old_header = ((heap_block_header*)p) - 1;

                    lock (heap_locker)
                    {
                        if (heap_check_always)
                        {
                            verify_heap(true, filename, linenumber);
                        }

                        var header = (heap_block_header*)Marshal.ReAllocHGlobal((nint)old_header, (nint)total_size);
                        if (header == null)
                        {
                            try_trap_heap(false, 8, filename, linenumber);
                            return null;
                        }

                        if (header != old_header)
                        {
                            header->next->previous = header;
                            header->previous->next = header;
                        }

                        header->filename = filename;
                        header->line_number = linenumber;
                        header->size = size;
                        header->request_number = number;

                        var body = (byte*)(header + 1);

                        var another_guard_bytes = body + size;
                        fixed (void* ap = &no_mans_land_bytes)
                        {
                            __memcpy(another_guard_bytes, ap, sizeof(ulong));
                        }

                        return body;
                    }
                }
                else
                {
                    return (void*)Marshal.ReAllocHGlobal((nint)p, (nint)size);
                }
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return null;
            }
        }

        public static unsafe void* calloc(
            nuint nmemb, nuint size, sbyte* filename, int linenumber)
        {
            var s = nmemb * size;
            var body = malloc(s, filename, linenumber);
            if (body == null)
            {
                return null;
            }

            // malloc is returned uninitialized memory.
            __memset(body, 0, s);
            return body;
        }

        public static unsafe void free(
            void* body, sbyte* filename, int linenumber)
        {
            if (body == null)
            {
                return;
            }

            if (heap_check_mode != HeapCheckModes.None)
            {
                var header = ((heap_block_header*)body) - 1;

                lock (heap_locker)
                {
                    if (heap_check_always)
                    {
                        verify_heap(true, filename, linenumber);
                    }

                    var next = header->next;
                    var prev = header->previous;

                    if (next->previous != header)
                    {
                        try_trap_heap(true, 9, filename, linenumber);
                        return;
                    }

                    if (prev->next != header)
                    {
                        try_trap_heap(true, 10, filename, linenumber);
                        return;
                    }

                    next->previous = prev;
                    prev->next = next;
                }

                header->next = null;
                header->previous = null;
                header->guard_bytes = dead_bytes;

                __memset(body, 0xdd, header->size);

                var another_guard_bytes = ((byte*)body) + header->size;
                fixed (void* ap = &dead_bytes)
                {
                    __memcpy(another_guard_bytes, ap, sizeof(ulong));
                }

                Marshal.FreeHGlobal((nint)header);
            }
            else
            {
                Marshal.FreeHGlobal((nint)body);
            }
        }
    }
}
