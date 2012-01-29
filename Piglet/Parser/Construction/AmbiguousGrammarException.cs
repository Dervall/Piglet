using Piglet.Parser.Configuration;

namespace Piglet.Parser.Construction
{
    public class AmbiguousGrammarException : ParserConfigurationException
    {
        public AmbiguousGrammarException(string message)
            : base (message)
        {
        }

        public int StateNumber { get; set; }
        public int TokenNumber { get; set; }
        public int PreviousValue { get; set; }
        public int NewValue { get; set; }
    }
}