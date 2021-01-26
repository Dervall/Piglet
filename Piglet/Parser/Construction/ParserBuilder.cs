using System.Collections.Generic;
using System.Linq;
using System;

using Piglet.Parser.Configuration;
using Piglet.Common;

namespace Piglet.Parser.Construction
{
    internal sealed class ParserBuilder<T>
    {
        // Holds the generated reduction rules, which we'll feed the table at the end of this method
        // the second part at least, the other is for indexing them while making the table.
        private readonly List<(IProductionRule<T>, ReductionRule<T>)> _reductionRules;
        private readonly IGrammar<T> _grammar;


        public ParserBuilder(IGrammar<T> grammar)
        {
            _grammar = grammar;
            _reductionRules = new List<(IProductionRule<T>, ReductionRule<T>)>();
        }

        internal IParser<T> CreateParser()
        {
            // First order of business is to create the canonical list of LR1 states, or at least we are going to go through them as we merge the sets together.
            // This starts with augmenting the grammar with an accept symbol, then we derive the grammar from that
            IProductionRule<T> start = _grammar.Start;

            // Get the first and follow sets for all nonterminal symbols
            ISet<NonTerminal<T>> nullable = CalculateNullable();
            TerminalSet<T> first = CalculateFirst(nullable);

            // So, we are going to calculate the LR1 closure for the start symbol, which should
            // be the augmented accept state of the grammar.
            // The closure is all states which are accessible by the dot at the left hand side of the
            // item.
            List<Lr1ItemSet<T>> itemSets = new List<Lr1ItemSet<T>>
            {
                Closure(new List<Lr1Item<T>>
                {
                    new Lr1Item<T>(start, 0, new HashSet<Terminal<T>> {_grammar.EndOfInputTerminal})
                }, first, nullable)
            };
            List<GotoSetTransition> gotoSetTransitions = new List<GotoSetTransition>();

            // Repeat until nothing gets added any more
            // This is neccessary since we are merging sets as we go, which changes things around.
            bool added;

            do
            {
                added = false;

                for (int i = 0; i < itemSets.Count(); ++i)
                {
                    Lr1ItemSet<T> itemSet = itemSets[i];

                    foreach (ISymbol<T> symbol in _grammar.AllSymbols)
                    {
                        // Calculate the itemset for by goto for each symbol in the grammar
                        Lr1ItemSet<T> gotoSet = Goto(itemSet, symbol);

                        // If there is anything found in the set
                        if (gotoSet.Any())
                        {
                            // Do a closure on the goto set and see if it's already present in the sets of items that we have
                            // if that is not the case add it to the item set
                            gotoSet = Closure(gotoSet, first, nullable);

                            Lr1ItemSet<T> oldGotoSet = itemSets.FirstOrDefault(f => f.CoreEquals(gotoSet));

                            if (oldGotoSet is null)
                            {
                                itemSets.Add(gotoSet); // Add goto set to itemsets
                                gotoSetTransitions.Add(new GotoSetTransition(itemSet, gotoSet, symbol)); // Add a transition

                                added = true;
                            }
                            else
                            {
                                // Already found the set
                                // Merge the lookaheads for all rules
                                oldGotoSet.MergeLookaheads(gotoSet);

                                // Add a transition if it already isn't there
                                GotoSetTransition nt = new GotoSetTransition(itemSet, oldGotoSet, symbol);

                                if (!gotoSetTransitions.Any(a => a.From == nt.From && a.OnSymbol == nt.OnSymbol && a.To == nt.To))
                                    gotoSetTransitions.Add(nt);
                            }
                        }
                    }
                }
            }
            while (added);

            LRParseTable<T> parseTable = CreateParseTable(itemSets, gotoSetTransitions);

            // Create a new parser using that parse table and some additional information that needs
            // to be available for the runtime parsing to work.
            return new LRParser<T>(
                parseTable,
                ((Terminal<T>)_grammar.ErrorToken).TokenNumber,
                _grammar.EndOfInputTerminal.TokenNumber,
                _grammar.AllSymbols.OfType<Terminal<T>>().Select(f => f.DebugName).ToArray()
            );
        }

