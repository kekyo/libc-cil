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
using System.IO;
using System.Runtime.InteropServices;
using C.type;

namespace C;

public static partial class data
{
    public const int EPERM = 1;
    public const int ENOENT = 2;
    public const int ESRCH = 3;
    public const int EINTR = 4;
    public const int EIO = 5;
    public const int ENXIO = 6;
    public const int E2BIG = 7;
    public const int ENOEXEC = 8;
    public const int EBADF = 9;
    public const int ECHILD = 10;
    public const int EAGAIN = 11;
    public const int ENOMEM = 12;
    public const int EACCES = 13;
    public const int EFAULT = 14;
    public const int ENOTBLK = 15;
    public const int EBUSY = 16;
    public const int EEXIST = 17;
    public const int EXDEV = 18;
    public const int ENODEV = 19;
    public const int ENOTDIR = 20;
    public const int EISDIR = 21;
    public const int EINVAL = 22;
    public const int ENFILE = 23;
    public const int EMFILE = 24;
    public const int ENOTTY = 25;
    public const int ETXTBSY = 26;
    public const int EFBIG = 27;
    public const int ENOSPC = 28;
    public const int ESPIPE = 29;
    public const int EROFS = 30;
    public const int EMLINK = 31;
    public const int EPIPE = 32;
    public const int EDOM = 33;
    public const int ERANGE = 34;
    public const int EDEADLK = 35;
    public const int ENAMETOOLONG = 36;
    public const int ENOLCK = 37;
    public const int ENOSYS = 38;
    public const int ENOTEMPTY = 39;
    public const int ELOOP = 40;
    public const int EWOULDBLOCK = EAGAIN;
    public const int ENOMSG = 42;
    public const int EIDRM = 43;
    public const int ECHRNG = 44;
    public const int EL2NSYNC = 45;
    public const int EL3HLT = 46;
    public const int EL3RST = 47;
    public const int ELNRNG = 48;
    public const int EUNATCH = 49;
    public const int ENOCSI = 50;
    public const int EL2HLT = 51;
    public const int EBADE = 52;
    public const int EBADR = 53;
    public const int EXFULL = 54;
    public const int ENOANO = 55;
    public const int EBADRQC = 56;
    public const int EBADSLT = 57;
    public const int EDEADLOCK = EDEADLK;
    public const int EBFONT = 59;
    public const int ENOSTR = 60;
    public const int ENODATA = 61;
    public const int ETIME = 62;
    public const int ENOSR = 63;
    public const int ENONET = 64;
    public const int ENOPKG = 65;
    public const int EREMOTE = 66;
    public const int ENOLINK = 67;
    public const int EADV = 68;
    public const int ESRMNT = 69;
    public const int ECOMM = 70;
    public const int EPROTO = 71;
    public const int EMULTIHOP = 72;
    public const int EDOTDOT = 73;
    public const int EBADMSG = 74;
    public const int EOVERFLOW = 75;
    public const int ENOTUNIQ = 76;
    public const int EBADFD = 77;
    public const int EREMCHG = 78;
    public const int ELIBACC = 79;
    public const int ELIBBAD = 80;
    public const int ELIBSCN = 81;
    public const int ELIBMAX = 82;
    public const int ELIBEXEC = 83;
    public const int EILSEQ = 84;
    public const int ERESTART = 85;
    public const int ESTRPIPE = 86;
    public const int EUSERS = 87;
    public const int ENOTSOCK = 88;
    public const int EDESTADDRREQ = 89;
    public const int EMSGSIZE = 90;
    public const int EPROTOTYPE = 91;
    public const int ENOPROTOOPT = 92;
    public const int EPROTONOSUPPORT = 93;
    public const int ESOCKTNOSUPPORT = 94;
    public const int EOPNOTSUPP = 95;
    public const int EPFNOSUPPORT = 96;
    public const int EAFNOSUPPORT = 97;
    public const int EADDRINUSE = 98;
    public const int EADDRNOTAVAIL = 99;
    public const int ENETDOWN = 100;
    public const int ENETUNREACH = 101;
    public const int ENETRESET = 102;
    public const int ECONNABORTED = 103;
    public const int ECONNRESET = 104;
    public const int ENOBUFS = 105;
    public const int EISCONN = 106;
    public const int ENOTCONN = 107;
    public const int ESHUTDOWN = 108;
    public const int ETOOMANYREFS = 109;
    public const int ETIMEDOUT = 110;
    public const int ECONNREFUSED = 111;
    public const int EHOSTDOWN = 112;
    public const int EHOSTUNREACH = 113;
    public const int EALREADY = 114;
    public const int EINPROGRESS = 115;
    public const int ESTALE = 116;
    public const int EUCLEAN = 117;
    public const int ENOTNAM = 118;
    public const int ENAVAIL = 119;
    public const int EISNAM = 120;
    public const int EREMOTEIO = 121;
    public const int EDQUOT = 122;
    public const int ENOMEDIUM = 123;
    public const int EMEDIUMTYPE = 124;
    public const int ECANCELED = 125;
    public const int ENOKEY = 126;
    public const int EKEYEXPIRED = 127;
    public const int EKEYREVOKED = 128;
    public const int EKEYREJECTED = 129;
    public const int EOWNERDEAD = 130;
    public const int ENOTRECOVERABLE = 131;
    public const int ERFKILL = 132;
    public const int EHWPOISON = 133;
    public const int ENOTSUP = EOPNOTSUPP;
}

