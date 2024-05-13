/////////////////////////////////////////////////////////////////////////////////////
//
// libc-cil - libc implementation on CIL, part of chibicc-cil
// Copyright (c) Kouji Matsui(@kozy_kekyo, @kekyo @mastodon.cloud)
//
// Licensed under MIT: https://opensource.org/licenses/MIT
//
/////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace C
{
    namespace type
    {
        public unsafe struct glob_t
        {
            public nuint gl_pathc;
            public sbyte** gl_pathv;
            public nuint gl_offs;
        }
    }

    public static partial class data
    {
        public const int GLOB_NOSPACE = 1;        /* Ran out of memory.  */
        public const int GLOB_ABORTED = 2;        /* Read error.  */
        public const int GLOB_NOMATCH = 3;        /* No matches found.  */
        public const int GLOB_NOSYS = 4;          /* Not implemented.  */
    }
    
    public static partial class text
    {
        private static readonly char[] __directory_separators =
            Environment.OSVersion.Platform == PlatformID.Win32NT ?
                new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, } :
                new[] { Path.DirectorySeparatorChar, };
        
        // int glob(const char *pattern, int flags,
        //   int (*errfunc) (const char *epath, int eerrno),
        //   glob_t *pglob);
        public static unsafe int glob(sbyte* pattern, int flags,
            delegate*<sbyte*, int, int> errfunc,
            type.glob_t* pglob)
        {
            // HACK: This is junkie implementation.
            try
            {
                var pt = __ngetstr(pattern)!;
                var fullPath = Path.GetFullPath(pt);
                var elements = fullPath.Split(__directory_separators);
                if (elements[0].Length == 0)
                {
                    elements[0] = Path.DirectorySeparatorChar.ToString();
                }

                static void dig(
                    string basePath, string[] elements, int index,
                    List<string> results)
                {
                    var element = elements[index];
                    if (index >= (elements.Length - 1))
                    {
                        var files = Directory.GetFiles(
                            basePath, element, SearchOption.TopDirectoryOnly);
                        results.AddRange(files);
                    }
                    else
                    {
                        foreach (var path in Directory.GetDirectories(
                            basePath, element, SearchOption.TopDirectoryOnly))
                        {
                            dig(path, elements, index + 1, results);
                        }
                    }
                }

                var results = new List<string>();
                dig(elements[0], elements, 1, results);
                if (results.Count >= 1)
                {
                    pglob->gl_offs = 0;
                    pglob->gl_pathc = (nuint)results.Count;
                    pglob->gl_pathv = (sbyte**)heap.calloc(
                        pglob->gl_pathc + 1, (nuint)sizeof(sbyte**), null, 0);
                    for (var index = 0; index < results.Count; index++)
                    {
                        pglob->gl_pathv[index] = __nstrdup(results[index]);
                    }
                    return 0;
                }
                else
                {
                    return data.GLOB_NOMATCH;
                }
            }
            catch (Exception ex)
            {
                __set_exception_to_errno(ex);
                return data.GLOB_ABORTED;
            }
        }
        
        // void globfree(glob_t *pglob);
        public static unsafe void globfree(type.glob_t* pglob)
        {
            for (var index = (nuint)0; index < pglob->gl_pathc; index++)
            {
                heap.free(pglob->gl_pathv[index], null, 0);
                pglob->gl_pathv[index] = null;
            }
            heap.free(pglob->gl_pathv, null, 0);
            pglob->gl_pathv = null;
        }
    }
}
