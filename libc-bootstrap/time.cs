/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace C
{
    namespace type
    {
        [StructLayout(LayoutKind.Sequential, Size = 0, Pack = 8)]
        public struct tm
        {
            public int tm_sec;
            public int tm_min;
            public int tm_hour;
            public int tm_mday;
            public int tm_mon;
            public int tm_year;
            public int tm_wday;
            public int tm_yday;
            public int tm_isdst;
        }
    }

    ///////////////////////////////////////////////////////////////////////

    public static partial class text
    {
        private static readonly DateTime __unix_epoch =
            new(1970, 1, 1, 0, 0, 0, DateTimeKind.Local);
        private static unsafe type.tm* __localtime_buffer =
            (type.tm*)Marshal.AllocHGlobal(sizeof(type.tm));
        
        // time_t time(time_t *tloc);
        public static unsafe long time(long* tloc)
        {
            var t = (long)DateTime.Now.Subtract(__unix_epoch).TotalSeconds;
            if (tloc != null)
            {
                *tloc = t;
            }
            return t;
        }
        
        // struct tm *localtime(const time_t *timer);
        public static unsafe type.tm* localtime(long *timer)
        {
            var t = __unix_epoch.AddSeconds(*timer);
            __localtime_buffer->tm_year = t.Year - 1900;
            __localtime_buffer->tm_mon = t.Month - 1;
            __localtime_buffer->tm_mday = t.Day;
            __localtime_buffer->tm_hour = t.Hour;
            __localtime_buffer->tm_min = t.Minute;
            __localtime_buffer->tm_sec = t.Second;
            __localtime_buffer->tm_wday = (int)t.DayOfWeek;
            __localtime_buffer->tm_yday =
                (int)(t - new DateTime(t.Year, t.Month, 1, 0, 0, 0, DateTimeKind.Local)).TotalDays;
            __localtime_buffer->tm_isdst = t.IsDaylightSavingTime() ? 1 : 0;
            return __localtime_buffer;
        }
    }
}