        private ISet<NonTerminal<T>> CalculateNullable()
        {
            // TODO: This is a naive implementation that keeps iterating until the set becomes stable
            // TODO: This could probably be optimized.

            // A nullable symbol is a symbol that may consist of only epsilon transitions
            HashSet<NonTerminal<T>> nullable = new HashSet<NonTerminal<T>>();
            bool nullableSetChanged;

            do
            {
                nullableSetChanged = false;

                foreach (NonTerminal<T> nonTerminal in _grammar.AllSymbols.OfType<NonTerminal<T>>())
                {
                    // No need to reevaluate things we know to be nullable.
                    if (nullable.Contains(nonTerminal))
                        continue;

                    foreach (IProductionRule<T> production in nonTerminal.ProductionRules)
                    {
                        // If this production is nullable, add the nonterminal to the set.

                        // Iterate over symbols. If we find a terminal it is never nullable
                        // if we find a nonterminal continue iterating only if this terminal itself is not nullable.
                        // By this rule, empty production rules will always return nullable true
                        bool symbolIsNullable = production.Symbols.All(symbol => symbol is NonTerminal<T> term && nullable.Contains(term));

                        if (symbolIsNullable)
                            nullableSetChanged |= nullable.Add(nonTerminal);
                    }
                }
            }
            while (nullableSetChanged);

            return nullable;
        }

