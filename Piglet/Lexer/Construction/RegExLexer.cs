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
            NumberedRepetition,
            InsideCharacterClassEscaped
        }

        private class CharacterClassState
        {
            public CharacterClassState()
            {
                CharsSet = new CharSet();
            }

            public CharSet CharsSet { get; private set; }
            public bool Negated { get; set; }
            public char LastChar { get; set; }
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

        private CharSet EscapedCharToAcceptCharRange(char c)
        {
            switch (c)
            {
                // Shorthand for [0-9]
                case 'd':
                    return CharRange('0', '9');
                // Shorthand for [^0-9]
                case 'D':
                    return AllCharactersExceptNull.Except(CharRange('0', '9'));
                case 's':
                    return AllWhitespaceCharacters;
                case 'S':
                    return AllCharactersExceptNull.Except(AllWhitespaceCharacters);
                case 'w':
                    return CharRange('ä', 'å').Union(CharRange('Ä', 'Å')).Union(CharRange('ö', 'ö'))
                        .Union(CharRange('Ö','Ö')).Union(CharRange('_', '_')).Union(CharRange('a', 'z')).Union(CharRange('A', 'Z'));
                case 'W':
                    return AllCharactersExceptNull.Except(
                        CharRange('0', '9').Union(CharRange('a', 'z')).Union(CharRange('A', 'Z')));
                case 'n':
                    return SingleChar('\n');
                case 'r':
                    return SingleChar('\r');
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
                    return SingleChar(c);
                default:
                    return new CharSet();   // Empty charset, might be added to
            }
        }

        private CharSet SingleChar(char c)
        {
            var cs = new CharSet();
            cs.Add(c);
            return cs;
        }

        private CharSet EscapedCharToAcceptCharsInClass(char c)
        {
            // There are some additional escapeable characters for a character class
            switch (c)
            {
                case '-':
                case '^':
                    return SingleChar(c);

            }
            return EscapedCharToAcceptCharRange(c);
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
                            default:    return new RegExToken { Type = RegExToken.TokenType.Accept, Characters = SingleChar(c)};
                        }
                        break;

                    case State.NormalEscaped:
                        {
                            var characters = EscapedCharToAcceptCharRange(c);
                            if (!characters.Any())
                            {
                                throw new LexerConstructionException(string.Format("Unknown escaped character '{0}'", c));
                            }
                            return new RegExToken {Characters = characters, Type = RegExToken.TokenType.Accept};
                        }

                    case State.BeginCharacterClass:
                        switch (c)
                        {
                            case '^':
                                if (classState.Negated)
                                {
                                    // If the classstate is ALREADY negated
                                    // Readd the ^ to the expression
                                    classState.LastChar = '^';
                                    state = State.InsideCharacterClass;
                                }
                                classState.Negated = true;
                                break;
                            case '[':
                            case ']':
                            case '-':
                                // This does not break the character class TODO: I THINK!!!
                                classState.LastChar = c;
                                break;
                            case '\\':
                                state = State.InsideCharacterClassEscaped;
                                break;
                            default:
                                classState.LastChar = c;
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
                                if (classState.LastChar != (char)0) 
                                    classState.CharsSet.Add(classState.LastChar);

                                // Ending class
                                return new RegExToken
                                           {
                                               Type = RegExToken.TokenType.Accept,
                                               Characters = classState.Negated
                                                                ? AllCharactersExceptNull.Except(classState.CharsSet)
                                                                : classState.CharsSet
                                           };
                            case '\\':
                                state = State.InsideCharacterClassEscaped;
                                break;
                            default:
                                if (classState.LastChar != 0)
                                    classState.CharsSet.Add(classState.LastChar);
                                classState.LastChar = c;
                                break;
                        }
                        break;

                    case State.InsideCharacterClassEscaped:
                        {
                            var characters = EscapedCharToAcceptCharsInClass(c);
                            if (!characters.Any())
                            {
                                throw new LexerConstructionException(string.Format("Unknown escaped character '{0}' in character class", c));
                            }

                            if (classState.LastChar != 0)
                                classState.CharsSet.Add(classState.LastChar);

                            classState.CharsSet.UnionWith(characters);
                            classState.LastChar = (char)0;
                            state = State.InsideCharacterClass;
                        }
                        break;


                    case State.RangeEnd:
                        switch (c)
                        {
                            case ']':
                                // We found the - at the position BEFORE the end of the class
                                // which means we should handle it as a litteral and end the class
                                classState.CharsSet.Add(classState.LastChar);
                                classState.CharsSet.Add('-');

                                return new RegExToken
                                {
                                    Type = RegExToken.TokenType.Accept,
                                    Characters = classState.Negated
                                                     ? AllCharactersExceptNull.Except(classState.CharsSet)
                                                     : classState.CharsSet
                                };

                            default:
                                char lastClassChar = classState.LastChar;
                                char from = lastClassChar < c ? lastClassChar : c;
                                char to = lastClassChar < c ? c : lastClassChar;
                                classState.CharsSet.AddRange(from, to);
                                classState.LastChar = (char) 0;
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
                                int reps;

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

        private static CharSet CharRange(char start, char end)
        {
            var charRange = new CharSet();
            charRange.AddRange(start, end);
            return charRange;
        }

        protected static CharSet AllCharactersExceptNull
        {
            get
            {
                return CharRange((char) 1, char.MaxValue);                
            }
        }

        protected static CharSet AllWhitespaceCharacters
        {
            get
            {
                var cs = new CharSet();
                cs.Add(' ');
                cs.Add('\t');
                cs.Add('\n');
                cs.Add('\r');
                return cs;
            }
        }
    }
}
