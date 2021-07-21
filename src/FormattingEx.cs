using System;
using System.Collections.Generic;
using System.Diagnostics;
#if NETSTANDARD2_1
using System.Diagnostics.CodeAnalysis;
#endif

using StringFormatEx.Helpers;

namespace StringFormatEx
{
    internal static class FormattingEx
    {
        /// <summary>
        /// Formats the string format using the given arguments.
        /// </summary>
        /// <param name="format">The string format.</param>
        /// <param name="provider">The <see cref="ICustomFormatter"/> provider.</param>
        /// <param name="options">The formatting options</param>
        /// <param name="arguments">The arguments ordered by <see cref="FormattingArgumentSymbolComparer"/>.</param>
        /// <returns>The string <paramref name="format"/> where the holes are replaced by the <paramref name="arguments"/>.</returns>
        public static string Format(in ReadOnlySpan<char> format, IFormatProvider? provider, StringFormattingOptions options, in ReadOnlySpan<FormattingArgument> arguments)
        {
            return Format(format,
                new FormattingArgumentCollection<object>(arguments, provider),
                (options & StringFormattingOptions.DoNotThrowOnUnrecognisedSymbol) != 0,
                (options & StringFormattingOptions.DollarLiteralHoleMode) != 0 ? 1 : 0);
        }

        /// <summary>
        /// Formats the string format using the given arguments.
        /// </summary>
        /// <param name="format">The string format.</param>
        /// <param name="provider">The <see cref="ICustomFormatter"/> provider.</param>
        /// <param name="arguments">The arguments dictionary.</param>
        /// <param name="options">The formatting options</param>
        /// <returns>The string <paramref name="format"/> where the holes are replaced by the <paramref name="arguments"/>.</returns>
        public static string Format<TValue>(in ReadOnlySpan<char> format, IFormatProvider? provider, StringFormattingOptions options, IReadOnlyDictionary<string, TValue> arguments)
        {
            return Format(format,
                new FormattingArgumentCollection<TValue>(arguments, provider),
                (options & StringFormattingOptions.DoNotThrowOnUnrecognisedSymbol) != 0,
                (options & StringFormattingOptions.DollarLiteralHoleMode) != 0 ? 1 : 0);
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
        private static string Format<T>(in ReadOnlySpan<char> format, in FormattingArgumentCollection<T> arguments, bool doNotThrowOnUnrecognizedSymbol, int requireDollarForValidHole)
        {
            Debug.Assert((requireDollarForValidHole & 1) == requireDollarForValidHole, "requireDollarForValidHole must be either zero or one.");

            ValueStringBuilder output = new(stackalloc char[Math.Min(4096, format.Length)]);
            ValueStringBuilder symbol = new(stackalloc char[32]);
            int index = 0;
            int length = format.Length;
            int holeStart = 0; // The index of the first char of the symbol inside the hole.
            int holeEnd = 0; // The index of the first character after the end of the last hole.
            bool enableHole = requireDollarForValidHole == 0; // Controls whether we recognize a hole as such or not. Always true when requireDollarForValidHole is zero.
            bool atomicAppend = requireDollarForValidHole == 0 & !doNotThrowOnUnrecognizedSymbol; // Indicates whether we append each char or spans between holes. This controls whether we escape brackets or not,
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
                    else if (current == '}')
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
                    
                    if (holeStart != 0 & atomicAppend)
                    {
                        // This is slower, but unescapes curly-brackets
                        output.Append(current);
                    }
                }

                if (index == length)
                {
                    // Finalize the output string.
                    if (!atomicAppend)
                    {
                        output.Append(format.Slice(holeEnd));
                    }
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

                if (!atomicAppend)
                {
                    // This is faster, but does not unescape curly-brackets.
                    // Add a slice of format from the last hole end until the hole start.
                    // If we require a dollar sign, we must subtract two, otherwise one character to skip the hole.
                    output.Append(format.Slice(holeEnd, holeStart - holeEnd - (1 << requireDollarForValidHole)));
                }

                string syName = format.Slice(holeStart, index - holeStart).ToString();
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