public static partial class text
{
    // https://sourceforge.net/p/libnix/code/
    // https://github.com/diegocr/libnix/blob/master/string/strerror.c
    private static readonly string[] __error_messages =
    {
	    "Undefined error",
		"Operation not permitted",	// 1: EPERM
		"No such file or directory",	// 2: ENOENT
		"No such process",	// 3: ESRCH
		"Interrupted system call",	// 4: EINTR
		"I/O error",	// 5: EIO
		"No such device or address",	// 6: ENXIO
		"Argument list too long",	// 7: E2BIG
		"Exec format error",	// 8: ENOEXEC
		"Bad file number",	// 9: EBADF
		"No child processes",	// 10: ECHILD
		"Try again",	// 11: EAGAIN
		"Out of memory",	// 12: ENOMEM
		"Permission denied",	// 13: EACCES
		"Bad address",	// 14: EFAULT
		"Block device required",	// 15: ENOTBLK
		"Device or resource busy",	// 16: EBUSY
		"File exists",	// 17: EEXIST
		"Cross-device link",	// 18: EXDEV
		"No such device",	// 19: ENODEV
		"Not a directory",	// 20: ENOTDIR
		"Is a directory",	// 21: EISDIR
		"Invalid argument",	// 22: EINVAL
		"File table overflow",	// 23: ENFILE
		"Too many open files",	// 24: EMFILE
		"Not a typewriter",	// 25: ENOTTY
		"Text file busy",	// 26: ETXTBSY
		"File too large",	// 27: EFBIG
		"No space left on device",	// 28: ENOSPC
		"Illegal seek",	// 29: ESPIPE
		"Read-only file system",	// 30: EROFS
		"Too many links",	// 31: EMLINK
		"Broken pipe",	// 32: EPIPE
		/* math software */
		"Math argument out of domain of func",	// 33: EDOM
		"Math result not representable",	// 34: ERANGE
		/* non-blocking and interrupt i/o */
		"Resource temporarily unavailable",	// 35: EAGAIN
		"Operation now in progress",	// 36: EINPROGRESS
		"Operation already in progress",	// 37: EALREADY
		/* ipc/network software -- argument errors */
		"Socket operation on non-socket",	// 38: ENOTSOCK
		"Destination address required",	// 39: EDESTADDRREQ
		"Message too long",	// 40: EMSGSIZE
		"Protocol wrong type for socket",	// 41: EPROTOTYPE
		"Protocol not available",	// 42: ENOPROTOOPT
		"Protocol not supported",	// 43: EPROTONOSUPPORT
		"Socket type not supported",	// 44: ESOCKTNOSUPPORT
		"Operation not supported",	// 45: EOPNOTSUPP
		"Protocol family not supported",	// 46: EPFNOSUPPORT
		"Address family not supported by protocol family",	// 47: EAFNOSUPPORT
		"Address already in use",	// 48: EADDRINUSE
		"Can't assign requested address",	// 49: EADDRNOTAVAIL
		/* ipc/network software -- operational errors */
		"Network is down",	// 50: ENETDOWN
		"Network is unreachable",	// 51: ENETUNREACH
		"Network dropped connection on reset",	// 52: ENETRESET
		"Software caused connection abort",	// 53: ECONNABORTED
		"Connection reset by peer",	// 54: ECONNRESET
		"No buffer space available",	// 55: ENOBUFS
		"Socket is already connected",	// 56: EISCONN
		"Socket is not connected",	// 57: ENOTCONN
		"Can't send after socket shutdown",	// 58: ESHUTDOWN
		"Too many references: can't splice",	// 59: ETOOMANYREFS
		"Operation timed out",	// 60: ETIMEDOUT
		"Connection refused",	// 61: ECONNREFUSED
		"Too many levels of symbolic links",	// 62: ELOOP
		"File name too long",	// 63: ENAMETOOLONG
		/* should be rearranged */
		"Host is down",	// 64: EHOSTDOWN
		"No route to host",	// 65: EHOSTUNREACH
		"Directory not empty",	// 66: ENOTEMPTY
		/* quotas & mush */
		"Too many processes",	// 67: EPROCLIM
		"Too many users",	// 68: EUSERS
		"Disc quota exceeded",	// 69: EDQUOT
		/* Network File System */
		"Stale NFS file handle",	// 70: ESTALE
		"Too many levels of remote in path",	// 71: EREMOTE
		"RPC struct is bad",	// 72: EBADRPC
		"RPC version wrong",	// 73: ERPCMISMATCH
		"RPC prog. not avail",	// 74: EPROGUNAVAIL
		"Program version wrong",	// 75: EPROGMISMATCH
		"Bad procedure for program",	// 76: EPROCUNAVAIL
		"No locks available",	// 77: ENOLCK
		"Function not implemented",	// 78: ENOSYS
		"Inappropriate file type or format",	// 79: EFTYPE
		/* others.. */
		"corrupted shared library",	// 80: ELIBBAD
		"section corrupted",	// 81: ELIBSCN
		"link overflow",	// 82: ELIBMAX
		"exec error",	// 83: ELIBEXEC
		"Illegal byte sequence",	// 84: EILSEQ
		"Interrupted system call should be restarted",	// 85: ERESTART
		"Streams pipe error",	// 86: ESTRPIPE
		"Too many users",	// 87: EUSERS
		"Socket operation on non-socket",	// 88: ENOTSOCK
		"Destination address required",	// 89: EDESTADDRREQ
		"Message too long",	// 90: EMSGSIZE
		"Protocol wrong type for socket",	// 91: EPROTOTYPE
		"Protocol not available",	// 92: ENOPROTOOPT
		"Protocol not supported",	// 93: EPROTONOSUPPORT
		"Socket type not supported",	// 94: ESOCKTNOSUPPORT
		"Operation not supported on transport endpoint",	// 95: EOPNOTSUPP
		"Protocol family not supported",	// 96: EPFNOSUPPORT
		"Address family not supported by protocol",	// 97: EAFNOSUPPORT
		"Address already in use",	// 98: EADDRINUSE
		"Cannot assign requested address",	// 99: EADDRNOTAVAIL
		"Network is down",	// 100: ENETDOWN
		"Network is unreachable",	// 101: ENETUNREACH
		"Network dropped connection because of reset",	// 102: ENETRESET
		"Software caused connection abort",	// 103: ECONNABORTED
		"Connection reset by peer",	// 104: ECONNRESET
		"No buffer space available",	// 105: ENOBUFS
		"Transport endpoint is already connected",	// 106: EISCONN
		"Transport endpoint is not connected",	// 107: ENOTCONN
		"Cannot send after transport endpoint shutdown",	// 108: ESHUTDOWN
		"Too many references: cannot splice",	// 109: ETOOMANYREFS
		"Connection timed out",	// 110: ETIMEDOUT
		"Connection refused",	// 111: ECONNREFUSED
		"Host is down",	// 112: EHOSTDOWN
		"No route to host",	// 113: EHOSTUNREACH
		"Operation already in progress",	// 114: EALREADY
		"Operation now in progress",	// 115: EINPROGRESS
		"Stale NFS file handle",	// 116: ESTALE
		"Structure needs cleaning",	// 117: EUCLEAN
		"Not a XENIX named type file",	// 118: ENOTNAM
		"No XENIX semaphores available",	// 119: ENAVAIL
		"Is a named type file",	// 120: EISNAM
		"Remote I/O error",	// 121: EREMOTEIO
		"Quota exceeded",	// 122: EDQUOT
		"No medium found",	// 123: ENOMEDIUM
		"Wrong medium type",	// 124: EMEDIUMTYPE
		"Operation Canceled",	// 125: ECANCELED
		"Required key not available",	// 126: ENOKEY
		"Key has expired",	// 127: EKEYEXPIRED
		"Key has been revoked",	// 128: EKEYREVOKED
		"Key was rejected by service",	// 129: EKEYREJECTED
		/* for robust mutexes */
		"Owner died",	// 130: EOWNERDEAD
		"State not recoverable",	// 131: ENOTRECOVERABLE
		"ERFKILL",
        "EHWPOISON",
    };
    
