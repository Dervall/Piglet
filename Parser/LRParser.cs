using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Piglet
{
    public class LRParser<T> : IParser<T>
    {
        public T Parse()
        {
            return default(T);
        }
    }
}
