using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace StringFormatEx
{
    internal static class FormattingEx
    {
        /// <summary></summary>
        /// <param name="format">Formatting string.</param>
        /// <param name="provider">Format provider</param>
        /// <param name="arguments">Sorted array of formatting arguments.</param>
        /// <param name="doNotThrowOnMissingArgument">if <see langword="true"/> leaves arguments which are not replaced unchanged, otherwise; throws a <see cref="FormatException"/>.</param>
        /// <returns></returns>
        /// <exception cref="FormatException">Incomplete format or argument in format string for which no replacement is provided if <paramref name="doNotThrowOnMissingArgument"/> is <see langword="false"/>.</exception>
        public static string Format(string format, IFormatProvider? provider, ReadOnlySpan<FormattingArgument> arguments, bool doNotThrowOnMissingArgument)
        {
            ValueStringBuilder output = new(stackalloc char[256]);
            ValueStringBuilder symbol = new(stackalloc char[32]);
            var index = 0;
            int length = format.Length;
            var inHole = false;
            ICustomFormatter? formatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            while (true)
            {
                // Find hole
                while (index < length)
                {
                    char current = format[index];
                    index++;
                    if (current == '{')
                    {
                        if (index < length && format[index] == '{')
                        {
                            index++;
                        }
                        else
                        {
                            inHole = true;
                            continue;
                        }
                    }
                    else if (current == '}')
                    {
                        if (index < length && format[index] == '}')
                        {
                            index++;
                        }
                        else if (inHole)
                        {
                            index--;
                            break;
                        }
                        else
                        {
                            ThrowFormatError();
                        }
                    }

                    if (inHole)
                        symbol.Append(current);
                    else
                        output.Append(current);
                }

                if (index == length)
                    break;
                if (!inHole)
                    ThrowFormatError();

                // Find named argument in arguments
                string syName = symbol.ToString();
                FormattingArgument dummy = new(syName, null);
                
                int argumentIndex = arguments.BinarySearch(dummy, FormattingArgumentSymbolComparer.Default);
                if (argumentIndex >= 0)
                {
                    object? value = arguments[argumentIndex].Value;
                    // Append formatted string to sb
                    string? s = formatter == null ? value?.ToString() : formatter.Format("{0}", value, provider);
                    output.Append(s);
                }
                else if (doNotThrowOnMissingArgument)
                {
                    output.Append('{');
                    output.Append(syName);
                    output.Append('}');
                }
                else
                {
                    ThrowFormatArgumentNotFound(syName);
                }

                // Cleanup
                symbol.Length = 0;
                inHole = false;
                index++;
            }

            return output.ToString();
        }
        
        /// <summary></summary>
        /// <param name="format">Formatting string.</param>
        /// <param name="provider">Format provider</param>
        /// <param name="arguments">Sorted array of formatting arguments.</param>
        /// <param name="doNotThrowOnMissingArgument">if <see langword="true"/> leaves arguments which are not replaced unchanged, otherwise; throws a <see cref="FormatException"/>.</param>
        /// <returns></returns>
        /// <exception cref="FormatException">Incomplete format or argument in format string for which no replacement is provided if <paramref name="doNotThrowOnMissingArgument"/> is <see langword="false"/>.</exception>
        public static string Format<T>(string format, IFormatProvider? provider, IReadOnlyDictionary<string, T> arguments, bool doNotThrowOnMissingArgument)
        {
            ValueStringBuilder output = new(stackalloc char[256]);
            ValueStringBuilder symbol = new(stackalloc char[32]);
            var index = 0;
            int length = format.Length;
            var inHole = false;
            ICustomFormatter? formatter = (ICustomFormatter?)provider?.GetFormat(typeof(ICustomFormatter));
            while (true)
            {
                // Find hole
                while (index < length)
                {
                    char current = format[index];
                    index++;
                    if (current == '{')
                    {
                        if (index < length && format[index] == '{')
                        {
                            index++;
                        }
                        else
                        {
                            inHole = true;
                            continue;
                        }
                    }
                    else if (current == '}')
                    {
                        if (index < length && format[index] == '}')
                        {
                            index++;
                        }
                        else if (inHole)
                        {
                            index--;
                            break;
                        }
                        else
                        {
                            ThrowFormatError();
                        }
                    }

                    if (inHole)
                        symbol.Append(current);
                    else
                        output.Append(current);
                }

                if (index == length)
                    break;
                if (!inHole)
                    ThrowFormatError();

                // Find named argument in arguments
                string syName = symbol.ToString();
                if (arguments.TryGetValue(syName, out T? argument))
                {
                    // Append formatted string to sb
                    string? s = formatter == null ? argument?.ToString() : formatter.Format("{0}", argument, provider);
                    output.Append(s);
                }
                else if (doNotThrowOnMissingArgument)
                {
                    output.Append('{');
                    output.Append(syName);
                    output.Append('}');
                }
                else
                {
                    ThrowFormatArgumentNotFound(syName);
                }

                // Cleanup
                symbol.Length = 0;
                inHole = false;
                index++;
            }

            return output.ToString();
        }

        [DoesNotReturn]
        private static void ThrowFormatError()
        {
            throw new FormatException("Invalid string format.");
        }

        [DoesNotReturn]
        private static void ThrowFormatArgumentNotFound(string argumentName)
        {
            throw new FormatException($"Invalid string format the no argument with the symbol-name \"{argumentName}\" in the argument array.");
        }
    }

    public readonly struct FormattingArgument
    {
        public FormattingArgument(string symbol, object? value)
        {
            Symbol = symbol;
            Value = value;
        }

        public readonly string Symbol;

        public readonly object? Value;

        public static implicit operator FormattingArgument((string, object?) tuple)
        {
            return new(tuple.Item1, tuple.Item2);
        }
    }

    public sealed class FormattingArgumentSymbolComparer : IComparer<FormattingArgument>
    {
        public static FormattingArgumentSymbolComparer Default { get; } = new();

        public int Compare(FormattingArgument x, FormattingArgument y)
        {
            return String.Compare(x.Symbol, y.Symbol, StringComparison.Ordinal);
        }
    }
}
