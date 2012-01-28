namespace Piglet.Parser.Configuration
{
    public interface IProductionConfigurator<T>
    {
        IConfigureProductionAction<T> Production(params object[] parts);
    }
}