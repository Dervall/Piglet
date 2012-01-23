using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Piglet
{
    public class Symbol<T> : ISymbol<T>
    {
        public string DebugName { get; set; }
    }
}
