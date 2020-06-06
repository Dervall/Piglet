using System;

namespace Piglet.Lexer.Construction
{
    internal sealed class CharRange
        : IComparable<CharRange>
    {
        public char From { get; set; }
        public char To { get; set; }


        public int CompareTo(CharRange other)
    	{
    		int cmp = From - other.From;

    		return cmp == 0 ? To - other.To : cmp;
    	}

        public override string ToString() => From == To ? ToGraphSafeString(From) : $"{ToGraphSafeString(From)}-{ToGraphSafeString(To)}";

        public bool Equals(CharRange? other)
        {
            if (other is null)
                return false;
            else if (ReferenceEquals(this, other))
                return true;

            return other.From == From && other.To == To;
        }

        public override bool Equals(object? obj) => obj is CharRange other && Equals(other);

        public override int GetHashCode() => unchecked((From.GetHashCode() * 397) ^ To.GetHashCode());

        private static string ToGraphSafeString(char c) => c >= 33 && c <= 0x7e ? c.ToString() : $"0x{(int)c:x2}";

        public static bool operator ==(CharRange left, CharRange right) => Equals(left, right);

        public static bool operator !=(CharRange left, CharRange right) => !(left == right);
    }
}