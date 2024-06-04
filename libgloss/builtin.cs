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

namespace C;

public static partial class data
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static unsafe readonly nuint __builtin_sizeof_intptr = (nuint)sizeof(nint);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static unsafe readonly nuint __builtin_sizeof_uintptr = (nuint)sizeof(nuint);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static readonly nint __builtin_intptr_max = ((nint)new IntPtr(-1)) >> 1;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static readonly nint __builtin_intptr_min = ~(((nint)new IntPtr(-1)) >> 1);

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static readonly nuint __builtin_uintptr_max = (nuint)(nint)new IntPtr(-1);
}

public static partial class text
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static unsafe bool __baddovf(int lhs, int rhs, int* res)
    {
        try
        {
            checked
            {
                *res = lhs + rhs;
                return true;
            }
        }
        catch (OverflowException)
        {
            return false;
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static unsafe bool __baddovfu(uint lhs, uint rhs, uint* res)
    {
        try
        {
            checked
            {
                *res = lhs + rhs;
                return true;
            }
        }
        catch (OverflowException)
        {
            return false;
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static unsafe bool __baddovfl(long lhs, long rhs, long* res)
    {
        try
        {
            checked
            {
                *res = lhs + rhs;
                return true;
            }
        }
        catch (OverflowException)
        {
            return false;
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static unsafe bool __baddovful(ulong lhs, ulong rhs, ulong* res)
    {
        try
        {
            checked
            {
                *res = lhs + rhs;
                return true;
            }
        }
        catch (OverflowException)
        {
            return false;
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static unsafe bool __bsubovf(int lhs, int rhs, int* res)
    {
        try
        {
            checked
            {
                *res = lhs - rhs;
                return true;
            }
        }
        catch (OverflowException)
        {
            return false;
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static unsafe bool __bsubovfu(uint lhs, uint rhs, uint* res)
    {
        try
        {
            checked
            {
                *res = lhs - rhs;
                return true;
            }
        }
        catch (OverflowException)
        {
            return false;
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static unsafe bool __bsubovfl(long lhs, long rhs, long* res)
    {
        try
        {
            checked
            {
                *res = lhs - rhs;
                return true;
            }
        }
        catch (OverflowException)
        {
            return false;
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static unsafe bool __bsubovful(ulong lhs, ulong rhs, ulong* res)
    {
        try
        {
            checked
            {
                *res = lhs - rhs;
                return true;
            }
        }
        catch (OverflowException)
        {
            return false;
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static unsafe bool __bmulovf(int lhs, int rhs, int* res)
    {
        try
        {
            checked
            {
                *res = lhs * rhs;
                return true;
            }
        }
        catch (OverflowException)
        {
            return false;
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static unsafe bool __bmulovfu(uint lhs, uint rhs, uint* res)
    {
        try
        {
            checked
            {
                *res = lhs * rhs;
                return true;
            }
        }
        catch (OverflowException)
        {
            return false;
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static unsafe bool __bmulovfl(long lhs, long rhs, long* res)
    {
        try
        {
            checked
            {
                *res = lhs * rhs;
                return true;
            }
        }
        catch (OverflowException)
        {
            return false;
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static unsafe bool __bmulovful(ulong lhs, ulong rhs, ulong* res)
    {
        try
        {
            checked
            {
                *res = lhs * rhs;
                return true;
            }
        }
        catch (OverflowException)
        {
            return false;
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static int __builtin_ffs(int x)
    {
        if (x == 0)
        {
            return 0;
        }

        var v = (uint)x;
        var index = 1;
        var mask = 0x01U;
        while (true)
        {
            if ((v & mask) != 0U)
            {
                return index;
            }
            index++;
            v <<= 1;
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static int __builtin_ffsl(long x)
    {
        if (x == 0)
        {
            return 0;
        }
        
        var v = (ulong)x;
        var index = 1;
        var mask = 0x01UL;
        while (true)
        {
            if ((v & mask) != 0UL)
            {
                return index;
            }
            index++;
            v <<= 1;
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static int __builtin_clz(int x)
    {
        if (x == 0)
        {
            return 32;
        }
        
        var v = (uint)x;
        var index = 0;
        var mask = 0x80000000U;
        while (true)
        {
            if ((v & mask) != 0U)
            {
                return index;
            }
            index++;
            v >>= 1;
        }
    }
    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static int __builtin_clzl(long x)
    {
        if (x == 0)
        {
            return 64;
        }
        
        var v = (ulong)x;
        var index = 0;
        var mask = 0x8000000000000000UL;
        while (true)
        {
            if ((v & mask) != 0UL)
            {
                return index;
            }
            index++;
            v >>= 1;
        }
    }
}
