using System;
using System.Diagnostics;

namespace StringFormatEx
{
    internal static class EscapeEx
    {
        /// <summary>
        /// Escapes a string using an arbitrary look-up-table to determine whether to escape the character.
        /// </summary>
        /// <param name="unescaped">The string to escape.</param>
        /// <param name="escapePrefix">The characters representing the escape prefix.</param>
        /// <param name="escapePostfix">The characters representing the escape postfix.</param>
        /// <param name="escapeTable">The table used to determine whether to escape the character.</param>
        /// <param name="escapeLevel">The bit-mask required of any specific entry in the <see cref="escapeTable"/> to escape the character.</param>
        /// <returns>The escaped representation of the string.</returns>
        public static string Escape(this string unescaped, ReadOnlySpan<char> escapePrefix, ReadOnlySpan<char> escapePostfix, ReadOnlySpan<byte> escapeTable, byte escapeLevel)
        {
            Debug.Assert(escapeTable.Length >= 128, "escapeTable.Length >= 128");

            int tableLen = escapeTable.Length;
            for (var index = 0; index < unescaped.Length; index++)
            {
                char ch = unescaped[index];
                if (ch < tableLen && (escapeTable[ch] & escapeLevel) != 0)
                    return InternalEscape(unescaped, index, escapePrefix, escapePostfix, escapeTable, escapeLevel);
            }

            return unescaped;
        }

        private static string InternalEscape(ReadOnlySpan<char> unescaped, int startingIndex, ReadOnlySpan<char> escapePrefix, ReadOnlySpan<char> escapePostfix, ReadOnlySpan<byte> escapeTable, byte escapeLevel)
        {
            ValueStringBuilder sb = new(stackalloc char[unescaped.Length + 8]);
            sb.Append(unescaped.Slice(0, startingIndex));
            
            int tableLen = escapeTable.Length;
            int index = startingIndex;
            do
            {
                sb.Append(escapePrefix);
                sb.Append(unescaped[index]);
                sb.Append(escapePostfix);
                if (index == unescaped.Length - 1)
                    break;
                int pos = index;
                char ch;
                do
                {
                    ch = unescaped[++pos];
                } while (pos < unescaped.Length && (ch >= tableLen || (escapeTable[ch] & escapeLevel) == 0));

                sb.Append(unescaped.Slice(index, pos - index));
                index = pos;
            } while (index < unescaped.Length);

            return sb.ToString();
        }
        
    }
}
