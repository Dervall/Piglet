using System.Collections.Generic;

namespace Piglet.Lexer.Construction
{
    internal sealed class RegexToken
    {
        private static readonly Dictionary<RegexTokenType, int> _precedences = new Dictionary<RegexTokenType, int>
        {
            [RegexTokenType.OperatorPlus] = 3,
            [RegexTokenType.OperatorMul] = 3,
            [RegexTokenType.OperatorQuestion] = 3,
            [RegexTokenType.NumberedRepeat] = 3,
            [RegexTokenType.OperatorConcat] = 2,
            [RegexTokenType.OperatorOr] = 1,
            [RegexTokenType.OperatorOpenParanthesis] = 0,
        };

        public RegexTokenType Type { get; set; }
        public CharSet Characters { get; set; }
        public int MinRepetitions { get; set; }
        public int MaxRepetitions { get; set; }
        public int Precedence => _precedences[Type];
    }

    internal enum RegexTokenType
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
}