using System;

namespace Piglet.Configuration
{
    public class ParserConfigurationException : Exception
    {
        public ParserConfigurationException(string message)
            : base(message)
        {   
        }
    }
}