        private LRParseTable<T> CreateParseTable(List<Lr1ItemSet<T>> itemSets, List<GotoSetTransition> gotoSetTransitions)
        {
            LRParseTable<T> table = new LRParseTable<T>();

            // Create a temporary uncompressed action table. This is what we will use to create
            // the compressed action table later on. This could probably be improved upon to save memory if needed.
            short[,] uncompressedActionTable = new short[itemSets.Count, _grammar.AllSymbols.OfType<Terminal<T>>().Count()];

            for (int i = 0; i < itemSets.Count(); ++i)
                for (int j = 0; j < _grammar.AllSymbols.OfType<Terminal<T>>().Count(); ++j)
                    uncompressedActionTable[i, j] = short.MinValue;

            int firstNonTerminalTokenNumber = _grammar.AllSymbols.OfType<NonTerminal<T>>().First().TokenNumber;
            List<GotoTable.GotoTableValue> gotos = new List<GotoTable.GotoTableValue>();

            for (int i = 0; i < itemSets.Count(); ++i)
            {
                Lr1ItemSet<T> itemSet = itemSets[i];
                foreach (Lr1Item<T> lr1Item in itemSet)
                {
                    // Fill the action table first

                    // If the next symbol in the LR0 item is a terminal (symbol
                    // found after the dot, add a SHIFT j IF GOTO(lr0Item, nextSymbol) == j
                    if (lr1Item.SymbolRightOfDot != null)
                    {
                        if (lr1Item.SymbolRightOfDot is Terminal<T>)
                        {
                            // Look for a transition in the gotoSetTransitions there should always be one.
                            GotoSetTransition transition = gotoSetTransitions.First(t => t.From == itemSet && t.OnSymbol == lr1Item.SymbolRightOfDot);
                            int transitionIndex = itemSets.IndexOf(transition.To);
                            int tokenNumber = ((Terminal<T>)lr1Item.SymbolRightOfDot).TokenNumber;

                            SetActionTable(uncompressedActionTable, i, tokenNumber, LRParseTable<T>.Shift(transitionIndex));
                        }
                    }
                    else
                    {
                        // The dot is at the end. Add reduce action to the parse table for all lookaheads for the resulting symbol
                        // Do NOT do this if the resulting symbol is the start symbol
                        if (lr1Item.ProductionRule.ResultSymbol != _grammar.AcceptSymbol)
                        {
                            int numReductionRules = _reductionRules.Count();
                            int reductionRule = 0;

                            for (; reductionRule < numReductionRules; ++reductionRule)
                                if (_reductionRules[reductionRule].Item1 == lr1Item.ProductionRule)
                                    break; // Found it, it's already created

                            if (numReductionRules == reductionRule)
                                // Need to create a new reduction rule
                                _reductionRules.Add((lr1Item.ProductionRule, new ReductionRule<T>(
                                    (INonTerminal<T>)lr1Item.ProductionRule.ResultSymbol,
                                    lr1Item.ProductionRule.Symbols.Count(),
                                    ((Symbol<T>)lr1Item.ProductionRule.ResultSymbol).TokenNumber - firstNonTerminalTokenNumber,
                                    lr1Item.ProductionRule.ReduceAction!
                                )));

                            foreach (Terminal<T> lookahead in lr1Item.Lookaheads)
                                try
                                {
                                    SetActionTable(uncompressedActionTable, i, lookahead.TokenNumber, LRParseTable<T>.Reduce(reductionRule));
                                }
                                catch (ReduceReduceConflictException<T> e)
                                {
                                    // Augment exception with correct symbols for the poor user
                                    e.PreviousReduceSymbol = _reductionRules[-(1 + e.PreviousValue)].Item1.ResultSymbol;
                                    e.NewReduceSymbol = _reductionRules[reductionRule].Item1.ResultSymbol;
                                    e._message = $"Grammar contains a reduce-reduce conflict: previous reduce symbol '{e.PreviousReduceSymbol}', new reduce symbol: '{e.NewReduceSymbol}'.\nDid you forget to set an associativity/precedence?";

                                    throw;
                                }
                        }
                        else
                            // This production rule has the start symbol with the dot at the rightmost end in it, add ACCEPT to action for end of input character.
                            SetActionTable(uncompressedActionTable, i, _grammar.EndOfInputTerminal.TokenNumber, LRParseTable<T>.Accept());
                    }
                }

                // Fill the goto table with the state IDs of all states that have been originally produced by the GOTO operation from this state
                foreach (GotoSetTransition gotoTransition in gotoSetTransitions.Where(f => f.From == itemSet && f.OnSymbol is NonTerminal<T>))
                    gotos.Add(new GotoTable.GotoTableValue
                    {
                        NewState = itemSets.IndexOf(gotoTransition.To),
                        State = i,
                        Token = ((Symbol<T>)gotoTransition.OnSymbol).TokenNumber - firstNonTerminalTokenNumber
                    });
            }

            // Move the reduction rules to the table. No need for the impromptu dictionary
            // anymore.
            table.ReductionRules = _reductionRules.Select(f => f.Item2).ToArray();
            table.Action = new CompressedTable(uncompressedActionTable);
            table.Goto = new GotoTable(gotos);
            table.StateCount = itemSets.Count;

            // Useful point to look at the table, and everything the builder has generated, since after this point the grammar is pretty much destroyed.
            //string gotoGraph = gotoSetTransitions.AsDotNotation(itemSets);
            //string debugTable = table.ToDebugString(grammar, itemSets.Count);
            return table;
        }

