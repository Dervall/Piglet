using System;

namespace Piglet.Parser.Configuration
{
    public class ParserConfigurationException : Exception
    {
        public ParserConfigurationException(string message)
            : base(message)
        {   
        }
    }
}