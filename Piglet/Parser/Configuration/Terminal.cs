using System;

namespace Piglet.Parser.Configuration
{
    internal sealed class Terminal<T>
        : Symbol<T>
        , ITerminal<T>
    {
        public string? Regex { get; private set; }
        public Func<string, T> OnParse { get; private set; }

        public static readonly Func<string, T> DefaultFunc = f => default;


        public Terminal(string? regex, Func<string, T>? onParse)
        {
            OnParse = onParse ?? DefaultFunc;
            Regex = DebugName = regex;
        }

        public override string ToString() => $"{DebugName} {{{Regex}}}";
    }
}