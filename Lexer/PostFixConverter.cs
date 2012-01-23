using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Piglet.Lexer
{
    public static class PostFixConverter
    {
        private struct ScopeState
        {
            public int NumAlternates;
            public int NumAtoms;
        }

        public static string ToPostFix(string regExp)
        {
            var state = new ScopeState();
            
            var buffer = new StringBuilder();
            var reader = new StringReader(regExp);

            var stack = new Stack<ScopeState>();
            while (reader.Peek() != -1)
            {
                var inputCharacter = (char)reader.Read();
                switch (inputCharacter)
                {
                    case '(':
                        if (state.NumAtoms> 1)
                        {
                            --state.NumAtoms;
                            buffer.Append('&');
                        }

                        stack.Push(state);

                        state = new ScopeState();
                        break;
                    case '|':
                        if (state.NumAtoms == 0)
                            return null;
                        while (--state.NumAtoms > 0)
                            buffer.Append('&');

                        state.NumAlternates++;
                        break;
                    case ')':
                        if (stack.Count() == 0)
                            return null;
                        if (state.NumAtoms == 0)
                            return null;
                        while (--state.NumAtoms > 0)
                            buffer.Append('&');
                        for (; state.NumAlternates > 0; state.NumAlternates--)
                            buffer.Append('|');

                        state = stack.Pop();
                        state.NumAtoms++;
                        break;

                    case '*':
                    case '+':
                    case '?':
                        if (state.NumAtoms == 0)
                            return null;
                        buffer.Append(inputCharacter);
                        break;

                    default:
                        if (state.NumAtoms > 1)
                        {
                            --state.NumAtoms;
                            buffer.Append('&');
                        }
                        buffer.Append(inputCharacter);
                        state.NumAtoms++;
                        break;
                }
            }

            if (stack.Count() != 0)
                return null;
            while (--state.NumAtoms > 0)
                buffer.Append('&');
            for (; state.NumAlternates > 0; state.NumAlternates--)
                buffer.Append('|');

            return buffer.ToString();

        }
    }
}
