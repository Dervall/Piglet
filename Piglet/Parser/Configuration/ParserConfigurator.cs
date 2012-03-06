using System;
using System.Collections.Generic;
using System.Linq;
using Piglet.Lexer;
using Piglet.Parser.Construction;

namespace Piglet.Parser.Configuration
{
    internal class ParserConfigurator<T> : IParserConfigurator<T>, IGrammar<T>
    {
        private NonTerminal<T> startSymbol;
        private readonly List<NonTerminal<T>> nonTerminals;
        private readonly List<Terminal<T>> terminals;
        private readonly ILexerSettings lexerSettings;
        private readonly List<TokenPrecedence> tokenPrecedences;
        private int currentPrecedence;

        private class TokenPrecedence : ITokenPrecedence
        {
            public AssociativityDirection Associativity { get; set; }
            public Terminal<T> Terminal { get; set; }
            public int Precedence { get; set; }
        }

        public ParserConfigurator()
        {
            nonTerminals = new List<NonTerminal<T>>();
            terminals = new List<Terminal<T>>();
            lexerSettings = new LexerSettingsImpl();
            tokenPrecedences = new List<TokenPrecedence>();
            currentPrecedence = 0;
        
            // Set some default settings
            LexerSettings.CreateLexer = true;
            LexerSettings.EscapeLiterals = true;
            LexerSettings.Ignore = new[] { "\\s+" };     // Ignore all whitespace by default
        }

        private class LexerSettingsImpl : ILexerSettings
        {
            public bool CreateLexer { get; set; }
            public bool EscapeLiterals { get; set; }
            public string[] Ignore { get; set; }
        }

        public ITerminal<T> Terminal(string regExp, Func<string, T> onParse = null)
        {
            Terminal<T> terminal = terminals.SingleOrDefault(f => f.RegExp == regExp);
            if (terminal != null)
            {
                if (terminal.OnParse != (onParse??Terminal<T>.DefaultFunc))
                    throw new ParserConfigurationException(
                        "Redefinition of terminal uses the same regex but different onParse action");
            }
            else
            {
                terminal = new Terminal<T>(regExp, onParse);
                terminals.Add(terminal);
            }
            return terminal;
        }

        public INonTerminal<T> NonTerminal(Action<IProductionConfigurator<T>> productionAction = null)
        {
            var nonTerminal = new NonTerminal<T>(this, productionAction);
            nonTerminals.Add(nonTerminal);
            
            if (startSymbol == null)
            {
                // First symbol to be created is the start symbol
                SetStartSymbol(nonTerminal);
            }
            return nonTerminal;
        }

        public ILexerSettings LexerSettings
        {
            get { return lexerSettings; }
        }

        public void NonAssociative(params ITerminal<T>[] symbols)
        {
            SetSymbolAssociativity(symbols, AssociativityDirection.NonAssociative);
        }

        public void RightAssociative(params ITerminal<T>[] symbols)
        {
            SetSymbolAssociativity(symbols, AssociativityDirection.Right);
        }

        public void LeftAssociative(params ITerminal<T>[] symbols)
        {
            SetSymbolAssociativity(symbols, AssociativityDirection.Left);
        }

        private void SetSymbolAssociativity(IEnumerable<ITerminal<T>> symbols, AssociativityDirection associativityDirection)
        {
            foreach (var terminal in symbols.OfType<Terminal<T>>())
            {
                if (tokenPrecedences.Any( f => f.Terminal == terminal))
                {
                    // This terminal is defined multiple times
                    throw new ParserConfigurationException(
                        string.Format("Terminal {0} has been declared to have a precedence multiple times",
                                      terminal.DebugName));
                }

                tokenPrecedences.Add(new TokenPrecedence
                                         {
                                             Associativity = associativityDirection,
                                             Terminal = terminal,
                                             Precedence = currentPrecedence
                                         });
            }
            ++currentPrecedence;
        }

        public void SetStartSymbol(INonTerminal<T> start)
        {
            startSymbol = (NonTerminal<T>) start;
        }

        public void AugmentGrammar()
        {
            // First we need to augment the grammar with a start rule and a new start symbol
            // Create the derived start symbol
            var augmentedStart = (NonTerminal<T>)NonTerminal();  // Unfortunate cast...

            // Use the start symbols debug name with a ' in front to indicate the augmented symbol.
            augmentedStart.DebugName = "'" + startSymbol.DebugName;

            // Create a single production 
            augmentedStart.Productions(p => p.AddProduction(startSymbol)); // This production is never reduced, parser accepts when its about to reduce. No reduce action.
            Start = augmentedStart.ProductionRules.First(); // There's only one production.

            // Make sure all the terminals are registered.
            // This becomes neccessary since the user can configure the parser using only strings.
            // Since the nonterminal used for that does not carry a back-reference to the configurator,
            // we do it this way.
            foreach (var nonTerminal in nonTerminals)
            {
                foreach (var terminal in nonTerminal.ProductionRules.SelectMany(f => f.Symbols).OfType<Terminal<T>>())
                {
                    var oldTerminal = terminals.SingleOrDefault(f => f.RegExp == terminal.RegExp);
                    if (oldTerminal != null)
                    {
                        if (oldTerminal.OnParse != terminal.OnParse)
                        {
                            throw new ParserConfigurationException(
                                "Multiply defined terminal has more than one OnParse action");
                        }
                    }
                    else
                    {
                        terminals.Add(terminal);
                    }
                }
            }

            // Add the end of input symbol
            var eoi = Terminal(null, s => default(T));
            eoi.DebugName = "$";

            // Assign all tokens in the grammar token numbers!
            AssignTokenNumbers();

            // This class is now a valid implementation of IGrammar, ready to use.
        }

        public ILexer<T> CreateLexer()
        {
            // User wants a default lexer, great. Use the lexer from grammar factory
            // to fix him up
            return LexerFactory<T>.ConfigureFromGrammar(this, LexerSettings);
        }

        public IParser<T> CreateParser()
        {
            if (Start == null)
            {
                // User has forgotten to augment the grammar. Lets help him out and do it
                // for him
                AugmentGrammar();
            }

            var parser = new ParserBuilder<T>(this).CreateParser();

            // If our lexer settings says that we are supposed to create a lexer, do so now and assign
            // the lexer to the created parser.
            if (LexerSettings.CreateLexer)
            {
                parser.Lexer = CreateLexer();
            }

            return parser;
        }

        public IProductionRule<T> Start { get; private set; }

        public IEnumerable<IProductionRule<T>> ProductionRules
        {
            get { return nonTerminals.SelectMany(nonTerminal => nonTerminal.ProductionRules); }
        }

        public IEnumerable<ISymbol<T>> AllSymbols
        {
            get
            {
                foreach (var terminal in terminals)
                {
                    yield return terminal;
                }
 
                foreach (var nonTerminal in nonTerminals)
                {
                    yield return nonTerminal;
                }
            }
        }

        public NonTerminal<T> AcceptSymbol
        {
            get { return (NonTerminal<T>)Start.ResultSymbol; }
        }

        public Terminal<T> EndOfInputTerminal
        {
            get { return terminals.Single(f => f.RegExp == null); }
        }

        public ITokenPrecedence GetPrecedence(ITerminal<T> terminal)
        {
            return tokenPrecedences.FirstOrDefault(f => f.Terminal == terminal);
        }

        private void AssignTokenNumbers()
        {
            int t = 0;
            foreach (var symbol in AllSymbols)
            {
                ((Symbol<T>)symbol).TokenNumber = t++;
            }
        }
    }
}