    [ThreadStatic]
    private static int __errno;
    [ThreadStatic]
    private static __obj_holder? __error_message;

    // int* __errno_location();
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static unsafe int* __errno_location()
    {
        fixed (int* p = &__errno)
        {
            return p;
        }
    }

    // int errno;
    public static int errno
    {
        get => __errno;
        set => __errno = value;
    }

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static void __set_exception_to_errno(Exception ex) =>
        __errno = Environment.OSVersion.Platform == PlatformID.Win32NT ?
	        ex switch
	        {
	            ArgumentException _ => data.EINVAL,
	            ArithmeticException _ => data.ERANGE,
	            OutOfMemoryException _ => data.ENOMEM,
	            PathTooLongException _ => data.ENAMETOOLONG,
	            FileNotFoundException _ => data.ENOENT,
	            DirectoryNotFoundException _ => data.ENOENT,
	            UnauthorizedAccessException _ => data.EPERM,
	            IOException _ => data.EIO,
	            _ => Marshal.GetHRForException(ex),
	        } :
	        Marshal.GetHRForException(ex);

    // char *strerror(int errnum);
    public static unsafe sbyte* strerror(int errnum)
    {
	    __error_message = (uint)errnum < __error_messages.Length ?
			__error_messages[errnum] :
			$"Undefined error: errno={errnum}";
        return __error_message;
    }
}
