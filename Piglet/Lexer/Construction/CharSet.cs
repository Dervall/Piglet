using System.Collections.Generic;
using System.Linq;
using System;

namespace Piglet.Lexer.Construction
{
    internal sealed class CharSet
    {
        private readonly IList<CharRange> _ranges = new List<CharRange>();

        public IEnumerable<CharRange> Ranges => _ranges;


        public CharSet()
        {
        }

        public CharSet(IEnumerable<CharRange> ranges) => _ranges = ranges.ToList();

        public CharSet(bool combine, params char[] ranges)
        {
            if (ranges.Length % 2 != 0)
                throw new ArgumentException("Number of chars in ranges must be an even number.");
          
            for (int i = 0; i < ranges.Length; i += 2)
                AddRange(ranges[i], ranges[i + 1], combine);
        }

        public void Add(char c) => AddRange(c, c, true);

        public void AddRange(char from, char to, bool combine = true)
        {
            if (from > to)
            {
                char pivot = to;

                to = from;
                from = pivot;
            }

            if (combine)
                // See if there is an old range that contains the new from as the to in that case merge the ranges
                if (_ranges.SingleOrDefault(f => f.To == from) is { } r1)
                {
                    r1.To = to;

                    return;
                }
                else if (_ranges.SingleOrDefault(f => f.From == to) is { } r2) // To the same thing the other direction
                {
                    r2.From = from;

                    return;
                }

            // Ranges are not mergeable. Add the range straight up
            _ranges.Add(new CharRange { From = from, To = to });
        }

        public bool Any() => _ranges.Any();

        public override string ToString() => !Any() ? "ε" : string.Join(", ", _ranges.Select(f => f.ToString()).ToArray());

        public void UnionWith(CharSet charSet)
        {
            foreach (CharRange charRange in charSet._ranges)
                if (!_ranges.Contains(charRange))
                {
                    // Sanity check
//#if DEBUG
//                    if (ranges.Any(f => f.From == charRange.From || f.To == charRange.To))
//                        throw new Exception("Do not want");
//#endif
                    _ranges.Add(charRange);
                }
        }

        public CharSet Except(CharSet except)
        {
            CharSet cs = new CharSet();

            foreach (CharRange range in _ranges)
                foreach (CharRange clippedRange in ClipRange(range, except._ranges))
                    cs.AddRange(clippedRange.From, clippedRange.To);

            return cs;
        }

        private IEnumerable<CharRange> ClipRange(CharRange range, IList<CharRange> excludedCharRanges)
        {
            char from = range.From;
            char to = range.To;

            foreach (CharRange excludedRange in excludedCharRanges)
            {
                // If the range is fully excluded by the excluded range, yield nothing
                if (excludedRange.From <= from && excludedRange.To >= to)
                    yield break;

                // Check if the excluded range is wholly contained within the range
                if (excludedRange.From > from && excludedRange.To < to)
                {
                    // Split this range and return
                    foreach (CharRange charRange in ClipRange(new CharRange { From = @from, To = (char)(excludedRange.From - 1) }, excludedCharRanges))
                        yield return charRange;

                    // Second split
                    foreach (CharRange charRange in ClipRange(new CharRange { From = (char)(excludedRange.To + 1), To = to }, excludedCharRanges))
                        yield return charRange;

                    yield break;
                }

                // Trim the edges of the range
                if (to >= excludedRange.From && to <= excludedRange.To)
                    to = (char)(excludedRange.From - 1);
              
                if (from >= excludedRange.From && from <= excludedRange.To)
                    from = (char)(excludedRange.To + 1);
            }

            // If the range has been clipped away to nothing, then quit
            if (to < from)
                yield break;

            // Return the possibly clipped range
            yield return new CharRange { From = from, To = to };
        }

        public CharSet Union(CharSet charRange)
        {
            CharSet c = new CharSet();

            foreach (CharRange range in _ranges)
                c.AddRange(range.From, range.To);

            foreach (CharRange range in charRange._ranges)
                c.AddRange(range.From, range.To);

            return c;
        }

        public bool ContainsChar(char input) => _ranges.Any(charRange => charRange.From <= input && charRange.To >= input);
    }
}