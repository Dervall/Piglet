using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Piglet.Parser.Construction
{
    internal class Lr1ItemSet<T> : IEnumerable<Lr1Item<T>>
    {
        public List<Lr1Item<T>> Items { get; private set; }

        public Lr1ItemSet() => Items = new List<Lr1Item<T>>();

        public Lr1ItemSet(IEnumerable<Lr1Item<T>> lr1Items) => Items = new List<Lr1Item<T>>(lr1Items);

        public override string ToString() => string.Join("\n", Items);

        public bool Add(Lr1Item<T> item)
        {
            // See if there already exists an item with the same core
            Lr1Item<T> oldItem = Items.FirstOrDefault(f => f.ProductionRule == item.ProductionRule && f.DotLocation == item.DotLocation);
            if (oldItem != null)
            {
                // There might be lookaheads that needs adding
                bool addedLookahead = false;
                foreach (Configuration.Terminal<T> lookahead in item.Lookaheads)
                {
                    addedLookahead |= oldItem.Lookaheads.Add(lookahead);
                }
                return addedLookahead;
            }
            // There's no old item. Add the item and return true to indicate that we've added stuff
            Items.Add(item);
            return true;
        }

        public IEnumerator<Lr1Item<T>> GetEnumerator() => Items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Items.GetEnumerator();

        public Lr1Item<T> this[int index]
        {
            get { return Items[index]; }
        }

        public bool CoreEquals(Lr1ItemSet<T> other)
        {
            // Must be the same number of items
            if (Items.Count == other.Items.Count)
            {
                // Every item must have the same production rule the dot at the same place
                return Items.All(f => other.Any(o => o.ProductionRule == f.ProductionRule && f.DotLocation == o.DotLocation));
            }
            return false;
        }

        public void MergeLookaheads(Lr1ItemSet<T> other)
        {
            foreach (Lr1Item<T> lr1Item in Items)
            {
                Lr1Item<T> otherRule = other.First(f => f.ProductionRule == lr1Item.ProductionRule && f.DotLocation == lr1Item.DotLocation);
                lr1Item.Lookaheads.UnionWith(otherRule.Lookaheads);
            }
        }
    }
}