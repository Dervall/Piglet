namespace Piglet
{
    public interface IParser<out T>
    {
        T Parse();
    }
}