using System;

namespace Piglet.Parser
{
    /// <summary>
    /// ParseExceptions are thrown when the parser detects an illegal token according to the given
    /// grammar.
    /// </summary>
    public class ParseException : Exception
    {
        /// <summary>
        /// Construct a new Parseexception
        /// </summary>
        /// <param name="message"></param>
        public ParseException(string message)
            : base(message)
        {
        }
    }
}