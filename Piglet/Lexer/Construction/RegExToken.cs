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

		private static readonly Dictionary<TokenType, int> precedences = new Dictionary<TokenType, int> {
            			{TokenType.OperatorPlus, 3}, 
						{TokenType.OperatorMul, 3}, 
						{TokenType.OperatorQuestion, 3}, 
						{TokenType.NumberedRepeat, 3}, 
						{TokenType.OperatorConcat, 2}, 
						{TokenType.OperatorOr, 1}, 
						{TokenType.OperatorOpenParanthesis, 0}
            		};

        public TokenType Type { get; set; }

        public CharSet Characters { get; set; }

        public int MinRepetitions { get; set; }

        public int MaxRepetitions { get; set; }

        public int Precedence
        {
            get { return precedences[Type]; }
        }
    }
}