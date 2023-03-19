/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using System.ComponentModel;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace C.type;

[EditorBrowsable(EditorBrowsableState.Advanced)]
public sealed unsafe class __array_holder<T> : CriticalFinalizerObject
    where T : unmanaged
{
    private static readonly nuint element_size = (nuint)sizeof(T);

    private T* ptr;

    public __array_holder(int length) =>
        this.ptr = (T*)text.calloc((nuint)length, element_size);
    public __array_holder(params T[] arr)
    {
        var h = GCHandle.Alloc(arr, GCHandleType.Pinned);
        try
        {
            var parr = (void*)h.AddrOfPinnedObject();
            var size = (nuint)arr.Length * element_size;
            this.ptr = (T*)text.malloc(size);
            text.memcpy(this.ptr, parr, size);
        }
        finally
        {
            h.Free();
        }
    }

    ~__array_holder()
    {
        if (this.ptr != (T*)0)
        {
            text.free(this.ptr);
            this.ptr = (T*)0;
        }
    }

    public T* get() =>
        this.ptr;
    public T* get(int index) =>
        this.ptr + index;

    public ref T this[int index] =>
        ref *(this.ptr + index);

    public static implicit operator T*(__array_holder<T> sh) =>
        sh.get();
}
