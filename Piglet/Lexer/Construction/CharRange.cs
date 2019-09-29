using System;

namespace Piglet.Lexer.Construction
{
    internal class CharRange : IComparable<CharRange>
    {
        public char From { get; set; }
        public char To { get; set; }

        private static string ToGraphSafeString(char c) => c >= 33 && c <= 0x7e
                       ? c.ToString()
                       : string.Format("0x{0:x2}", (int)c);

        public int CompareTo(CharRange other)
    	{
    		int cmp = From - other.From;
    		return cmp == 0 ? To - other.To : cmp;
    	}

        public override string ToString() => From == To ? ToGraphSafeString(From) : string.Format("{0}-{1}", ToGraphSafeString(From), ToGraphSafeString(To));

        public bool Equals(CharRange other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return other.From == From && other.To == To;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (CharRange)) return false;
            return Equals((CharRange) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (From.GetHashCode()*397) ^ To.GetHashCode();
            }
        }

        public static bool operator ==(CharRange left, CharRange right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CharRange left, CharRange right)
        {
            return !Equals(left, right);
        }
    }
}