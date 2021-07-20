using System;
using System.Collections.Generic;
using System.Diagnostics;
#if NETSTANDARD2_1
using System.Diagnostics.CodeAnalysis;
#endif

namespace StringFormatEx
{
    internal static class FormattingEx
    {
        /// <summary>
        /// Formats the string format using the given arguments.
        /// </summary>
        /// <param name="format">The string format.</param>
        /// <param name="provider">The <see cref="ICustomFormatter"/> provider.</param>
        /// <param name="arguments">The arguments ordered by <see cref="FormattingArgumentSymbolComparer"/>.</param>
        /// <param name="doNotThrowOnUnrecognizedSymbol">Whether to throw if a symbol is not found in the <paramref name="arguments"/> or not.</param>
        /// <param name="requireDollarForValidHole">Whether the formatting string uses the dollar sign prefix to indicate a hole or not.</param>
        /// <returns>The string <paramref name="format"/> where the holes are replaced by the <paramref name="arguments"/>.</returns>
        /// <remarks>
        ///     Curly brackets are only unescaped, if <c>!</c><paramref name="requireDollarForValidHole"/><c> &amp;&amp; !</c><paramref name="doNotThrowOnUnrecognizedSymbol"/>
        /// </remarks>
        public static string Format(string format, IFormatProvider? provider, in ReadOnlySpan<FormattingArgument> arguments, bool doNotThrowOnUnrecognizedSymbol = false, bool requireDollarForValidHole = false)
        {
            return Format(format, new FormattingArgumentCollection<object>(arguments, provider), doNotThrowOnUnrecognizedSymbol, requireDollarForValidHole ? 1 : 0);
        }

        /// <summary>
        /// Formats the string format using the given arguments.
        /// </summary>
        /// <param name="format">The string format.</param>
        /// <param name="provider">The <see cref="ICustomFormatter"/> provider.</param>
        /// <param name="arguments">The arguments dictionary.</param>
        /// <param name="doNotThrowOnUnrecognizedSymbol">Whether to throw if a symbol is not found in the <paramref name="arguments"/> or not.</param>
        /// <param name="requireDollarForValidHole">Whether the formatting string uses the dollar sign prefix to indicate a hole or not.</param>
        /// <returns>The string <paramref name="format"/> where the holes are replaced by the <paramref name="arguments"/>.</returns>
        /// <remarks>
        ///     Curly brackets are only unescaped, if <c>!</c><paramref name="requireDollarForValidHole"/><c> &amp;&amp; !</c><paramref name="doNotThrowOnUnrecognizedSymbol"/>
        /// </remarks>
        public static string Format<TValue>(string format, IFormatProvider? provider, IReadOnlyDictionary<string, TValue> arguments, bool doNotThrowOnUnrecognizedSymbol = false, bool requireDollarForValidHole = false)
        {
            return Format(format, new FormattingArgumentCollection<TValue>(arguments, provider), doNotThrowOnUnrecognizedSymbol, requireDollarForValidHole ? 1 : 0);
        }

