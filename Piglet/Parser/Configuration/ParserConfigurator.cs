using System.Collections.Generic;
using System.Linq;
using System;

using Piglet.Lexer.Configuration;
using Piglet.Parser.Construction;
using Piglet.Lexer;

namespace Piglet.Parser.Configuration
{
    internal sealed class ParserConfigurator<T>
        : IParserConfigurator<T>
        , IGrammar<T>
    {
        private NonTerminal<T> _startSymbol;
        private readonly List<NonTerminal<T>> _nonTerminals;
        private readonly LinkedList<Terminal<T>> _terminals;
        private readonly ILexerSettings lexerSettings;
        private readonly List<TerminalPrecedence> _terminalPrecedences;
        private int _currentPrecedence;


        public IProductionRule<T> Start { get; private set; }

        public ILexerSettings LexerSettings => lexerSettings;

        public ITerminal<T> ErrorToken { get; set; }

        public IEnumerable<IProductionRule<T>> ProductionRules => _nonTerminals.SelectMany(nonTerminal => nonTerminal.ProductionRules);

        public IEnumerable<ISymbol<T>> AllSymbols
        {
            get
            {
                foreach (Terminal<T> terminal in _terminals)
                    yield return terminal;

                foreach (NonTerminal<T> nonTerminal in _nonTerminals)
                    yield return nonTerminal;
            }
        }

        public NonTerminal<T> AcceptSymbol => (NonTerminal<T>)Start.ResultSymbol;

        public Terminal<T> EndOfInputTerminal { get; set; }


        public ParserConfigurator()
        {
            _nonTerminals = new List<NonTerminal<T>>();
            _terminals = new LinkedList<Terminal<T>>();
            lexerSettings = new LexerSettingsImpl();
            _terminalPrecedences = new List<TerminalPrecedence>();
            _currentPrecedence = 0;

            // Create the Error token. This will create it as terminal 0, but in the end it will be the LAST terminal
            // second last is EndOfInput. This is sort of hackish and mainly due to the way the lexer is configured.
            ErrorToken = CreateTerminal(null, s => default);
            ErrorToken.DebugName = "%ERROR%";

            // Set some default settings
            LexerSettings.CreateLexer = true;
            LexerSettings.EscapeLiterals = true;
            LexerSettings.Ignore = new[] { "\\s+" };     // Ignore all whitespace by default
        }

        public ITerminal<T> CreateTerminal(string? regex, Func<string, T>? onParse = null, bool topPrecedence = false)
        {
            Terminal<T> terminal = _terminals.SingleOrDefault(f => f.Regex == regex);

            if (terminal is { } && regex is { })
            {
                if (terminal.OnParse != (onParse??Terminal<T>.DefaultFunc))
                    throw new ParserConfigurationException("Redefinition of terminal uses the same regex but different onParse action");
            }
            else
            {
                terminal = new Terminal<T>(regex, onParse);

                if (topPrecedence)
                    _terminals.AddFirst(terminal);
                else
                    _terminals.AddLast(terminal);
            }

            return terminal;
        }

        public INonTerminal<T> CreateNonTerminal()
        {
            NonTerminal<T> nonTerminal = new NonTerminal<T>(this);

            _nonTerminals.Add(nonTerminal);
            
            if (_startSymbol == null)
                // First symbol to be created is the start symbol
                SetStartSymbol(nonTerminal);

            return nonTerminal;
        }

        public IPrecedenceGroup NonAssociative(params ITerminal<T>[] symbols) => SetSymbolAssociativity(symbols, AssociativityDirection.NonAssociative);

        public IPrecedenceGroup RightAssociative(params ITerminal<T>[] symbols) => SetSymbolAssociativity(symbols, AssociativityDirection.Right);

        public IPrecedenceGroup LeftAssociative(params ITerminal<T>[] symbols) => SetSymbolAssociativity(symbols, AssociativityDirection.Left);

        private IPrecedenceGroup SetSymbolAssociativity(IEnumerable<ITerminal<T>> symbols, AssociativityDirection associativityDirection)
        {
            foreach (Terminal<T> terminal in symbols.OfType<Terminal<T>>())
            {
                if (_terminalPrecedences.Any( f => f.Terminal == terminal))
                {
                    // This terminal is defined multiple times
                    throw new ParserConfigurationException(
                        string.Format("Terminal {0} has been declared to have a precedence multiple times",
                                      terminal.DebugName));
                }

                _terminalPrecedences.Add(new TerminalPrecedence
                {
                    Associativity = associativityDirection,
                    Terminal = terminal,
                    Precedence = _currentPrecedence
                });
            }

            PrecedenceGroup group = new PrecedenceGroup  { Precedence = _currentPrecedence };

            ++_currentPrecedence;

            return group;
        }

        public void SetStartSymbol(INonTerminal<T> start) => _startSymbol = (NonTerminal<T>)start;

        public void AugmentGrammar()
        {
            // First we need to augment the grammar with a start rule and a new start symbol
            // Create the derived start symbol
            NonTerminal<T> augmentedStart = (NonTerminal<T>)CreateNonTerminal();  // Unfortunate cast...

            // Use the start symbols debug name with a ' in front to indicate the augmented symbol.
            augmentedStart.DebugName = "'" + _startSymbol.DebugName;

            // Create a single production 
            augmentedStart.AddProduction(_startSymbol); // This production is never reduced, parser accepts when its about to reduce. No reduce action.
            Start = augmentedStart.ProductionRules.First(); // There's only one production.

            // Make sure all the terminals are registered.
            // This becomes neccessary since the user can configure the parser using only strings.
            // Since the nonterminal used for that does not carry a back-reference to the configurator,
            // we do it this way.
            // TODO: Does the terminals.AddLast ever get called? This looks like dead code to me, apart from the sanity
            // TODO: check for redefinition. Which even that gets done someplace else.
            foreach (NonTerminal<T> nonTerminal in _nonTerminals)
                foreach (Terminal<T> terminal in nonTerminal.ProductionRules.SelectMany(f => f.Symbols).OfType<Terminal<T>>())
                {
                    Terminal<T> oldTerminal = _terminals.SingleOrDefault(f => f.Regex == terminal.Regex);
                    if (oldTerminal != null)
                    {
                        if (oldTerminal.OnParse != terminal.OnParse)
                            throw new ParserConfigurationException(
                                "Multiply defined terminal has more than one OnParse action");
                    }
                    else
                        _terminals.AddLast(terminal);
                }

            // Add the end of input symbol
            EndOfInputTerminal = (Terminal<T>)CreateTerminal(null, s => default);
            EndOfInputTerminal.DebugName = "%EOF%";

            // Move the error symbol to the end of the list
            // Hackish I know, but it guarantees that the ErrorToken is always created and that 0 -> n-2 are reserved 
            // for the REAL symbols in the grammar.
            _terminals.Remove((Terminal<T>) ErrorToken);
            _terminals.AddLast((Terminal<T>) ErrorToken);

            // Assign all tokens in the grammar token numbers!
            AssignTokenNumbers();

            // This class is now a valid implementation of IGrammar, ready to use.
        }

        // User wants a default lexer, great. Use the lexer from grammar factory to fix him up
        public ILexer<T> CreateLexer() => LexerFactory<T>.ConfigureFromGrammar(this, LexerSettings);

        public IParser<T> CreateParser()
        {
            // User has forgotten to augment the grammar. Lets help him out and do it for him
            if (Start is null)
                AugmentGrammar();

            IParser<T> parser = new ParserBuilder<T>(this).CreateParser();

            // If our lexer settings says that we are supposed to create a lexer, do so now and assign the lexer to the created parser.
            if (LexerSettings.CreateLexer)
                parser.Lexer = CreateLexer();

            return parser;
        }

        public IPrecedenceGroup GetPrecedence(ITerminal<T> terminal) => _terminalPrecedences.FirstOrDefault(f => f.Terminal == terminal);

        private void AssignTokenNumbers()
        {
            int t = 0;

            foreach (ISymbol<T> symbol in AllSymbols)
                ((Symbol<T>)symbol).TokenNumber = t++;
        }


        private sealed class TerminalPrecedence
            : PrecedenceGroup
        {
            public Terminal<T>? Terminal { get; set; }
        }

        private sealed class LexerSettingsImpl
            : ILexerSettings
        {
            public bool CreateLexer { get; set; }
            public bool EscapeLiterals { get; set; }
            public string[]? Ignore { get; set; }
            public LexerRuntime Runtime { get; set; } = LexerRuntime.Tabular;
        }
    }
}
