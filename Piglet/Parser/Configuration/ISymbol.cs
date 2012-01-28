namespace Piglet.Parser.Configuration
{
    public interface ISymbol<T>
    {
        string DebugName { get; set; }
        int TokenNumber { get; set; }
    }
}