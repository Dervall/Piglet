using System.Collections.Generic;
using System.Linq;

namespace Piglet.Lexer.Construction
{
    internal sealed class ShuntingYard
    {
        private readonly RegExLexer _lexer;


        public ShuntingYard(RegExLexer lexer) => _lexer = lexer;

        private IEnumerable<RegExToken> TokensWithImplicitConcat()
        {
            RegExToken lastToken = null;

            for (RegExToken token = _lexer.NextToken(); token != null;)
            {
                // If the last token was accept and this new token is also accept we need to insert a concat operator  between the two.
                if (lastToken != null && PreceedingTypeRequiresConcat(lastToken.Type) && NextTypeRequiresConcat(token.Type))
                    yield return new RegExToken { Type = RegExToken.TokenType.OperatorConcat };

                yield return token;

                lastToken = token;
                token = _lexer.NextToken();
            }
        }

        private static bool PreceedingTypeRequiresConcat(RegExToken.TokenType type)
        {
            switch (type)
            {
                case RegExToken.TokenType.OperatorMul:
                case RegExToken.TokenType.OperatorQuestion:
                case RegExToken.TokenType.OperatorPlus:
                case RegExToken.TokenType.Accept:
                case RegExToken.TokenType.OperatorCloseParanthesis:
                case RegExToken.TokenType.NumberedRepeat:
                    return true;
            }

            return false;
        }

        private static bool NextTypeRequiresConcat(RegExToken.TokenType type)
        {
            switch (type)
            {
                case RegExToken.TokenType.Accept:
                case RegExToken.TokenType.OperatorOpenParanthesis:
                    return true;
            }

            return false;
        }

        public IEnumerable<RegExToken> ShuntedTokens()
        {
            Stack<RegExToken> operatorStack = new Stack<RegExToken>();

            foreach (RegExToken token in TokensWithImplicitConcat())
            {
                switch (token.Type)
                {
                    case RegExToken.TokenType.Accept:
                        yield return token;

                        break;
                    case RegExToken.TokenType.OperatorOpenParanthesis:
                        operatorStack.Push(token);

                        break;
                    case RegExToken.TokenType.OperatorCloseParanthesis:
                        while (operatorStack.Any() && operatorStack.Peek().Type != RegExToken.TokenType.OperatorOpenParanthesis)
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
                RegExToken op = operatorStack.Pop();

                if (op.Type == RegExToken.TokenType.OperatorOpenParanthesis)
                    throw new LexerConstructionException("Mismatched parenthesis in regular expression");

                yield return op;
            }
        }
    }
}
