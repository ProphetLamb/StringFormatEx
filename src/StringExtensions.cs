using System;
using System.Diagnostics;
using StringFormatEx.Helpers;

namespace StringFormatEx
{
    public static partial class StringExtensions
    {
        [ThreadStatic]
        private static FormattingArgument[]? s_arguments;

        private static ReadOnlySpan<FormattingArgument> GetArgumentSpan(
            int count,
            FormattingArgument arg0 = default,
            FormattingArgument arg1 = default,
            FormattingArgument arg2 = default,
            FormattingArgument arg3 = default)
        {
            s_arguments ??= new FormattingArgument[4];

            s_arguments[0] = arg0;
            s_arguments[1] = arg1;
            s_arguments[2] = arg2;
            s_arguments[3] = arg3;

            Span<FormattingArgument> span = s_arguments.AsSpan(0, count);
            span.InsertionSort(FormattingArgumentSymbolComparer.Default);
            return span;
        }

        /// <summary>
        ///     Replaces characters in a string with their respective replacement at the same index.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="replaceChars">The characters to replace.</param>
        /// <param name="withChars">The characters to replace with.</param>
        /// <returns>The source string with specific characters replaced with their respective replacement.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><c>replace.Length != with.Length</c>.</exception>
        public static string ReplaceMany(this string str, in ReadOnlySpan<char> replaceChars, in ReadOnlySpan<char> withChars)
        {
            if (replaceChars.Length != withChars.Length)
                throw new ArgumentOutOfRangeException(nameof(withChars), "Length has to be equal to replace.Length.");
            if (replaceChars.Length == 0)
                return str;
            for (int i = 0; i < str.Length; i++)
            {
                int replaceIndex = replaceChars.IndexOf(str[i]);
                if (replaceIndex >= 0)
                {
                    return InternalReplaceMany(str.AsSpan(), i, replaceIndex, replaceChars, withChars);
                }
            }
            return str;
        }

        /// <summary>
        ///     Replaces characters in a string with their respective replacement at the same index.
        /// </summary>
        /// <param name="span">The string.</param>
        /// <param name="replaceChars">The characters to replace.</param>
        /// <param name="withChars">The characters to replace with.</param>
        /// <returns>The source string with specific characters replaced with their respective replacement.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><c>replace.Length != with.Length</c>.</exception>
        public static string ReplaceMany(in this ReadOnlySpan<char> span, in ReadOnlySpan<char> replaceChars, in ReadOnlySpan<char> withChars)
        {
            if (replaceChars.Length != withChars.Length)
                throw new ArgumentOutOfRangeException(nameof(withChars), "Length has to be equal to replace.Length.");
            if (replaceChars.Length == 0)
                return span.ToString();
            for (int i = 0; i < span.Length; i++)
            {
                int replaceIndex = replaceChars.IndexOf(span[i]);
                if (replaceIndex >= 0)
                {
                    return InternalReplaceMany(span, i, replaceIndex, replaceChars, withChars);
                }
            }

            return span.ToString();
        }

        private static string InternalReplaceMany(in ReadOnlySpan<char> span, int startIndex, int replaceIndex, in ReadOnlySpan<char> replaceChars, in ReadOnlySpan<char> withChars)
        {
            Span<char> buffer = stackalloc char[span.Length];
            span.CopyTo(buffer); // Assume replacement density to be low.
            
            Debug.Assert((uint)replaceIndex < (uint)withChars.Length);
            buffer[startIndex] = withChars[replaceIndex];
            
            for (int i = startIndex + 1; i < buffer.Length; i++)
            {
                replaceIndex = replaceChars.IndexOf(buffer[i]);
                if (replaceIndex >= 0)
                {
                    buffer[i] = withChars[replaceIndex];
                }
            }

            return buffer.ToString();
        }

        /// <summary>
        ///     Surrounds the string with quotes.
        /// </summary>
        public static string Quote(this string str, bool trimExistingQuotes = true) => Quote(str.AsSpan(), trimExistingQuotes);

        /// <summary>
        ///     Surrounds the string with quotes.
        /// </summary>
        public static string Quote(in this ReadOnlySpan<char> span, bool trimExistingQuotes = true)
        {
            ValueStringBuilder sb = new(stackalloc char[Math.Min(4096, span.Length + 2)]);
            sb.Append('\"');
            sb.Append(trimExistingQuotes ? span.Trim('\"') : span);
            sb.Append('\"');
            return sb.ToString();
        }

        /// <summary>
        ///     Surrounds the string with curly brackets. 
        /// </summary>
        public static string Symbolize(in this ReadOnlySpan<char> span, bool trimExistingCurlyBrackets = true, bool prefixDollarSign = false)
        {
            ValueStringBuilder sb = new(stackalloc char[Math.Min(4096, span.Length + 2)]);
            if (prefixDollarSign)
                sb.Append('$');
            sb.Append('{');
            sb.Append(trimExistingCurlyBrackets ? span.TrimStart('{').TrimEnd('}') : span);
            sb.Append('}');
            return sb.ToString();
        }
    }
}
