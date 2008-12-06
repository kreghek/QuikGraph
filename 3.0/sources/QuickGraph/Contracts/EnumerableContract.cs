﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.Contracts;
using System.Diagnostics;

namespace QuickGraph
{
    public static class EnumerableContract
    {
        [Pure]
        public static bool ElementsNotNull<T>(IEnumerable<T> elements)
        {
            Contract.Requires(elements != null);
#if DEBUG

            return Contract.ForAll(elements, e => e != null);
#else
            return true;
#endif
        }
    }
}