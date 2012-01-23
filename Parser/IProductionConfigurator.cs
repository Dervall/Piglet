namespace Piglet
{
    public interface IProductionConfigurator<T>
    {
        IConfigureProductionAction<T> Production(params object[] parts);
    }
}