        /// <summary></summary>
        /// <param name="format">Formatting string.</param>
        /// <param name="arguments">Sorted array of formatting arguments.</param>
        /// <param name="doNotThrowOnUnrecognizedSymbol">If <see langword="true"/> leaves arguments which are not replaced unchanged, otherwise; throws a <see cref="FormatException"/>.</param>
        /// <param name="requireDollarForValidHole">If <see langword="1"/> only recognises holes with a dollar-sign '$' prefix. e.g. ${name} is recognized, but ${{name}} and {name} is not, otherwise; behaves similar to <see cref="String.Format(string, object)"/>.</param>
        /// <returns></returns>
        /// <exception cref="FormatException">Incomplete format or argument in format string for which no replacement is provided if <paramref name="doNotThrowOnUnrecognizedSymbol"/> is <see langword="false"/>.</exception>
        /// <remarks>
        ///     Curly brackets are only unescaped, if <paramref name="requireDollarForValidHole"/><c>==0 &amp;&amp; !</c><paramref name="doNotThrowOnUnrecognizedSymbol"/>
        /// </remarks>
        private static string Format<T>(string format, in FormattingArgumentCollection<T> arguments, bool doNotThrowOnUnrecognizedSymbol, int requireDollarForValidHole)
        {
            Debug.Assert((requireDollarForValidHole & 1) == requireDollarForValidHole, "requireDollarForValidHole must be either zero or one.");

            ValueStringBuilder output = new(stackalloc char[Math.Min(4096, format.Length)]);
            ValueStringBuilder symbol = new(stackalloc char[32]);
            int index = 0;
            int length = format.Length;
            int holeStart = 0; // The index of the first char of the symbol inside the hole.
            int holeEnd = 0; // The index of the first character after the end of the last hole.
            bool enableHole = requireDollarForValidHole == 0; // Controls whether we recognize a hole as such or not. Always true when requireDollarForValidHole is zero.
            while (true)
            {
                while (index < length)
                {
                    char current = format[index];
                    index++;
                    // Do not use short-circuit operators for branching here: superscalar optimization
                    if (current == '$' & requireDollarForValidHole == 1)
                    {
                        enableHole = true;
                    }
                    else if (current == '{' & enableHole)
                    {
                        if (index < length && format[index] == '{')
                        {
                            index++;
                        }
                        else
                        {
                            holeStart = index;
                            continue;
                        }
                    }
                    else if (current == '}' & enableHole)
                    {
                        if (index < length && format[index] == '}')
                        {
                            index++;
                        }
                        else if (holeStart != 0)
                        {
                            index--;
                            break;
                        }
                        else
                        {
                            ThrowFormatError();
                        }
                    }

                    if (current != '$' & requireDollarForValidHole == 1)
                    {
                        enableHole = false;
                    }
                    
                    if (holeStart != 0 && (requireDollarForValidHole == 0 & !doNotThrowOnUnrecognizedSymbol))
                    {
                        // This is slower, but unescapes curly-brackets
                        output.Append(current);
                    }
                }

                if (index == length)
                {
                    break;
                }

                if (holeStart == 0)
                {
                    ThrowFormatError();
                }
                // holeStart points to the first char of the symbol, the first char after the {
                // index points to the } bracket enclosing the symbol.
                // -> index - holeStart is equal to the length of the symbol.
                Debug.Assert(holeStart - (1 << requireDollarForValidHole) >= holeEnd, "The hole-prefix ({ or ${) must be between the end of the previous hole and the start of the current hole.");

                if (requireDollarForValidHole == 1 | doNotThrowOnUnrecognizedSymbol)
                {
                    // This is faster, but does not unescape curly-brackets.
                    // Add a slice of format from the last hole end until the hole start.
                    // If we require a dollar sign, we must subtract two, otherwise one character to skip the hole.
                    output.Append(format.AsSpan(holeEnd, holeStart - holeEnd - (1 << requireDollarForValidHole)));
                }

                string syName = format.Substring(holeStart, index - holeStart);
                if (arguments.TryGetValue(syName, out string? value))
                {
                    output.Append(value);
                }
                else if (doNotThrowOnUnrecognizedSymbol)
                {
                    if (requireDollarForValidHole == 0)
                    {
                        output.Append('{');
                    }
                    else
                    {
                        output.Append("${");
                    }
                    output.Append(syName);
                    output.Append('}');
                }
                else
                {
                    ThrowFormatArgumentNotFound(syName);
                }

                // Cleanup
                symbol.Length = 0;
                holeStart = 0;
                holeEnd = index + 1;
                index = holeEnd;
                enableHole = requireDollarForValidHole == 0;
            }

            return output.ToString();
        }

#if NETSTANDARD2_1
        [DoesNotReturn]
#endif
        private static void ThrowFormatError()
        {
            throw new FormatException("Invalid string format.");
        }

#if NETSTANDARD2_1
        [DoesNotReturn]
#endif
        private static void ThrowFormatArgumentNotFound(string argumentName)
        {
            throw new FormatException($"Invalid string format the no argument with the symbol-name \"{argumentName}\" in the argument array.");
        }
    }
}
