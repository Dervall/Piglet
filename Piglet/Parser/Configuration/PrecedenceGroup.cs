using Piglet.Parser.Construction;

namespace Piglet.Parser.Configuration
{
    internal class PrecedenceGroup : IPrecedenceGroup
    {
        public AssociativityDirection Associativity { get; set; }
        public int Precedence { get; set; }
    }
}