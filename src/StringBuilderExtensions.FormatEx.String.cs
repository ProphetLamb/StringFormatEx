using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using StringFormatEx.Helpers;

namespace StringFormatEx
{
    public static partial class StringBuilderExtensions
    {
        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="builder">The string builder.</param>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="arg0">The argument to replace.</param>
        /// <param name="provider">The provider providing the <see cref="ICustomFormatter"/> formatting the values inserted into holes.</param>
        /// <param name="options">The formatting options.</param>
        [Pure]
        public static StringBuilder AppendFormatEx(
            this StringBuilder builder,
            string format,
            in FormattingArgument arg0,
            IFormatProvider? provider = default,
            StringFormattingOptions options = StringFormattingOptions.None)
        {
            return builder.Append(builder.Append(FormattingEx.Format(format.AsSpan(), provider, options, GetArgumentSpan(1, arg0))));
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="builder">The string builder.</param>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="arg0">The first argument.</param>
        /// <param name="arg1">The second argument.</param>
        /// <param name="provider">The provider providing the <see cref="ICustomFormatter"/> formatting the values inserted into holes.</param>
        /// <param name="options">The formatting options.</param>
        [Pure]
        public static StringBuilder AppendFormatEx(
            this StringBuilder builder,
            string format,
            in FormattingArgument arg0,
            in FormattingArgument arg1,
            IFormatProvider? provider = default,
            StringFormattingOptions options = StringFormattingOptions.None)
        {
            return builder.Append(FormattingEx.Format(format.AsSpan(), provider, options, GetArgumentSpan(2, arg0, arg1)));
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="builder">The string builder.</param>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="arg0">The first argument.</param>
        /// <param name="arg1">The second argument.</param>
        /// <param name="arg2">The third argument.</param>
        /// <param name="provider">The provider providing the <see cref="ICustomFormatter"/> formatting the values inserted into holes.</param>
        /// <param name="options">The formatting options.</param>
        [Pure]
        public static StringBuilder AppendFormatEx(
            this StringBuilder builder,
            string format,
            in FormattingArgument arg0,
            in FormattingArgument arg1,
            in FormattingArgument arg2,
            IFormatProvider? provider = default,
            StringFormattingOptions options = StringFormattingOptions.None)
        {
            return builder.Append(FormattingEx.Format(format.AsSpan(), provider, options, GetArgumentSpan(3, arg0, arg1, arg2)));
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="builder">The string builder.</param>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="arg0">The first argument.</param>
        /// <param name="arg1">The second argument.</param>
        /// <param name="arg2">The third argument.</param>
        /// <param name="arg3">The forth argument.</param>
        /// <param name="provider">The provider providing the <see cref="ICustomFormatter"/> formatting the values inserted into holes.</param>
        /// <param name="options">The formatting options.</param>
        [Pure]
        public static StringBuilder AppendFormatEx(
            this StringBuilder builder,
            string format,
            in FormattingArgument arg0,
            in FormattingArgument arg1,
            in FormattingArgument arg2,
            in FormattingArgument arg3,
            IFormatProvider? provider = default,
            StringFormattingOptions options = StringFormattingOptions.None)
        {
            return builder.Append(FormattingEx.Format(format.AsSpan(), provider, options, GetArgumentSpan(4, arg0, arg1, arg2, arg3)));
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="builder">The string builder.</param>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="args">The params array containing the unsorted formatting arguments.</param>
        /// <param name="provider">The provider providing the <see cref="ICustomFormatter"/> formatting the values inserted into holes.</param>
        /// <param name="options">The formatting options.</param>
        /// <remarks>
        ///     This will sort <paramref name="args" />.
        ///     If symbols are not distinct the first occurence will be used.
        /// </remarks>
        public static StringBuilder AppendFormatEx(
            this StringBuilder builder,
            string format,
            IFormatProvider? provider,
            StringFormattingOptions options,
            params FormattingArgument[] args)
        {
            Array.Sort(args, FormattingArgumentSymbolComparer.Default);
            return builder.Append(FormattingEx.Format(format.AsSpan(), provider, options, args));
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="builder">The string builder.</param>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="args">The span containing the unsorted formatting arguments.</param>
        /// <param name="provider">The provider providing the <see cref="ICustomFormatter"/> formatting the values inserted into holes.</param>
        /// <param name="options">The formatting options.</param>
        /// <remarks>
        ///     This will sort <paramref name="args" />.
        ///     If symbols are not distinct the first occurence will be used.
        /// </remarks>
        public static StringBuilder AppendFormatEx(
            this StringBuilder builder,
            string format,
            IFormatProvider? provider,
            StringFormattingOptions options,
            in Span<FormattingArgument> args)
        {
            args.InsertionSort(FormattingArgumentSymbolComparer.Default);
            return builder.Append(FormattingEx.Format(format.AsSpan(), provider, options, args));
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="builder">The string builder.</param>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="args">The dictionary mapping the formatting symbols to their respective values.</param>
        /// <param name="provider">The provider providing the <see cref="ICustomFormatter"/> formatting the values inserted into holes.</param>
        /// <param name="options">The formatting options.</param>
        [Pure]
        public static StringBuilder AppendFormatEx<T>(
            this StringBuilder builder,
            string format,
            IFormatProvider? provider,
            StringFormattingOptions options,
            IReadOnlyDictionary<string, T> args)
        {
            return builder.Append(FormattingEx.Format(format.AsSpan(), provider, options, args));
        }
    }
}
