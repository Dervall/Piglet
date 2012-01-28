using System;

namespace Piglet.Parser
{
    public class ParseException : Exception
    {
        public ParseException(string message)
            : base(message)
        {
        }
    }
}