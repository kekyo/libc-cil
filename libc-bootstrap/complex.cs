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
using C.type;

namespace C
{
    namespace type
    {
        public readonly struct _FloatComplex
        {
            internal readonly float __re;
            internal readonly float __im;

            public _FloatComplex(float re, float im)
            {
                this.__re = re;
                this.__im = im;
            }
        }
        
        public readonly struct _DoubleComplex
        {
            internal readonly double __re;
            internal readonly double __im;

            public _DoubleComplex(double re, double im)
            {
                this.__re = re;
                this.__im = im;
            }
        }
    }

    public static partial class text
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static _FloatComplex __CMPLXF(float re, float im) =>
            new(re, im);
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static _DoubleComplex __CMPLX(double re, double im) =>
            new(re, im);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static _FloatComplex __caddf(_FloatComplex a, _FloatComplex b) =>
            new(a.__re + b.__re,
                a.__im + b.__im);
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static _DoubleComplex __cadd(_DoubleComplex a, _DoubleComplex b) =>
            new(a.__re + b.__re,
                a.__im + b.__im);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static _FloatComplex __csubf(_FloatComplex a, _FloatComplex b) =>
            new(a.__re - b.__re,
                a.__im - b.__im);
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static _DoubleComplex __csub(_DoubleComplex a, _DoubleComplex b) =>
            new(a.__re - b.__re,
                a.__im - b.__im);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static _FloatComplex __cmulf(_FloatComplex a, _FloatComplex b) =>
            new(a.__re * b.__re - a.__im * b.__im,
                a.__im * b.__re + a.__re * b.__im);
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static _DoubleComplex __cmul(_DoubleComplex a, _DoubleComplex b) =>
            new(a.__re * b.__re - a.__im * b.__im,
                a.__im * b.__re + a.__re * b.__im);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static _FloatComplex __cdivf(_FloatComplex a, _FloatComplex b)
        {
            var d = b.__re * b.__re + b.__im * b.__im;
            return new(
                (a.__re * b.__re + a.__im * b.__im) / d,
                (a.__im * b.__re - a.__re * b.__im) / d);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static _DoubleComplex __cdiv(_DoubleComplex a, _DoubleComplex b)
        {
            var d = b.__re * b.__re + b.__im * b.__im;
            return new(
                (a.__re * b.__re + a.__im * b.__im) / d,
                (a.__im * b.__re - a.__re * b.__im) / d);
        }

        public static float crealf(_FloatComplex fc) =>
            fc.__re;
        public static float cimagf(_FloatComplex fc) =>
            fc.__im;
        
        public static double creal(_DoubleComplex dc) =>
            dc.__re;
        public static double cimag(_DoubleComplex dc) =>
            dc.__im;
        
        public static double creall(_DoubleComplex ldc) =>
            ldc.__re;
        public static double cimagl(_DoubleComplex ldc) =>
            ldc.__im;
    }
}
