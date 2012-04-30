using System;
using System.Collections.Generic;
using System.Linq;

namespace Piglet.Lexer.Construction
{
    internal class RegExToken
    {       
        internal enum TokenType
        {
            OperatorOr,
            OperatorPlus,
            OperatorMul,
            OperatorQuestion,
            Accept,
            NumberedRepeat,
            OperatorOpenParanthesis,
            OperatorCloseParanthesis,
            OperatorConcat
        }

        public TokenType Type { get; set; }

        public CharSet Characters { get; set; }

        public int MinRepetitions { get; set; }

        public int MaxRepetitions { get; set; }

        public int Precedence
        {
            get
            {
                return new[]
                           {
                               new Tuple<TokenType, int>(TokenType.OperatorPlus, 3),
                               new Tuple<TokenType, int>(TokenType.OperatorMul, 3),
                               new Tuple<TokenType, int>(TokenType.OperatorQuestion, 3),
                               new Tuple<TokenType, int>(TokenType.NumberedRepeat, 3),
                               new Tuple<TokenType, int>(TokenType.OperatorConcat, 2),
                               new Tuple<TokenType, int>(TokenType.OperatorOr, 1),
                               new Tuple<TokenType, int>(TokenType.OperatorOpenParanthesis, 0)
                           }.First(f => f.Item1 == Type).Item2;
            }
        }
    }
}