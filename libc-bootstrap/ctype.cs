/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

namespace C;

public static partial class text
{
    public static sbyte tolower(sbyte c) =>
        (c >= 0x41 && c <= 0x5a) ? (sbyte)(c + 0x20) : c;

    // int isspace(int c);
    public static int isspace(int c)
    {
        switch (c)
        {
            case 0x20:   // ' '
            case 0x0c:   // \f
            case 0x0a:   // \n
            case 0x0d:   // \r
            case 0x09:   // \t
            case 0x0b:   // \v
                return 1;
        }
        return 0;
    }

    // int isdigit(int c);
    public static int isdigit(int c) =>
        // 0 - 9
        (c >= 0x30 && c <= 0x39) ? 1 : 0;

    // int isxdigit(int c);
    public static int isxdigit(int c) =>
        // 0 - 9 || A - F || a - f
        ((c >= 0x30 && c <= 0x39) || (c >= 0x41 && c <= 0x46) || (c >= 0x61 && c <= 0x66)) ? 1 : 0;

    // int ispunct(int c);
    public static int ispunct(int c) =>
        ((isprint(c) != 0) && (isspace(c) == 0) && (isalnum(c) == 0)) ? 1 : 0;

    // int isprint(int c);
    public static int isprint(int c) =>
        (c >= 0x20 && c <= 0x7e) ? 1 : 0;

    // int isalnum(int c);
    public static int isalnum(int c) =>
        ((isalpha(c) != 0) || (isdigit(c) != 0)) ? 1 : 0;

    // int isalpha(int c);
    public static int isalpha(int c) =>
        ((isupper(c) != 0) || (islower(c) != 0)) ? 1 : 0;

    // int isupper(int c);
    public static int isupper(int c) =>
        // A - Z
        (c >= 0x41 && c <= 0x5a) ? 1 : 0;

    // int islower(int c);
    public static int islower(int c) =>
        // a - z
        (c >= 0x61 && c <= 0x7a) ? 1 : 0;
}
