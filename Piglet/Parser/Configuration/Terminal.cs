using System;

namespace Piglet.Parser.Configuration
{
    internal class Terminal<T> : Symbol<T>, ITerminal<T>
    {
        // A epsilon symbol is useful for first and follow set calculation. This symbol
        // is purposely made illegal and will kill the parser generator if it makes it to the table
        // generation (which it never will if the code isn't wrong)
        public static readonly Terminal<T> Epsilon = new Terminal<T>("*", null);

        public static readonly Func<string, T> DefaultFunc = f => default(T);
        public string RegExp { get; private set; }
        public Func<string, T> OnParse { get; private set; }

        public Terminal(string regExp, Func<string, T> onParse)
        {
            if (onParse == null)
            {
                onParse = DefaultFunc;
            }

            OnParse = onParse;
            RegExp = regExp;
            DebugName = RegExp;
        }

        public override string ToString()
        {
            return string.Format("{0}{{{1}}} - {2}", DebugName, RegExp, OnParse);
        }
    }
}