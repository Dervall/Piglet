using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Piglet.Parser.Configuration.Fluent
{
#pragma warning disable 1591
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IHideObjectMembers
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        Type GetType();

        [EditorBrowsable(EditorBrowsableState.Never)]
        int GetHashCode();

        
        [EditorBrowsable(EditorBrowsableState.Never)]
        [return: MaybeNull]
        string ToString();

        [EditorBrowsable(EditorBrowsableState.Never)]
        bool Equals(object obj);
    }
#pragma warning restore 1591
}
