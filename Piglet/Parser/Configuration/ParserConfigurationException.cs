using System;

namespace Piglet.Parser.Configuration
{
    /// <summary>
    /// This exception is thrown for illegal parser configurations
    /// </summary>
    public class ParserConfigurationException : Exception
    {
        /// <summary>
        /// Construct a new parser configuration exception
        /// </summary>
        /// <param name="message">message to show user</param>
        public ParserConfigurationException(string message)
            : base(message)
        {   
        }
    }
}