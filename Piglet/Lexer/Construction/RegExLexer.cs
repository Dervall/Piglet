using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Piglet.Lexer.Construction
{
    internal class RegExLexer
    {
        private readonly TextReader input;
        private State state;

        private enum State
        {
            Normal,
            NormalEscaped,
            BeginCharacterClass,
            InsideCharacterClass,
            RangeEnd,
            NumberedRepetition
        }

        private class CharacterClassState
        {
            public CharacterClassState()
            {
                ClassChars = new List<char>();
            }

            public IList<char> ClassChars { get; private set; }
            public bool Negated { get; set; }
        }

        private class NumberedRepetitionState
        {
            public NumberedRepetitionState()
            {
                MinRepetitions = -1;
                MaxRepetitions = -1;
                Chars = new List<char>();
            }

            public int MaxRepetitions { get; set; }
            public int MinRepetitions { get; set; }
            public List<char> Chars { get; private set; }
            public int CurrentPart { get; set; }
        }

        public RegExLexer(TextReader input)
        {
            this.input = input;
            state = State.Normal;
        }

        public RegExToken NextToken()
        {
            // These keeps track of classes
            var classState = new CharacterClassState();
            var numberedRepetitionState = new NumberedRepetitionState();
            state = State.Normal;

            while (input.Peek() != -1)
            {
                var c = (char)input.Read();
                
                switch (state)
                {
                    case State.Normal:
                        switch (c)
                        {
                            case '\\':
                                state = State.NormalEscaped;
                                break;
                            case '[':
                                state = State.BeginCharacterClass;
                                break;
                            case '{':
                                state = State.NumberedRepetition;
                                break;

                            case '(':   return new RegExToken { Type = RegExToken.TokenType.OperatorOpenParanthesis };
                            case ')':   return new RegExToken { Type = RegExToken.TokenType.OperatorCloseParanthesis };
                            case '|':   return new RegExToken { Type = RegExToken.TokenType.OperatorOr };
                            case '+':   return new RegExToken { Type = RegExToken.TokenType.OperatorPlus };
                            case '*':   return new RegExToken { Type = RegExToken.TokenType.OperatorMul };
                            case '?':   return new RegExToken { Type = RegExToken.TokenType.OperatorQuestion };
                            case '.':   return new RegExToken { Type = RegExToken.TokenType.Accept, Characters = AllCharactersExceptNull };
                            default:    return new RegExToken { Type = RegExToken.TokenType.Accept, Characters = new[] {c}};
                        }
                        break;

                    case State.NormalEscaped:
                        switch (c)
                        {
                            // Shorthand for [0-9]
                            case 'd':   
                                return new RegExToken {Type = RegExToken.TokenType.Accept, Characters = CharRange('0', '9')};
                            // Shorthand for [^0-9]
                            case 'D':
                                return new RegExToken
                                           {
                                               Type = RegExToken.TokenType.Accept,
                                               Characters = AllCharactersExceptNull.Except(CharRange('0', '9'))
                                           };

                            case 's':
                                return new RegExToken
                                {
                                    Type = RegExToken.TokenType.Accept,
                                    Characters = AllWhitespaceCharacters
                                };

                            case 'S':
                                return new RegExToken
                                {
                                    Type = RegExToken.TokenType.Accept,
                                    Characters = AllCharactersExceptNull.Except(AllWhitespaceCharacters)
                                };

                            case 'w':
                                return new RegExToken
                                {
                                    Type = RegExToken.TokenType.Accept,
                                    Characters = CharRange('0', '9').Union(CharRange('a', 'z')).Union(CharRange('A', 'Z'))
                                };

                            case 'W':
                                return new RegExToken
                                {
                                    Type = RegExToken.TokenType.Accept,
                                    Characters = AllCharactersExceptNull.Except(
                                        CharRange('0', '9').Union(CharRange('a', 'z')).Union(CharRange('A', 'Z')))
                                };

                            case 'n':
                                return new RegExToken { Type = RegExToken.TokenType.Accept, Characters = new[] { '\n' } };

                            case 'r':
                                return new RegExToken { Type = RegExToken.TokenType.Accept, Characters = new[] { '\r' } };

                            case '.':
                            case '*':
                            case '|':
                            case '[':
                            case ']':
                            case '+':
                            case '(':
                            case ')':
                            case '\\':
                            case '{':
                            case '}':
                            case ' ':
                            case '?':
                                return new RegExToken { Type = RegExToken.TokenType.Accept, Characters = new[] { c } };

                            default:
                                throw new LexerConstructionException(string.Format("Unknown escaped character '{0}'", c));
                        }

                    case State.BeginCharacterClass:
                        switch (c)
                        {
                            case '^':
                                if (classState.Negated)
                                {
                                    // If the classstate is ALREADY negated
                                    // Readd the ^ to the expression
                                    classState.ClassChars.Add('^');
                                    state = State.InsideCharacterClass;
                                }
                                classState.Negated = true;
                                break;
                            case '[':
                            case ']':
                            case '-':
                                // This does not break the character class TODO: I THINK!!!
                                classState.ClassChars.Add(c);
                                break;
                            default:
                                classState.ClassChars.Add(c);
                                state = State.InsideCharacterClass;
                                break;
                        }
                        break;

                    case State.InsideCharacterClass:
                        switch (c)
                        {
                            case '-':
                                state = State.RangeEnd;
                                break;
                            case '[':
                                throw new LexerConstructionException("Opening new character class inside an already open one");
                            case ']':
                                // Ending class
                                return new RegExToken
                                           {
                                               Type = RegExToken.TokenType.Accept,
                                               Characters = classState.Negated
                                                                ? AllCharactersExceptNull.Except(classState.ClassChars)
                                                                : classState.ClassChars.Distinct()
                                           };
                            default:
                                classState.ClassChars.Add(c);
                                break;
                        }
                        break;

                    case State.RangeEnd:
                        switch (c)
                        {
                            case ']':
                                // We found the - at the position BEFORE the end of the class
                                // which means we should handle it as a litteral and end the class
                                return new RegExToken
                                {
                                    Type = RegExToken.TokenType.Accept,
                                    Characters = classState.Negated
                                                     ? AllCharactersExceptNull.Except(classState.ClassChars)
                                                     : classState.ClassChars.Distinct()
                                };

                            default:
                                char lastClassChar = classState.ClassChars.Last();
                                char from = lastClassChar < c ? lastClassChar : c;
                                char to = lastClassChar < c ? c : lastClassChar;
                                for (; from <= to; ++from)
                                {
                                    classState.ClassChars.Add(from);
                                }
                                state = State.InsideCharacterClass;
                                break;
                        }
                        break;

                    case State.NumberedRepetition:
                        switch (c)
                        {
                            case '0':   // Is it really OK to start with a 0. It is now.
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                numberedRepetitionState.Chars.Add(c);
                                break;
                            case '}':
                            case ':':
                            case ',':
                                // Parse whatever is in Chars
                                int reps = -1;

                                // Number is required in FIRST part but OPTIONAL in the second
                                if (numberedRepetitionState.Chars.Any() || numberedRepetitionState.CurrentPart == 0)
                                {
                                    if (!int.TryParse(new string(numberedRepetitionState.Chars.ToArray()), out reps))
                                    {
                                        throw new LexerConstructionException("Numbered repetition operator contains operand that is not a number");
                                    }
                                }
                                else
                                {
                                    // End up here when nothing specified in the last part.
                                    // Use the max value to say that it can be infinite numbers.
                                    reps = int.MaxValue;
                                }
                                numberedRepetitionState.Chars.Clear();

                                // Set the right value
                                if (numberedRepetitionState.CurrentPart == 0)
                                {
                                    numberedRepetitionState.MinRepetitions = reps;
                                }
                                else
                                {
                                    numberedRepetitionState.MaxRepetitions = reps;
                                }

                                if (c == ':' || c == ',')
                                {
                                    ++numberedRepetitionState.CurrentPart;
                                    if (numberedRepetitionState.CurrentPart > 1)
                                        throw new LexerConstructionException("More than one , in numbered repetition.");
                                }
                                else
                                {
                                    return new RegExToken
                                    {
                                        Type = RegExToken.TokenType.NumberedRepeat,
                                        MinRepetitions = numberedRepetitionState.MinRepetitions, 
                                        MaxRepetitions = numberedRepetitionState.MaxRepetitions
                                    };
                                }
                                break;
                            default:
                                throw new LexerConstructionException(
                                    string.Format("Illegal character {0} in numbered repetition", c));
                        }
                        break;
                }
            }

            // We get here if we try to lex when the expression has ended.
            return null;
        }

        private static IEnumerable<char> CharRange(char start, char end)
        {
            for (var c = start; c <= end; ++c)
            {
                yield return c;
            }
        }

        protected static IEnumerable<char> AllCharactersExceptNull
        {
            get
            {
                for (var c = (char)1; c < 256; ++c)
                    yield return c;
            }
        }

        protected static IEnumerable<char> AllWhitespaceCharacters
        {
            get
            {
                yield return ' ';
                yield return '\t';
                yield return '\n';
                yield return '\r';            
            }
        }
    }
}
