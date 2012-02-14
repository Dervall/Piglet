using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Piglet.Common
{
    public static class SublistSearch
    {
        public static int IndexOf<T>(this IList<T> haystack, IList<T> needle)
        {
            // Stupid implementation
            for (int i = 0; i < haystack.Count(); ++i)
            {
                bool found = true;
                for (int j = 0; j < needle.Count(); ++j)
                {
                    if (!haystack[i + j].Equals(needle[j]))
                    {
                        found = false;
                        break;
                    }
                }

                if (found)
                    return i;
            }

            return -1;
        }
    }
}
