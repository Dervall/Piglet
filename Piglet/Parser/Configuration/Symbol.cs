namespace Piglet.Parser.Configuration
{
    internal class Symbol<T>
        : ISymbol<T>
    {
        public string? DebugName { get; set; }
        public int TokenNumber { get; set; }
    }
}