        private void SetActionTable(short[,] table, int state, int tokenNumber, short value)
        {
            // This is an error condition, find out what sort of exception it is
            short oldValue = table[state, tokenNumber];

            if (oldValue != value && oldValue != short.MinValue)
            {
                try
                {
                    if (oldValue < 0 && value < 0)
                        // Both values are reduce. Throw a reduce reduce conflict. This is not solveable
                        throw new ReduceReduceConflictException<T>("Grammar contains a reduce-reduce conflict.\nDid you forget to set an associativity/precedence?");

                    int shiftTokenNumber = tokenNumber;
                    int reduceRuleNumber;
                    short shiftValue;
                    short reduceValue;

                    if (oldValue < 0)
                    {
                        // The old value was a reduce, the new must be a shift
                        shiftValue = value;
                        reduceValue = oldValue;
                        reduceRuleNumber = -(oldValue + 1);
                    }
                    else
                    {
                        // TODO: Unsure if this is a real case. The only testcases 
                        // TODO: that end up here are retarded tests which are cyclic in nature.
                        // TODO: These cases always fail later on anyway due to conflicts.
                        // The old value was a shift
                        // the new value must be a reduce
                        shiftValue = oldValue;
                        reduceValue = value;
                        reduceRuleNumber = -(value + 1);
                    }

                    // Check if these tokens have declared precedences and associativity
                    // If they do, we might be able to act on this.
                    Terminal<T> shiftingTerminal = _grammar.AllSymbols.OfType<Terminal<T>>().First(f => f.TokenNumber == shiftTokenNumber);
                    IPrecedenceGroup shiftPrecedence = _grammar.GetPrecedence(shiftingTerminal);
                    IProductionRule<T> productionRule = _reductionRules[reduceRuleNumber].Item1;

                    // If the rule has a context dependent precedence, use that. Otherwise use
                    // the reduce precedence of the last terminal symbol in the production rules precedence                        
                    IPrecedenceGroup reducePrecedence = productionRule.ContextPrecedence ??
                        _grammar.GetPrecedence(productionRule.Symbols.Reverse().OfType<ITerminal<T>>().FirstOrDefault());

                    // If either rule has no precedence this is not a legal course of action.
                    // TODO: In bison this is apparently cool, it prefers to shift in this case. I don't know why, but this seems like a dangerous course of action to me.
                    if (shiftPrecedence is null || reducePrecedence is null)
                        throw new ShiftReduceConflictException<T>(shiftingTerminal, productionRule.ResultSymbol);

                    if (shiftPrecedence.Precedence < reducePrecedence.Precedence)
                        table[state, tokenNumber] = reduceValue; // Precedence of reduce is higher, choose to reduce
                    else if (shiftPrecedence.Precedence > reducePrecedence.Precedence)
                        table[state, tokenNumber] = shiftValue; // Shift precedence is higher. Shift

                    // Both tokens are in the same precedence group! It's now up to the associativity
                    // The two tokens CANNOT have different associativity, due to how the configuration works
                    // which throws up if you try to multiple-define the precedence
                    else if (shiftPrecedence.Associativity == AssociativityDirection.Left)
                        table[state, tokenNumber] = reduceValue; // Prefer reducing
                    else if (shiftPrecedence.Associativity == AssociativityDirection.Right)
                        table[state, tokenNumber] = shiftValue; // Prefer shifting
                    else // if (shiftPrecedence.Associativity == AssociativityDirection.NonAssociative) <- this is implied
                        throw new ShiftReduceConflictException<T>(shiftingTerminal, productionRule.ResultSymbol);
                }
                catch (AmbiguousGrammarException ex)
                {
                    // Fill in more information on the error and rethrow the error
                    ex.StateNumber = state;
                    ex.TokenNumber = tokenNumber;
                    ex.PreviousValue = oldValue;
                    ex.NewValue = value;

                    throw;
                }
            }
            else
                table[state, tokenNumber] = value;
        }

        private TerminalSet<T> CalculateFirst(ISet<NonTerminal<T>> nullable)
        {
            TerminalSet<T> first = new TerminalSet<T>(_grammar);

            // Algorithm is that if a nonterminal has a production that starts with a 
            // terminal, we add that to the first set. If it starts with a nonterminal, we add
            // that nonterminals firsts to the known firsts of our nonterminal.
            bool addedThings;

            do
            {
                addedThings = false;

                foreach (NonTerminal<T> symbol in _grammar.AllSymbols.OfType<NonTerminal<T>>())
                    foreach (IProductionRule<T> productionRule in symbol.ProductionRules)
                        foreach (ISymbol<T>? productionSymbol in productionRule.Symbols)
                            if (productionSymbol is Terminal<T> terminal)
                            {
                                // Terminals are trivial, just add them
                                addedThings |= first.Add(symbol, terminal);

                                // This production rule is done now
                                break;
                            }
                            else if (productionSymbol is NonTerminal<T> nonTerminal)
                            {
                                // Add everything in FIRST for the given terminal.
                                foreach (Terminal<T> f in first[nonTerminal])
                                    addedThings |= first.Add(symbol, f);

                                // Stop iterating if it wasn't nullable
                                if (!nullable.Contains(nonTerminal))
                                    break; // Jump out since we've found a non nullable symbol
                            }
            }
            while (addedThings);

            return first;
        }

