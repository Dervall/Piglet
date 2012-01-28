using System;

namespace Piglet.Configuration
{
    public interface IConfigureProductionAction<T>
    {
        void OnReduce(Func<T[], T> action);
    }
}