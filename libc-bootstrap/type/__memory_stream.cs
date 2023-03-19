/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using System.IO;
using System.Runtime.InteropServices;

namespace C.type;

//#if NETSTANDARD1_6   // TODO:

internal sealed unsafe class __memory_stream : MemoryStream
{
    private sbyte** ptr;
    private nuint* sizeloc;
    private byte[]? lastBuffer;
    private GCHandle lastHandle;

    public __memory_stream(sbyte** ptr, nuint* sizeloc)
    {
        this.ptr = ptr;
        this.sizeloc = sizeloc;

        this.lastBuffer = base.GetBuffer();
        this.lastHandle = GCHandle.Alloc(this.lastBuffer, GCHandleType.Pinned);
        *ptr = (sbyte*)this.lastHandle.AddrOfPinnedObject().ToPointer();
        *sizeloc = 0;
    }

    public override void Flush()
    {
        base.Flush();
        var buf = GetBuffer();
        if (buf != lastBuffer)
        {
            this.lastHandle.Free();
            this.lastBuffer = buf;
            this.lastHandle = GCHandle.Alloc(this.lastBuffer, GCHandleType.Pinned);
            *ptr = (sbyte*)this.lastHandle.AddrOfPinnedObject().ToPointer();
        }
        *sizeloc = (nuint)base.Length;
    }

    public override void Close()
    {
        base.Close();

        this.lastHandle.Free();
        this.lastBuffer = null;

        var buf = ToArray();
        *ptr = (sbyte*)text.malloc((nuint)buf.Length);
        Marshal.Copy(buf, 0, (nint)(*ptr), buf.Length);
        *sizeloc = (nuint)base.Length;
    }
}

//#endif
