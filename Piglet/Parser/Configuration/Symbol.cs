namespace Piglet.Parser.Configuration
{
    public class Symbol<T> : ISymbol<T>
    {
        public string DebugName { get; set; }
        public int TokenNumber { get; set; }
    }
}
