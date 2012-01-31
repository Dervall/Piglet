using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Piglet.Lexer.Construction
{
    internal static class PostFixConverter
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
                        if (state.NumAtoms > 1)
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
                        if (!stack.Any())
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

                    case '{':
                        if (state.NumAtoms == 0)
                            throw new Exception("No characters preceeding numbered repetition operator");
                        buffer.Append(inputCharacter);
                        while (true)
                        {
                            if (reader.Peek() == -1)
                                throw new Exception("Unterminated numbered repetition operator");
                            inputCharacter = (char) reader.Read();
                            buffer.Append(inputCharacter);

                            if (inputCharacter == '}')
                                break;
                        }
                        break;

                    case '[':
                        if (state.NumAtoms > 1)
                        {
                            --state.NumAtoms;
                            buffer.Append('&');
                        }
                        // Append to buffer until we find a matching ]
                        buffer.Append('[');
                        int nCharsFound = 0;
                        while (true)
                        {
                            if (reader.Peek() == ']')
                            {
                                if (nCharsFound != 0)
                                    break;
                            }

                            if (reader.Peek() == -1)
                                throw new Exception("Unterminated character class");
                            nCharsFound++;
                            buffer.Append((char)reader.Read());
                        }
                        buffer.Append(']');
                        reader.Read();
                        state.NumAtoms++;
                        break;

                    default:
                        if (state.NumAtoms > 1)
                        {
                            --state.NumAtoms;
                            buffer.Append('&');
                        }
                        if (inputCharacter == '\\')
                        {
                            // Append the escape to the buffer, it will be used by the NFA construction
                            buffer.Append('\\');

                            // Ignore the escape
                            if (reader.Peek() == -1)
                            {
                                throw new Exception("No character following escape character");
                            }
                            inputCharacter = (char)reader.Read();
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
