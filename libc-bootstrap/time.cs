/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using System.Globalization;
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
        
        [ThreadStatic]
        private static readonly unsafe type.tm* __localtime_buffer =
            (type.tm*)Marshal.AllocHGlobal(sizeof(type.tm));
        
        private static long __to_time(DateTime dateTime) =>
            (long)dateTime.Subtract(__unix_epoch).TotalSeconds;
        
        private static unsafe void __to_tm(DateTime dateTime, type.tm* tm)
        {
            tm->tm_year = dateTime.Year - 1900;
            tm->tm_mon = dateTime.Month - 1;
            tm->tm_mday = dateTime.Day;
            tm->tm_hour = dateTime.Hour;
            tm->tm_min = dateTime.Minute;
            tm->tm_sec = dateTime.Second;
            tm->tm_wday = (int)dateTime.DayOfWeek;
            tm->tm_yday = (int)(dateTime - new DateTime(
                dateTime.Year, dateTime.Month, 1, 0, 0, 0, DateTimeKind.Local)).
                TotalDays;
            tm->tm_isdst = dateTime.IsDaylightSavingTime() ? 1 : 0;
        }

        private static unsafe void __to_timespec(DateTime dateTime, type.timespec* ts)
        {
            var d = dateTime.Subtract(__unix_epoch);
            ts->tv_sec = (long)d.TotalSeconds;
            ts->tv_nsec = (long)(d.TotalMilliseconds * 1000);
        }

        ///////////////////////////////////////////////////////////////////////

        // time_t time(time_t *tloc);
        public static unsafe long time(long* tloc)
        {
            var t = __to_time(DateTime.Now);
            if (tloc != null)
            {
                *tloc = t;
            }
            return t;
        }
        
        // struct tm *localtime(const time_t *timer);
        public static unsafe type.tm* localtime(long* timer)
        {
            var t = __unix_epoch.AddSeconds(*timer);
            __to_tm(t, __localtime_buffer);
            return __localtime_buffer;
        }

        // char *ctime_r(const time_t *timep, char *buf);
        public static unsafe sbyte* ctime_r(long* timep, sbyte* buf)
        {
            var t = __unix_epoch.AddSeconds(*timep);
            var f = t.ToString("ddd MMM dd HH:mm:ss yyyy\n", CultureInfo.InvariantCulture);
            Debug.Assert(f.Length == 25);
            __nstrdup_to(f, buf);
            return buf;
        }
    }
}
