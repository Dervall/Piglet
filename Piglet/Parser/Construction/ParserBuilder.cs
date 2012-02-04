using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Piglet.Parser.Configuration;
using Piglet.Parser.Construction.Debug;

namespace Piglet.Parser.Construction
{
    internal class ParserBuilder<T>
    {
        private readonly IGrammar<T> grammar;

        public ParserBuilder(IGrammar<T> grammar)
        {
            this.grammar = grammar;
        }

        private sealed class GotoSetTransition
        {
            public List<Lr1Item<T>> From { get; set; }
            public List<Lr1Item<T>> To { get; set; }
            public ISymbol<T> OnSymbol { get; set; }
        }

        internal IParser<T> CreateParser()
        {
            // First order of business is to create the canonical list of LR0 states.
            // This starts with augmenting the grammar with an accept symbol, then we derive the
            // grammar from that
            var start = grammar.Start;

            // Get the first and follow sets for all nonterminal symbols
            var nullable = CalculateNullable();
            var first = CalculateFirst(nullable);

            // So, we are going to calculate the LR0 closure for the start symbol, which should
            // be the augmented accept state of the grammar.
            // The closure is all states which are accessible by the dot at the left hand side of the
            // item.
            var itemSets = new List<List<Lr1Item<T>>>
                               {
                                   Closure(new List<Lr1Item<T>>
                                               {
                                                   new Lr1Item<T>(start, 0, new HashSet<Terminal<T>> {grammar.EndOfInputTerminal})
                                               }, first, nullable)
                               };
            var gotoSetTransitions = new List<GotoSetTransition>();

            // TODO: This method is probably one big stupid performance sink since it iterates WAY to many times over the input

            // Repeat until nothing gets added any more
            while (true)
            {
                bool anythingAdded = false;

                foreach (var itemSet in itemSets)
                {
                    foreach (var symbol in grammar.AllSymbols)
                    {
                        // Calculate the itemset for by goto for each symbol in the grammar
                        var gotoSet = Goto(itemSet, symbol).ToList();

                        // If there is anything found in the set
                        if (gotoSet.Any())
                        {
                            // Do a closure on the goto set and see if it's already present in the sets of items that we have
                            // if that is not the case add it to the item sets and restart the entire thing.
                            gotoSet = Closure(gotoSet, first, nullable);

                            // TODO: I think this is the place to merge sets!

                            var oldGotoSet = itemSets.FirstOrDefault(f => f.All(a => gotoSet.Any(b => b.ProductionRule == a.ProductionRule &&
                                                                                b.DotLocation == a.DotLocation && b.Lookaheads.SetEquals(a.Lookaheads))));

                            if (oldGotoSet == null)
                            {
                                // Add goto set to itemsets
                                itemSets.Add(gotoSet);

                                // Add a transition
                                gotoSetTransitions.Add(new GotoSetTransition
                                                           {
                                                               From = itemSet,
                                                               OnSymbol = symbol,
                                                               To = gotoSet
                                                           });

                                anythingAdded = true;
                                break;
                            }
                            // Already found the set, add a transition if it already isn't there
                            var nt = new GotoSetTransition
                                         {
                                             From = itemSet,
                                             OnSymbol = symbol,
                                             To = oldGotoSet
                                         };
                            if (!gotoSetTransitions.Any(a => a.From == nt.From && a.OnSymbol == nt.OnSymbol && a.To == nt.To))
                            {
                                gotoSetTransitions.Add(nt);

                                // TODO: Not sure if should set anything added to true. Better set it
                                // TODO: Only thing that can happen is that this function is EVEN slower than it already is
                                anythingAdded = true;
                            }
                        }
                    }
                    if (anythingAdded)
                        break;
                }
                if (!anythingAdded)
                    break;
            }

            LRParseTable<T> parseTable = CreateParseTable(itemSets, gotoSetTransitions);

            return new LRParser<T>(parseTable);
        }

