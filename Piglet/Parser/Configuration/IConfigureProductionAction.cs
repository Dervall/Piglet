using System;

namespace Piglet.Parser.Configuration
{
    public interface IConfigureProductionAction<T>
    {
        void OnReduce(Func<T[], T> action);
    }
}