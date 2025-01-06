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
}