        private ISet<NonTerminal<T>> CalculateNullable()
        {
            // TODO: This is a naïve implementation that keeps iterating until the set becomes stable
            // TODO: This could probably be optimized.

            // A nullable symbol is a symbol that may consist of only epsilon transitions
            var nullable = new HashSet<NonTerminal<T>>();

            bool nullableSetChanged;

            do
            {
                nullableSetChanged = false;
                foreach (var nonTerminal in grammar.AllSymbols.OfType<NonTerminal<T>>())
                {
                    // No need to reevaluate things we know to be nullable.
                    if (nullable.Contains(nonTerminal))
                        continue;

                    foreach (var production in nonTerminal.ProductionRules)
                    {
                        // If this production is nullable, add the nonterminal to the set.

                        // Iterate over symbols. If we find a terminal it is never nullable
                        // if we find a nonterminal continue iterating only if this terminal itself is not nullable.
                        // By this rule, empty production rules will always return nullable true
                        bool symbolIsNullable = true;

                        foreach (var symbol in production.Symbols)
                        {
                            if (symbol is Terminal<T> || !nullable.Contains((NonTerminal<T>)symbol))
                            {
                                symbolIsNullable = false;
                                break;
                            }
                        }
                        if (symbolIsNullable)
                        {
                            nullableSetChanged |= nullable.Add(nonTerminal);
                        }
                    }
                }
            } while (nullableSetChanged);

            return nullable;
        }

        private LRParseTable<T> CreateParseTable(List<List<Lr1Item<T>>> itemSets, List<GotoSetTransition> gotoSetTransitions)
        {
            var table = new LRParseTable<T>();

            // Holds the generated reduction rules, which we'll feed the table at the end of this method
            // the second part at least, the other is for indexing them while making the table.
            var reductionRules = new List<Tuple<IProductionRule<T>, ReductionRule<T>>>();

            for (int i = 0; i < itemSets.Count(); ++i)
            {
                var itemSet = itemSets[i];
                foreach (var lr1Item in itemSet)
                {
                    // Fill the action table first

                    // If the next symbol in the LR0 item is a terminal (symbol
                    // found after the dot, add a SHIFT j IF GOTO(lr0Item, nextSymbol) == j
                    if (lr1Item.SymbolRightOfDot != null)
                    {
                        if (lr1Item.SymbolRightOfDot is Terminal<T>)
                        {
                            // Look for a transition in the gotoSetTransitions
                            // there should always be one.
                            var transition = gotoSetTransitions.First(t => t.From == itemSet && t.OnSymbol == lr1Item.SymbolRightOfDot);
                            int transitionIndex = itemSets.IndexOf(transition.To);
                            int tokenNumber = ((Terminal<T>)lr1Item.SymbolRightOfDot).TokenNumber;
                            try
                            {
                                table.Action[i, tokenNumber] = LRParseTable<T>.Shift(transitionIndex);
                            }
                            catch (ShiftReduceConflictException<T> e)
                            {
                                // Since we wanted to shift, it will not be reduce reduce exceptions at this point

                                // Grammar is ambiguous. Since we have the full grammar at hand and the state table hasn't we
                                // can augment this exception for the benefit of the user.
                                e.ShiftSymbol = lr1Item.SymbolRightOfDot;
                                e.ReduceSymbol = reductionRules[-(1 + e.PreviousValue)].Item1.ResultSymbol;
                                throw;
                            }
                        }
                    }
                    else
                    {
                        // The dot is at the end. Add reduce action to the parse table for
                        // all FOLLOW for the resulting symbol
                        // Do NOT do this if the resulting symbol is the start symbol
                        if (lr1Item.ProductionRule.ResultSymbol != grammar.AcceptSymbol)
                        {
                            int numReductionRules = reductionRules.Count();
                            int reductionRule = 0;
                            for (; reductionRule < numReductionRules; ++reductionRule)
                            {
                                if (reductionRules[reductionRule].Item1 == lr1Item.ProductionRule)
                                {
                                    // Found it, it's already created
                                    break;
                                }
                            }

                            if (numReductionRules == reductionRule)
                            {
                                // Need to create a new reduction rule
                                reductionRules.Add(new Tuple<IProductionRule<T>, ReductionRule<T>>(lr1Item.ProductionRule,
                                    new ReductionRule<T>
                                    {
                                        NumTokensToPop = lr1Item.ProductionRule.Symbols.Count(),
                                        OnReduce = lr1Item.ProductionRule.ReduceAction,
                                        TokenToPush = lr1Item.ProductionRule.ResultSymbol.TokenNumber
                                    }));
                            }

                            foreach (var lookahead in lr1Item.Lookaheads)
                            {
                                try
                                {
                                    table.Action[i, lookahead.TokenNumber] = LRParseTable<T>.Reduce(reductionRule);
                                }
                                catch (ReduceReduceConflictException<T> e)
                                {
                                    // Augment exception with correct symbols for the poor user
                                    e.PreviousReduceSymbol = reductionRules[-(1 + e.PreviousValue)].Item1.ResultSymbol;
                                    e.NewReduceSymbol = reductionRules[reductionRule].Item1.ResultSymbol;
                                    throw;
                                }
                                catch (ShiftReduceConflictException<T> e)
                                {
                                    // We know we're the cause of the reduce part
                                    e.ReduceSymbol = reductionRules[reductionRule].Item1.ResultSymbol;
                                    // The old value is the shift
                                    e.ShiftSymbol = e.PreviousValue == int.MaxValue
                                        ? grammar.AcceptSymbol // Conflicting with the accept symbol
                                        : grammar.AllSymbols.FirstOrDefault(f => f.TokenNumber == e.PreviousValue);
                                    throw;
                                }
                            }
                        }
                        else
                        {
                            // This production rule has the start symbol with the dot at the rightmost end in it, add ACCEPT to action
                            // for end of input character.
                            table.Action[i, grammar.EndOfInputTerminal.TokenNumber] = LRParseTable<T>.Accept();
                        }
                    }
                }

                // Fill the goto table with the state IDs of all states that have been originally
                // produced by the GOTO operation from this state
                foreach (var gotoTransition in gotoSetTransitions.Where(f => f.From == itemSet && f.OnSymbol is NonTerminal<T>))
                {
                    table.Goto[i, gotoTransition.OnSymbol.TokenNumber] = itemSets.IndexOf(gotoTransition.To);
                }
            }

            // Move the reduction rules to the table. No need for the impromptu dictionary
            // anymore.
            table.ReductionRules = reductionRules.Select(f => f.Item2).ToArray();

            // Useful point to look at the table, since after this point the grammar is pretty much destroyed.
            //       string debugTable = table.ToDebugString(grammar);

            return table;
        }

