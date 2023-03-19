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

namespace C.type;

[EditorBrowsable(EditorBrowsableState.Advanced)]
public sealed class __unknown_errno_exception : Exception
{
    public readonly int code;

    public __unknown_errno_exception(int code) =>
        this.code = code;

    public override string ToString() =>
        $"Unknown error {this.code}";
}