        private Lr1ItemSet<T> Goto(IEnumerable<Lr1Item<T>> closures, ISymbol<T> symbol) =>
            // Every place there is a symbol to the right of the dot that matches the symbol we are looking for
            // add a new Lr1 item that has the dot moved one step to the right.
            new Lr1ItemSet<T>(from lr1Item in closures
                              where lr1Item.SymbolRightOfDot != null
                              where lr1Item.SymbolRightOfDot == symbol
                              select new Lr1Item<T>(lr1Item.ProductionRule, lr1Item.DotLocation + 1, lr1Item.Lookaheads));

        private Lr1ItemSet<T> Closure(IEnumerable<Lr1Item<T>> items, TerminalSet<T> first, ISet<NonTerminal<T>> nullable)
        {
            // The items themselves are always in their own closure set
            Lr1ItemSet<T> closure = new Lr1ItemSet<T>();

            foreach (Lr1Item<T> lr1Item in items)
                closure.Add(lr1Item);

            // This needs to be a normal for loop since we add to the underlying collection
            // as we go along. This avoids investigating the same rule twice
            for (int currentItem = 0; currentItem < closure.Count(); ++currentItem)
            {
                Lr1Item<T> item = closure[currentItem];
                ISymbol<T> symbolRightOfDot = item.SymbolRightOfDot;

                if (symbolRightOfDot != null)
                {
                    // Generate the lookahead items
                    HashSet<Terminal<T>> lookaheads = new HashSet<Terminal<T>>();
                    bool nonNullableFound = false;

                    for (int i = item.DotLocation + 1; i < item.ProductionRule.Symbols.Length; ++i)
                    {
                        ISymbol<T>? symbol = item.ProductionRule.Symbols[i];

                        // If symbol is terminal, just add it
                        if (symbol is Terminal<T> term)
                        {
                            lookaheads.Add(term);

                            // Terminals are not nullable, break out of loop
                            nonNullableFound = true;

                            break;
                        }

                        foreach (Terminal<T> terminal in first[(NonTerminal<T>?)symbol])
                            lookaheads.Add(terminal);

                        if (!nullable.Contains(symbol))
                        {
                            nonNullableFound = true;

                            break;
                        }
                    }

                    if (!nonNullableFound)
                        // Add each of the lookahead symbols of the generating rule to the new lookahead set
                        foreach (Terminal<T> lookahead in item.Lookaheads)
                            lookaheads.Add(lookahead);

                    // Create new Lr1 items from all rules where the resulting symbol of the production rule matches the symbol that was to the right of the dot.
                    IEnumerable<Lr1Item<T>> newLr1Items = _grammar.ProductionRules.Where(f => f.ResultSymbol == symbolRightOfDot).Select(f => new Lr1Item<T>(f, 0, lookaheads));

                    foreach (Lr1Item<T> lr1Item in newLr1Items)
                        closure.Add(lr1Item);
                }
            }

            return closure;
        }


        internal sealed class GotoSetTransition
        {
            public Lr1ItemSet<T> From { get; }
            public Lr1ItemSet<T> To { get; }
            public ISymbol<T> OnSymbol { get; }


            public GotoSetTransition(Lr1ItemSet<T> from, Lr1ItemSet<T> to, ISymbol<T> onSymbol)
            {
                From = from;
                To = to;
                OnSymbol = onSymbol;
            }
        }
    }
}
