using System;
using System.Diagnostics.Contracts;

namespace StringFormatEx
{
    public static partial class StringExtensions
    {
        private const byte A = 255;
        private const byte B = 253;

        private static ReadOnlySpan<byte> CLikeEscapeTable => new byte[]
        {
            B, 0, 0, 0, 0, 0, 0, 0, B, B, B, B, 0, B, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
         //    !  "  #  $  %  &  '  (  )  *  +  ,  -  .  /  0  1  2  3  4  5  6  7  8  9  :  ;  <  =  >  ?
            0, 0, A, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
         // @  A  B  C  D  E  F  G  H  I  J  K  L  M  N  O  P  Q  R  S  T  U  V  W  X  Y  Z  [  \  ]  ^  _
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, B, 0, 0,
         // '  a  b  c  d  e  f  g  h  i  j  k  l  m  n  o  p  q  r  s  t  u  v  w  x  y  z  {  |  }  ~
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        /// <summary>
        /// Escapes the following characters with a back-slash (<c>'\\'</c>): <c>'\0', '\b', '\t', '\n', '\v', '\\', '\r'. '\"'</c> 
        /// </summary>
        /// <param name="unescaped">The string to escape.</param>
        /// <returns>The escaped representation of the string.</returns>
        [Pure]
        public static string Escape(in this ReadOnlySpan<char> unescaped)
        {
            return Escape(unescaped, "\\".AsSpan(), ReadOnlySpan<char>.Empty, CLikeEscapeTable, 1);
        }

        /// <summary>
        /// Escapes the following characters with a back-slash (<c>'\\'</c>): <c>'\0', '\b', '\t', '\n', '\v', '\\', '\r'. '\"'</c> 
        /// </summary>
        /// <param name="unescaped">The string to escape.</param>
        /// <returns>The escaped representation of the string.</returns>
        [Pure]
        public static string Escape(this string unescaped) => Escape(unescaped.AsSpan());

        /// <summary>
        /// Escapes quotes with a back-slash (<c>'\\'</c>).
        /// </summary>
        /// <param name="unescaped">The string to escape.</param>
        /// <returns>The escaped representation of the string.</returns>
        [Pure]
        public static string EscapeQuotes(in this ReadOnlySpan<char> unescaped)
        {
            return Escape(unescaped, "\\".AsSpan(), ReadOnlySpan<char>.Empty, CLikeEscapeTable, 2);
        }

        /// <summary>
        /// Escapes quotes with a back-slash (<c>'\\'</c>).
        /// </summary>
        /// <param name="unescaped">The string to escape.</param>
        /// <returns>The escaped representation of the string.</returns>
        [Pure]
        public static string EscapeQuotes(this string unescaped) => EscapeQuotes(unescaped.AsSpan()); 

        /// <summary>
        /// Escapes a string using an arbitrary look-up-table to determine whether to escape the character.
        /// </summary>
        /// <param name="unescaped">The string to escape.</param>
        /// <param name="escapePrefix">The characters representing the escape prefix.</param>
        /// <param name="escapePostfix">The characters representing the escape postfix.</param>
        /// <param name="escapeTable">The table used to determine whether to escape the character.</param>
        /// <param name="escapeLevel">The bit-mask required of any specific entry in the <see cref="escapeTable"/> to escape the character.</param>
        /// <returns>The escaped representation of the string.</returns>
        [Pure]
        public static string Escape(
            in this ReadOnlySpan<char> unescaped,
            ReadOnlySpan<char> escapePrefix,
            ReadOnlySpan<char> escapePostfix,
            ReadOnlySpan<byte> escapeTable,
            byte escapeLevel)
        {
            return EscapeEx.Escape(unescaped, escapePrefix, escapePostfix, escapeTable, escapeLevel);
        }

        /// <summary>
        /// Escapes a string using an arbitrary look-up-table to determine whether to escape the character.
        /// </summary>
        /// <param name="unescaped">The string to escape.</param>
        /// <param name="escapePrefix">The characters representing the escape prefix.</param>
        /// <param name="escapePostfix">The characters representing the escape postfix.</param>
        /// <param name="escapeTable">The table used to determine whether to escape the character.</param>
        /// <param name="escapeLevel">The bit-mask required of any specific entry in the <see cref="escapeTable"/> to escape the character.</param>
        /// <returns>The escaped representation of the string.</returns>
        [Pure]
        public static string Escape(
            this string unescaped,
            ReadOnlySpan<char> escapePrefix,
            ReadOnlySpan<char> escapePostfix,
            ReadOnlySpan<byte> escapeTable,
            byte escapeLevel)
        {
            return EscapeEx.Escape(unescaped, escapePrefix, escapePostfix, escapeTable, escapeLevel);
        }
    }
}
