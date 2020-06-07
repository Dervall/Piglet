using System.Collections.Generic;
using System.Linq;

namespace Piglet.Lexer.Construction
{
    internal sealed class ShuntingYard
    {
        private readonly RegexLexer _lexer;
        private readonly bool _ignorecase;


        public ShuntingYard(RegexLexer lexer, bool ignorecase)
        {
            _lexer = lexer;
            _ignorecase = ignorecase;
        }

        private IEnumerable<RegexToken> TokensWithImplicitConcat()
        {
            RegexToken? lastToken = null;

            for (RegexToken? token = _lexer.NextToken(_ignorecase); token is { };)
            {
                // If the last token was accept and this new token is also accept we need to insert a concat operator  between the two.
                if (lastToken != null && PreceedingTypeRequiresConcat(lastToken.Type) && NextTypeRequiresConcat(token.Type))
                    yield return new RegexToken { Type = RegexTokenType.OperatorConcat };

                yield return token;

                lastToken = token;
                token = _lexer.NextToken(_ignorecase);
            }
        }

        private static bool PreceedingTypeRequiresConcat(RegexTokenType type)
        {
            switch (type)
            {
                case RegexTokenType.OperatorMul:
                case RegexTokenType.OperatorQuestion:
                case RegexTokenType.OperatorPlus:
                case RegexTokenType.Accept:
                case RegexTokenType.OperatorCloseParanthesis:
                case RegexTokenType.NumberedRepeat:
                    return true;
            }

            return false;
        }

        private static bool NextTypeRequiresConcat(RegexTokenType type)
        {
            switch (type)
            {
                case RegexTokenType.Accept:
                case RegexTokenType.OperatorOpenParanthesis:
                    return true;
            }

            return false;
        }

        public IEnumerable<RegexToken> ShuntedTokens()
        {
            Stack<RegexToken> operatorStack = new Stack<RegexToken>();

            foreach (RegexToken token in TokensWithImplicitConcat())
            {
                switch (token.Type)
                {
                    case RegexTokenType.Accept:
                        yield return token;

                        break;
                    case RegexTokenType.OperatorOpenParanthesis:
                        operatorStack.Push(token);

                        break;
                    case RegexTokenType.OperatorCloseParanthesis:
                        while (operatorStack.Any() && operatorStack.Peek().Type != RegexTokenType.OperatorOpenParanthesis)
                            yield return operatorStack.Pop();

                        if (!operatorStack.Any())
                            // Mismatched parenthesis
                            throw new LexerConstructionException("Mismatched parenthesis in regular expression");

                        operatorStack.Pop();

                        break;
                    default:
                        while (operatorStack.Any() && token.Precedence <= operatorStack.Peek().Precedence)
                            yield return operatorStack.Pop();

                        operatorStack.Push(token);

                        break;
                }
            }

            while (operatorStack.Any())
            {
                RegexToken op = operatorStack.Pop();

                if (op.Type == RegexTokenType.OperatorOpenParanthesis)
                    throw new LexerConstructionException("Mismatched parenthesis in regular expression");

                yield return op;
            }
        }
    }
}