namespace Piglet.Configuration
{
    public interface ISymbol<T>
    {
        string DebugName { get; set; }
    }
}