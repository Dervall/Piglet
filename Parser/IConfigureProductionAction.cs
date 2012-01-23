using System;

namespace Piglet
{
    public interface IConfigureProductionAction<T>
    {
        void OnReduce(Func<T[], T> action);
    }
}