        private TerminalSet<T> CalculateFirst(ISet<NonTerminal<T>> nullable)
        {
            var first = new TerminalSet<T>(grammar);

            // Algorithm is that if a nonterminal has a production that starts with a 
            // terminal, we add that to the first set. If it starts with a nonterminal, we add
            // that nonterminals firsts to the known firsts of our nonterminal.
            // TODO: There is probably performance benefits to optimizing this.
            bool addedThings;
            do
            {
                addedThings = false;

                foreach (var symbol in grammar.AllSymbols.OfType<NonTerminal<T>>())
                {
                    foreach (var productionRule in symbol.ProductionRules)
                    {
                        foreach (var productionSymbol in productionRule.Symbols)
                        {
                            // Terminals are trivial, just add them
                            if (productionSymbol is Terminal<T>)
                            {
                                addedThings |= first.Add(symbol, (Terminal<T>)productionSymbol);

                                // This production rule is done now
                                break;
                            }

                            if (productionSymbol is NonTerminal<T>)
                            {
                                var nonTerminal = (NonTerminal<T>)productionSymbol;
                                // Add everything in FIRST for the given terminal.
                                foreach (var f in first[nonTerminal])
                                {
                                    addedThings |= first.Add(symbol, f);
                                }

                                // Stop iterating if it wasn't nullable
                                if (!nullable.Contains(nonTerminal))
                                {
                                    // Jump out since we've found a non nullable symbol
                                    break;
                                }
                            }
                        }
                    }
                }
            } while (addedThings);

            return first;
        }

