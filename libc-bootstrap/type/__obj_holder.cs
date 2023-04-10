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

namespace C.type;

[EditorBrowsable(EditorBrowsableState.Advanced)]
public sealed unsafe class __obj_holder : CriticalFinalizerObject
{
    private void* ptr;

    public __obj_holder(string str) =>
        this.ptr = text.__nstrdup(str);

    public __obj_holder(sbyte* str) =>
        this.ptr = str;

    ~__obj_holder()
    {
        if (this.ptr != null)
        {
            text.free(this.ptr);
            this.ptr = null;
        }
    }

    public void* get() =>
        this.ptr;
    public sbyte* c_str() =>
        (sbyte*)this.ptr;

    public static implicit operator __obj_holder(string str) =>
        new __obj_holder(str);
    public static implicit operator __obj_holder(sbyte* str) =>
        new __obj_holder(str);

    public static implicit operator void*(__obj_holder sh) =>
        sh.get();
    public static implicit operator sbyte*(__obj_holder sh) =>
        (sbyte*)sh.get();
}
