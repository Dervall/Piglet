using System;
using System.Collections.Generic;
using System.Linq;
using Piglet.Parser.Configuration;

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
            public List<Lr0Item<T>> From { get; set; }
            public List<Lr0Item<T>> To { get; set; }
            public ISymbol<T> OnSymbol { get; set; }
        }

        internal IParser<T> CreateParser()
        {
            // First order of business is to create the canonical list of LR0 states.
            // This starts with augmenting the grammar with an accept symbol, then we derive the
            // grammar from that
            var start = grammar.Start;

            // Get the first and follow sets for all nonterminal symbols
            var first = CalculateFirst();
            var follow = CalculateFollow(first);

            // So, we are going to calculate the LR0 closure for the start symbol, which should
            // be the augmented accept state of the grammar.
            // The closure is all states which are accessible by the dot at the left hand side of the
            // item.
            var itemSets = new List<List<Lr0Item<T>>>
                               {
                                   Closure(new List<Lr0Item<T>>
                                               {
                                                   new Lr0Item<T>(start, 0)
                                               })
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
                            gotoSet = Closure(gotoSet);
                            var oldGotoSet = itemSets.FirstOrDefault(f => f.All(a => gotoSet.Any(b => b.ProductionRule == a.ProductionRule &&
                                                                                b.DotLocation == a.DotLocation)));

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

            SLRParseTable<T> parseTable = CreateSLRParseTable(itemSets, follow, gotoSetTransitions);

            return new LRParser<T>(parseTable);
        }

        private SLRParseTable<T> CreateSLRParseTable(List<List<Lr0Item<T>>> itemSets, TerminalSet<T> follow, List<GotoSetTransition> gotoSetTransitions)
        {
            var table = new SLRParseTable<T>();

            // Holds the generated reduction rules, which we'll feed the table at the end of this method
            // the second part at least, the other is for indexing them while making the table.
            var reductionRules = new List<Tuple<IProductionRule<T>, ReductionRule<T>>>();

            for (int i = 0; i < itemSets.Count(); ++i )
            {
                var itemSet = itemSets[i];
                foreach (var lr0Item in itemSet)
                {
                    // Fill the action table first

                    // If the next symbol in the LR0 item is a terminal (symbol
                    // found after the dot, add a SHIFT j IF GOTO(lr0Item, nextSymbol) == j
                    if (lr0Item.SymbolRightOfDot != null)
                    {
                        if (lr0Item.SymbolRightOfDot is Terminal<T>)
                        {
                            // Look for a transition in the gotoSetTransitions
                            // there should always be one.
                            var transition = gotoSetTransitions.First(t => t.From == itemSet && t.OnSymbol == lr0Item.SymbolRightOfDot);
                            int transitionIndex = itemSets.IndexOf(transition.To);
                            int tokenNumber = ((Terminal<T>) lr0Item.SymbolRightOfDot).TokenNumber;
                            try
                            {
                                table.Action[i, tokenNumber] = SLRParseTable<T>.Shift(transitionIndex);
                            } 
                            catch (ShiftReduceConflictException<T> e)
                            {
                                // Since we wanted to shift, it will not be reduce reduce exceptions at this point

                                // Grammar is ambiguous. Since we have the full grammar at hand and the state table hasn't we
                                // can augment this exception for the benefit of the user.
                                e.ShiftSymbol = lr0Item.SymbolRightOfDot;
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
                        if (lr0Item.ProductionRule.ResultSymbol != grammar.AcceptSymbol)
                        {
                            int numReductionRules = reductionRules.Count();
                            int reductionRule = 0;
                            for (; reductionRule < numReductionRules; ++reductionRule)
                            {
                                if (reductionRules[reductionRule].Item1 == lr0Item.ProductionRule)
                                {
                                    // Found it, it's already created
                                    break;
                                }
                            }

                            if (numReductionRules == reductionRule)
                            {
                                // Need to create a new reduction rule
                                reductionRules.Add(new Tuple<IProductionRule<T>, ReductionRule<T>>(lr0Item.ProductionRule,
                                    new ReductionRule<T>
                                    {
                                        NumTokensToPop = lr0Item.ProductionRule.Symbols.Count(),
                                        OnReduce = lr0Item.ProductionRule.ReduceAction,
                                        TokenToPush = lr0Item.ProductionRule.ResultSymbol.TokenNumber
                                    }));
                            }

                            foreach (var followTerminal in follow[(NonTerminal<T>) lr0Item.ProductionRule.ResultSymbol])
                            {
                                try
                                {
                                    table.Action[i, followTerminal.TokenNumber] = SLRParseTable<T>.Reduce(reductionRule);
                                }
                                catch (ReduceReduceConflictException<T> e)
                                {
                                    // Augment exception with correct symbols for the poor user
                                    e.PreviousReduceSymbol = reductionRules[ -(1 + e.PreviousValue)].Item1.ResultSymbol;
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
                                        : grammar.AllSymbols.First(f => f.TokenNumber == e.PreviousValue);
                                    throw;
                                }
                            } 
                        }
                        else
                        {
                            // This production rule has the start symbol with the dot at the rightmost end in it, add ACCEPT to action
                            // for end of input character.
                            table.Action[i, grammar.EndOfInputTerminal.TokenNumber] = SLRParseTable<T>.Accept();
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
            table.ReductionRules = reductionRules.Select( f=> f.Item2 ).ToArray();

            // Useful point to look at the table, since after this point the grammar is pretty much destroyed.
           // string debugTable = table.ToDebugString(grammar, itemSets.Count());

            return table;
        }

        private TerminalSet<T> CalculateFollow(TerminalSet<T> first)
        {
            var follow = new TerminalSet<T>(grammar);

            // As per the dragon book, add end-of-input token to 
            // follow on the start symbol
            follow.Add(grammar.AcceptSymbol, grammar.EndOfInputTerminal);

            // TODO: This doesn't support epsilon rules yet
            bool addedThings;
            do
            {
                addedThings = false;
                foreach (var productionRule in grammar.ProductionRules)
                {
                    for (int n = 0; n < productionRule.Symbols.Length; ++n)
                    {
                        // Skip all terminals
                        if (productionRule.Symbols[n] is NonTerminal<T>)
                        {
                            var currentSymbol = (NonTerminal<T>) productionRule.Symbols[n];
                            var nextSymbol = n == productionRule.Symbols.Length - 1
                                                 ? null
                                                 : productionRule.Symbols[n + 1];
                            if (nextSymbol == null)
                            {
                                // Add everything in FOLLOW(production.ResultSymbol) since we were at the end
                                // of the production
                                // TODO: This is also a valid action if there is an Epsilon production of nextsymbol
                                foreach (var terminal in follow[(NonTerminal<T>)productionRule.ResultSymbol])
                                {
                                    addedThings |= follow.Add(currentSymbol, terminal);
                                }
                            }

                            if (nextSymbol != null)
                            {
                                // It's not at the end, if the next symbol is a terminal, just add it
                                if (nextSymbol is Terminal<T>)
                                {
                                    addedThings |= follow.Add(currentSymbol, (Terminal<T>) nextSymbol);
                                }
                                else
                                {
                                    // Add everthing in FIRST(nextSymbol)
                                    foreach (var terminal in first[(NonTerminal<T>)nextSymbol])
                                    {
                                        addedThings |= follow.Add(currentSymbol, terminal);
                                    }
                                }
                            }
                        }
                    }
                }
            } while (addedThings);

            return follow;
        }

        private TerminalSet<T> CalculateFirst()
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
                                addedThings |= first.Add(symbol, (Terminal<T>) productionSymbol);
                                
                                // This production rule is done now
                                break;
                            }

                            if (productionSymbol is NonTerminal<T>)
                            {
                                var nonTerminal = (NonTerminal<T>) productionSymbol;
                                // TODO: The check for nullable should be here...
                                // TODO: if it is nullable, it should add Epsilon to the first
                                // TODO: and continue with the next one. We are going to assume nullable
                                // TODO: is false and go on
                             /*   var nullable = false;
                                if (nullable)
                                {
                                    throw new NotImplementedException("Nullable production rules doesn't work yet");
                                }
                                else*/
                                {
                                    foreach (var f in first[nonTerminal])
                                    {
                                        addedThings |= first.Add(symbol, f);
                                    }
                                    // Jump out since the other symbols are not in the first set
                                    break;
                                }
                            }
                        }
                    }
                }
            } while (addedThings); 

            return first;
        }

        private IEnumerable<Lr0Item<T>> Goto(IEnumerable<Lr0Item<T>> closures, ISymbol<T> symbol)
        {
            // Every place there is a symbol to the right of the dot that matches the symbol we are looking for
            // add a new Lr0 item that has the dot moved one step to the right.
            return from lr0Item in closures
                   where lr0Item.SymbolRightOfDot != null && lr0Item.SymbolRightOfDot == symbol
                   select new Lr0Item<T>(lr0Item.ProductionRule, lr0Item.DotLocation + 1);
        }

        private List<Lr0Item<T>> Closure(IEnumerable<Lr0Item<T>> items)
        {
            // The items themselves are always in their own closure set
            var closure = new List<Lr0Item<T>>();
            closure.AddRange(items);

            var added = new HashSet<ISymbol<T>>();
            while (true)
            {
                var toAdd = new List<Lr0Item<T>>();
                foreach (var item in closure)
                {
                    ISymbol<T> symbolRightOfDot = item.SymbolRightOfDot;
                    if (symbolRightOfDot != null && !added.Contains(symbolRightOfDot))
                    {
                        // Create new Lr0 items from all rules where the resulting symbol of the production rule
                        // matches the symbol that was to the right of the dot.
                        toAdd.AddRange(
                            grammar.ProductionRules.Where(f => f.ResultSymbol == symbolRightOfDot).Select(
                                f => new Lr0Item<T>(f, 0)));
                        added.Add(symbolRightOfDot);
                    }
                }

                if (!toAdd.Any())
                    break;
                closure.AddRange(toAdd);
            }

            return closure;
        }
    }
}

