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
using System.Runtime.InteropServices;
using System.Threading;

namespace C
{
    namespace type
    {
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        [StructLayout(LayoutKind.Sequential, Size = 0, Pack = 8)]
        public readonly struct __tls_slot
        {
        }
    }
    
    public static partial class text
    {
        private sealed unsafe class TlsAccessor
        {
            private struct TlsValue
            {
                public void* value;
            }
            
            private readonly LocalDataStoreSlot slot = Thread.AllocateDataSlot();
            private readonly nuint size;
            private readonly delegate*<void> initializer;

            public TlsAccessor(nuint size, delegate*<void> initializer)
            {
                this.size = size;            
                this.initializer = initializer;
            }

            public void* GetValue()
            {
                if (Thread.GetData(this.slot) is not TlsValue tlsValue)
                {
                    tlsValue = new();
                    tlsValue.value = calloc(1, this.size);
                    Thread.SetData(this.slot, tlsValue);
                    this.initializer();
                }
                return tlsValue.value;
            }
        }
        
        private sealed unsafe class InternalPThread
        {
            private readonly Thread thread;
            private readonly delegate*<void*, void*> startRoutine;
            private readonly void* arg;
            private void* retVal;

            public InternalPThread(delegate*<void*, void*> startRoutine, void* arg)
            {
                this.startRoutine = startRoutine;
                this.arg = arg;

                this.thread = new(this.ThreadEntry);
                this.thread.Start();
            }

            public void* Join()
            {
                this.thread.Join();
                return this.retVal;
            }

            private void ThreadEntry(object? _) =>
                this.retVal = this.startRoutine(this.arg);
        }
        
        ////////////////////////////////////////////////////////////
        
        [DebuggerStepperBoundary]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static unsafe type.__tls_slot* __alloc_tls_slot(nuint size, delegate*<void> initializer) =>
            (type.__tls_slot*)__alloc_obj(new TlsAccessor(size, initializer));

        [DebuggerStepperBoundary]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public static unsafe void* __get_tls_value(type.__tls_slot* slot)
        {
            var accessor = (TlsAccessor)__get_obj(slot)!;
            return accessor.GetValue();
        }

        ////////////////////////////////////////////////////////////

        // int pthread_create(pthread_t *newthread,
        //   const pthread_attr_t *attr, void *(*start_routine)(void *), void *arg);
        public static unsafe int pthread_create(void** newthread,
            void* attr,
            delegate*<void*, void*> start_routine,
            void* arg)
        {
            try
            {
                var t = new InternalPThread(start_routine, arg);
                *newthread = __alloc_obj(t);
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return __errno;
            }
            return 0;
        }
        
        // int pthread_join(pthread_t th, void **thread_return);
        public static unsafe int pthread_join(void* th, void** thread_return)
        {
            try
            {
                var t = (InternalPThread)__get_obj(th)!;
                var r = t.Join();
                if (thread_return != null)
                {
                    *thread_return = r;
                }
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return __errno;
            }
            return 0;
        }
    }
}

