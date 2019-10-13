using System.Collections.Generic;

namespace Piglet.Common
{
    /// <summary>
    /// Extension methods for finding the index of a sublist
    /// </summary>
    public static class SublistSearch
    {
        /// <summary>
        /// Get the index of the a sublist
        /// </summary>
        /// <typeparam name="T">Type of list</typeparam>
        /// <param name="haystack">List to search</param>
        /// <param name="needle">List to find</param>
        /// <returns>Index of start of sublist, or -1 if not found</returns>
        public static int IndexOf<T>(this IList<T> haystack, IList<T> needle)
        {
            // Stupid implementation. This could probably benefit from using a string search algorithm.
            for (int i = 0; i < haystack.Count - needle.Count; ++i)
            {
                bool found = true;

                for (int j = 0; j < needle.Count; ++j)
                    if (!Equals(haystack[i + j], needle[j]))
                    {
                        found = false;

                        break;
                    }

                if (found)
                    return i;
            }

            return -1;
        }
    }
}