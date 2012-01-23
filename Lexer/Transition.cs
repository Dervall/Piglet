using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Piglet.Lexer
{
    public class Transition<TState>
    {
        public Transition(TState from, char onCharacter, TState to)
        {
            From = from;
            OnCharacter = onCharacter;
            To = to;
        }

        public TState From { get; set; }
        public TState To { get; set; }
        public char OnCharacter { get; set; }

        public override string ToString()
        {
            return string.Format("{0} ={1}=> {2}", From, OnCharacter == '\0' ? 'ε' : OnCharacter, To);
        }
    }
}
