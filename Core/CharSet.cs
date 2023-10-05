using System;
using System.Text;

using Sudoku.Utility;

namespace Sudoku.Core
{
    internal struct CharSet
    {
        public static readonly CharSet Empty = new CharSet(0UL);

        ulong data;
        public CharSet(string s)
        {
            data = 0UL;
            if (s != null)
                for (int index = 0, ceil = s.Length; index < ceil; index++)
                    this[s[index]] = true;
        }

        public CharSet(char c)
        {
            data = 0UL;
            this[c] = true;
        }

        CharSet(ulong data)
        {
            this.data = data;
        }

        public bool this[char c]
        {
            get
            {
                int index = IndexOf(c, Localization.MAX_CHARSET);
                if (index == -1)
                    return false;

                return ((data >> index) & 1UL) == 1UL;
            }
            set
            {
                int index = IndexOf(c, Localization.MAX_CHARSET);
                if (index == -1)
                    return;

                if (value)
                    data |= (1UL << index);
                else
                    data &= ~(1UL << index);
            }
        }

        static int IndexOf(char c, string s)
        {
            int index = s.Length;
            while (--index > -1)
                if (c == s[index])
                    break;
            return index;
        }

        /// <summary>
        /// Returns the index-th least significant character in the set
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public CharSet this[int index]
        {
            get
            {
                ulong data = this.data;
                while (index > 0)
                {
                    data &= data - 1UL;
                    index--;
                }
                return new CharSet(data ^ (data & (data - 1UL)));
            }
        }

        public int Size
        {
            get
            {
                int result = 0;
                ulong data = this.data;
                while (data != 0UL)
                {
                    data &= data - 1UL;
                    result++;
                }
                return result;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return data == 0UL;
            }
        }

        public bool IsSingleton
        {
            get
            {
                return data != 0UL && (data & (data - 1UL)) == 0UL;
            }
        }

        public bool HasMultipleElements
        {
            get
            {
                return (data & (data - 1UL)) != 0UL;
            }
        }

        public static CharSet operator ~(CharSet value)
        {
            return new CharSet(~value.data);
        }

        public static CharSet operator !(CharSet value)
        {
            return new CharSet(~value.data);
        }

        public static CharSet operator &(CharSet left, CharSet right)
        {
            return new CharSet(left.data & right.data);
        }

        public static CharSet operator |(CharSet left, CharSet right)
        {
            return new CharSet(left.data | right.data);
        }

        public static CharSet operator ^(CharSet left, CharSet right)
        {
            return new CharSet(left.data ^ right.data);
        }

        public static CharSet operator +(CharSet left, CharSet right)
        {
            return new CharSet(left.data | right.data);
        }

        public static CharSet operator -(CharSet left, CharSet right)
        {
            return new CharSet(left.data & ~right.data);
        }

        public static bool operator ==(CharSet left, CharSet right)
        {
            return left.data == right.data;
        }

        public static bool operator !=(CharSet left, CharSet right)
        {
            return left.data != right.data;
        }

        public static bool operator <=(CharSet left, CharSet right)
        {
            return (left.data | right.data) == right.data;
        }

        public static bool operator >=(CharSet left, CharSet right)
        {
            return (left.data | right.data) == left.data;
        }

        public override bool Equals(object obj)
        {
            return obj is CharSet charSet && charSet == this;
        }

        public override int GetHashCode()
        {
            return data.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(64, 64);

            int index = 0;
            for (ulong data = this.data; data != 0UL; data >>= 1, index++)
                if ((data & 1UL) == 1UL)
                    sb.Append(Localization.MAX_CHARSET[index]);

            return sb.ToString();
        }
    }
}