        private IEnumerable<Lr1Item<T>> Goto(IEnumerable<Lr1Item<T>> closures, ISymbol<T> symbol)
        {
            // Every place there is a symbol to the right of the dot that matches the symbol we are looking for
            // add a new Lr1 item that has the dot moved one step to the right.
            return from lr1Item in closures
                   where lr1Item.SymbolRightOfDot != null && lr1Item.SymbolRightOfDot == symbol
                   select new Lr1Item<T>(lr1Item.ProductionRule, lr1Item.DotLocation + 1, lr1Item.Lookaheads);
        }

        private List<Lr1Item<T>> Closure(IEnumerable<Lr1Item<T>> items, TerminalSet<T> first, ISet<NonTerminal<T>> nullable)
        {
            // The items themselves are always in their own closure set
            var closure = new Lr1ItemSet<T>();
            foreach (var lr1Item in items)
            {
                closure.Add(lr1Item);
            }

            // This needs to be a normal for loop since we add to the underlying collection
            // as we go along. This avoids investigating the same rule twice
            for (int currentItem = 0; currentItem < closure.Count(); ++currentItem)
            {
                var item = closure[currentItem];

                ISymbol<T> symbolRightOfDot = item.SymbolRightOfDot;
                if (symbolRightOfDot != null) // && !added.Contains(symbolRightOfDot))
                {
                    // Generate the lookahead items
                    var lookaheads = new HashSet<Terminal<T>>();

                    bool nonNullableFound = false;
                    for (int i = item.DotLocation + 1; i < item.ProductionRule.Symbols.Length; ++i)
                    {
                        var symbol = item.ProductionRule.Symbols[i];

                        // If symbol is terminal, just add it
                        if (symbol is Terminal<T>)
                        {
                            lookaheads.Add((Terminal<T>)symbol);

                            // Terminals are not nullable, break out of loop
                            nonNullableFound = true;
                            break;
                        }

                        foreach (var terminal in first[(NonTerminal<T>)symbol])
                        {
                            lookaheads.Add(terminal);
                        }

                        if (!nullable.Contains(symbol))
                        {
                            nonNullableFound = true;
                            break;
                        }
                    }

                    if (!nonNullableFound)
                    {
                        // Add each of the lookahead symbols of the generating rule
                        // to the new lookahead set
                        foreach (var lookahead in item.Lookaheads)
                        {
                            lookaheads.Add(lookahead);
                        }
                    }

                    // Create new Lr1 items from all rules where the resulting symbol of the production rule
                    // matches the symbol that was to the right of the dot.
                    var newLr1Items =
                        grammar.ProductionRules.Where(f => f.ResultSymbol == symbolRightOfDot).Select(
                            f => new Lr1Item<T>(f, 0, lookaheads));

                    foreach (var lr1Item in newLr1Items)
                    {
                        closure.Add(lr1Item);
                    }
                }
            }

            return closure.Items;
        }
    }

    internal class Lr1ItemSet<T> : IEnumerable<Lr1Item<T>>
    {
        public List<Lr1Item<T>> Items { get; set; }

        public Lr1ItemSet()
        {
            Items = new List<Lr1Item<T>>();
        }

        public bool Add(Lr1Item<T> item)
        {
            // See if there already exists an item with the same core
            var oldItem = Items.FirstOrDefault(f => f.ProductionRule == item.ProductionRule && f.DotLocation == item.DotLocation);
            if (oldItem != null)
            {
                // There might be lookaheads that needs adding
                bool addedLookahead = false;
                foreach (var lookahead in item.Lookaheads)
                {
                    addedLookahead |= oldItem.Lookaheads.Add(lookahead);
                }
                return addedLookahead;
            }
            // There's no old item. Add the item and return true to indicate that we've added stuff
            Items.Add(item);
            return true;
        }

        public IEnumerator<Lr1Item<T>> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public Lr1Item<T> this[int index]
        {
            get { return Items[index]; }
        }
    }
}

