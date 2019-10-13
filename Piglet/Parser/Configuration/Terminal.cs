using System;

namespace Piglet.Parser.Configuration
{
    internal class Terminal<T>
        : Symbol<T>
        , ITerminal<T>
    {
        public static readonly Func<string, T> DefaultFunc = f => default;

        public string RegExp { get; private set; }
        public Func<string, T> OnParse { get; private set; }

        public Terminal(string regExp, Func<string, T> onParse)
        {
            OnParse = onParse ?? DefaultFunc;
            RegExp = DebugName = regExp;
        }

        public override string ToString() => $"{DebugName} {{{RegExp}}}";
    }
}