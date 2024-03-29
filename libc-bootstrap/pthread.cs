/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;

namespace C;

public static partial class text
{
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
            *thread_return = t.Join();
        }
        catch (Exception ex)
        {
            __set_exception_to_errno(ex);
            return __errno;
        }
        return 0;
    }
}
