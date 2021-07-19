using System;
using System.Collections.Generic;
#if NETSTANDARD2_1
using System.Diagnostics.CodeAnalysis;
#endif
using StringFormatEx.Helpers;

namespace StringFormatEx
{
    public static class StringExtensions
    {
        private const byte A = 255;
        private const byte B = 253;
        [ThreadStatic]
        private static FormattingArgument[]? s_arguments;

        public static ReadOnlySpan<byte> CEscapeTable => new byte[] {
            B, 0, 0, 0, 0, 0, 0, 0, B, B, B, B, 0, B, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
         //    !  "  #  $  %  &  '  (  )  *  +  ,  -  .  /  0  1  2  3  4  5  6  7  8  9  :  ;  <  =  >  ?
            0, 0, A, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
         // @  A  B  C  D  E  F  G  H  I  J  K  L  M  N  O  P  Q  R  S  T  U  V  W  X  Y  Z  [  \  ]  ^  _
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, B, 0, 0,
         // '  a  b  c  d  e  f  g  h  i  j  k  l  m  n  o  p  q  r  s  t  u  v  w  x  y  z  {  |  }  ~
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

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

            return s_arguments.AsSpan(0, count).InsertionSort(FormattingArgumentSymbolComparer.Default);
        }

        /// <summary>
        ///     Replaces the format items in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="arg0">The object to format.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format items have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string Format(this string format, object? arg0)
        {
            return String.Format(format, arg0);
        }

        /// <summary>
        ///     Replaces the format items in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format items have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string Format(this string format, object? arg0, object? arg1)
        {
            return String.Format(format, arg0, arg1);
        }

        /// <summary>
        ///     Replaces the format items in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>>
        /// <param name="arg2">The third object to format.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format items have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string Format(this string format, object? arg0, object? arg1, object? arg2)
        {
            return String.Format(format, arg0, arg1, arg2);
        }

        /// <summary>
        ///     Replaces the format items in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="arg1">The second object to format.</param>
        /// <param name="arg2">The third object to format.</param>
        /// <param name="arg3">The forth object to format.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format items have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string Format(this string format, object? arg0, object? arg1, object? arg2, object? arg3)
        {
            return String.Format(format, arg0, arg1, arg2, arg3);
        }

        /// <summary>
        ///     Replaces the format items in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="args">The objects to format.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format items have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string Format(this string format, params object?[] args)
        {
            return String.Format(format, args);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="args">The params array containing the unsorted formatting arguments.</param>
        /// <remarks>
        ///     This will sort <paramref name="args" />.
        ///     If symbols are not distinct the first occurence will be used.
        /// </remarks>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAll(this string format, params FormattingArgument[] args)
        {
            return FormatAll(format, null, args);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="args">The dictionary mapping the formatting symbols to their respective values.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAll<T>(this string format, IReadOnlyDictionary<string, T> args)
        {
            return FormatAll(format, null, args);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="arg0">The argument to replace.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAll(this string format, in FormattingArgument arg0)
        {
            return FormattingEx.Format(format, null, GetArgumentSpan(1, arg0), false);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="arg0">The first argument.</param>
        /// <param name="arg1">The second argument.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAll(this string format, in FormattingArgument arg0, in FormattingArgument arg1)
        {
            return FormattingEx.Format(format, null, GetArgumentSpan(2, arg0, arg1), false);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="arg0">The first argument.</param>
        /// <param name="arg1">The second argument.</param>
        /// <param name="arg2">The third argument.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAll(this string format, in FormattingArgument arg0, in FormattingArgument arg1, in FormattingArgument arg2)
        {
            return FormattingEx.Format(format, null, GetArgumentSpan(3, arg0, arg1, arg2), false);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="arg0">The first argument.</param>
        /// <param name="arg1">The second argument.</param>
        /// <param name="arg2">The third argument.</param>
        /// <param name="arg3">The forth argument.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAll(this string format, in FormattingArgument arg0, in FormattingArgument arg1, in FormattingArgument arg2, in FormattingArgument arg3)
        {
            return FormattingEx.Format(format, null, GetArgumentSpan(4, arg0, arg1, arg2, arg3), false);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="provider">A object that supplies culture-specific formatting information.</param>
        /// <param name="args">The params array containing the unsorted formatting arguments.</param>
        /// <remarks>
        ///     This will sort <paramref name="args" />.
        ///     If symbols are not distinct the first occurence will be used.
        /// </remarks>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAll(this string format, IFormatProvider? provider, params FormattingArgument[] args)
        {
            Array.Sort(args, FormattingArgumentSymbolComparer.Default);
            return FormattingEx.Format(format, provider, args, false);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="provider">A object that supplies culture-specific formatting information.</param>
        /// <param name="args">The dictionary mapping the formatting symbols to their respective values.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAll<T>(this string format, IFormatProvider? provider, IReadOnlyDictionary<string, T> args)
        {
            return FormattingEx.Format(format, provider, args, false);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="provider">A object that supplies culture-specific formatting information.</param>
        /// <param name="arg0">The argument to replace.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAll(this string format, IFormatProvider? provider, in FormattingArgument arg0)
        {
            return FormattingEx.Format(format, provider, GetArgumentSpan(0, arg0), false);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="provider">A object that supplies culture-specific formatting information.</param>
        /// <param name="arg0">The first argument.</param>
        /// <param name="arg1">The second argument.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAll(this string format, IFormatProvider? provider, in FormattingArgument arg0, in FormattingArgument arg1)
        {
            return FormattingEx.Format(format, provider, GetArgumentSpan(2, arg0, arg1), false);
        }
        
        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="provider">A object that supplies culture-specific formatting information.</param>
        /// <param name="arg0">The first argument.</param>
        /// <param name="arg1">The second argument.</param>
        /// <param name="arg2">The third argument.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAll(this string format, IFormatProvider? provider, in FormattingArgument arg0, in FormattingArgument arg1, in FormattingArgument arg2)
        {
            return FormattingEx.Format(format, provider, GetArgumentSpan(3, arg0, arg1, arg2), false);
        }
        
        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="provider">A object that supplies culture-specific formatting information.</param>
        /// <param name="arg0">The first argument.</param>
        /// <param name="arg1">The second argument.</param>
        /// <param name="arg2">The third argument.</param>
        /// <param name="arg3">The forth argument.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAll(this string format, IFormatProvider? provider, in FormattingArgument arg0, in FormattingArgument arg1, in FormattingArgument arg2, in FormattingArgument arg3)
        {
            return FormattingEx.Format(format, provider, GetArgumentSpan(4, arg0, arg1, arg2, arg3), false);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="args">The params array containing the unsorted formatting arguments.</param>
        /// <remarks>
        ///     This will sort <paramref name="args" />.
        ///     If symbols are not distinct the first occurence will be used.
        /// </remarks>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAny(this string format, params FormattingArgument[] args)
        {
            return FormatAny(format, null, args);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="args">The dictionary mapping the formatting symbols to their respective values.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAny<T>(this string format, IReadOnlyDictionary<string, T> args)
        {
            return FormatAny(format, null, args);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="arg0">The argument to replace.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAny(this string format, in FormattingArgument arg0)
        {
            return FormattingEx.Format(format, null, GetArgumentSpan(1, arg0), true);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="arg0">The first argument.</param>
        /// <param name="arg1">The second argument.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAny(this string format, in FormattingArgument arg0, in FormattingArgument arg1)
        {
            return FormattingEx.Format(format, null, GetArgumentSpan(2, arg0, arg1), true);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="arg0">The first argument.</param>
        /// <param name="arg1">The second argument.</param>
        /// <param name="arg2">The third argument.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAny(this string format, in FormattingArgument arg0, in FormattingArgument arg1, in FormattingArgument arg2)
        {
            return FormattingEx.Format(format, null, GetArgumentSpan(3, arg0, arg1, arg2), true);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="arg0">The first argument.</param>
        /// <param name="arg1">The second argument.</param>
        /// <param name="arg2">The third argument.</param>
        /// <param name="arg3">The forth argument.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAny(this string format, in FormattingArgument arg0, in FormattingArgument arg1, in FormattingArgument arg2, in FormattingArgument arg3)
        {
            return FormattingEx.Format(format, null, GetArgumentSpan(4, arg0, arg1, arg2, arg3), true);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="provider">A object that supplies culture-specific formatting information.</param>
        /// <param name="args">The params array containing the unsorted formatting arguments.</param>
        /// <remarks>
        ///     This will sort <paramref name="args" />.
        ///     If symbols are not distinct the first occurence will be used.
        /// </remarks>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAny(this string format, IFormatProvider? provider, params FormattingArgument[] args)
        {
            Array.Sort(args, FormattingArgumentSymbolComparer.Default);
            return FormattingEx.Format(format, provider, args, true);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="provider">A object that supplies culture-specific formatting information.</param>
        /// <param name="args">The dictionary mapping the formatting symbols to their respective values.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAny<T>(this string format, IFormatProvider? provider, IReadOnlyDictionary<string, T> args)
        {
            return FormattingEx.Format(format, provider, args, true);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="provider">A object that supplies culture-specific formatting information.</param>
        /// <param name="arg0">The argument to replace.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAny(this string format, IFormatProvider? provider, in FormattingArgument arg0)
        {
            return FormattingEx.Format(format, provider, GetArgumentSpan(1, arg0), true);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="provider">A object that supplies culture-specific formatting information.</param>
        /// <param name="arg0">The first argument.</param>
        /// <param name="arg1">The second argument.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAny(this string format, IFormatProvider? provider, in FormattingArgument arg0, in FormattingArgument arg1)
        {
            return FormattingEx.Format(format, provider, GetArgumentSpan(2, arg0, arg1), true);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="provider">A object that supplies culture-specific formatting information.</param>
        /// <param name="arg0">The first argument.</param>
        /// <param name="arg1">The second argument.</param>
        /// <param name="arg2">The third argument.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAny(this string format, IFormatProvider? provider, in FormattingArgument arg0, in FormattingArgument arg1, in FormattingArgument arg2)
        {
            return FormattingEx.Format(format, provider, GetArgumentSpan(2, arg0, arg1, arg2), true);
        }

        /// <summary>
        ///     Replaces the format symbols in a string with the replacement objects.
        /// </summary>
        /// <param name="format">A composite format <see langword="string" />.</param>
        /// <param name="provider">A object that supplies culture-specific formatting information.</param>
        /// <param name="arg0">The first argument.</param>
        /// <param name="arg1">The second argument.</param>
        /// <param name="arg2">The third argument.</param>
        /// <param name="arg3">The forth argument.</param>
        /// <returns>
        ///     A copy of <paramref name="format" /> in which the format symbols have been replaced by their respective string
        ///     representation.
        /// </returns>
        public static string FormatAny(this string format, IFormatProvider? provider, in FormattingArgument arg0, in FormattingArgument arg1, in FormattingArgument arg2, in FormattingArgument arg3)
        {
            return FormattingEx.Format(format, provider, GetArgumentSpan(2, arg0, arg1, arg2, arg3), true);
        }
        
        /// <summary>
        /// Concatenates the members of a collection, using the specified separator between each member.
        /// </summary>
        /// <param name="separator">The string to use as a separator. <paramref name="separator"/> is included in the returned string only if <paramref name="values"/> has more than one element.</param>
        /// <param name="values">A collection that contains the objects to concatenate.</param>
        /// <typeparam name="T">The type of the members of <paramref name="values"/>.</typeparam>
        /// <returns>A string that consists of the members of <paramref name="values"/> delimited by the <paramref name="separator"/> string. If <paramref name="values"/> has no members, the method returns <see cref="String.Empty"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is null.</exception>
        /// <exception cref="OutOfMemoryException">The length of the resulting string overflows the maximum allowed length (<see cref="Int32.MaxValue"/>).</exception>
#if NETSTANDARD2_1
        [return: NotNullIfNotNull("values")]
#endif
        public static string? Join<T>(this string? separator,  IEnumerable<T>? values)
        {
            return values == null ? null : String.Join(separator, values);
        }

        /// <summary>
        /// Concatenates the members of a collection, using the specified separator between each member.
        /// </summary>
        /// <param name="separator">The character to use as a separator. <paramref name="separator"/> is included in the returned string only if <paramref name="values"/> has more than one element.</param>
        /// <param name="values">A collection that contains the objects to concatenate.</param>
        /// <typeparam name="T">The type of the members of <paramref name="values"/>.</typeparam>
        /// <returns>A string that consists of the members of <paramref name="values"/> delimited by the <paramref name="separator"/> string. If <paramref name="values"/> has no members, the method returns <see cref="String.Empty"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="values"/> is null.</exception>
        /// <exception cref="OutOfMemoryException">The length of the resulting string overflows the maximum allowed length (<see cref="Int32.MaxValue"/>).</exception>
#if NETSTANDARD2_1
        [return: NotNullIfNotNull("values")]
#endif
        public static string? Join<T>(this char separator, IEnumerable<T>? values)
        {
#if NETSTANDARD2_1
            return values == null ? null : String.Join(separator, values);
#else
            return values == null ? null : String.Join(separator.ToString(), values);
#endif
        }

        /// <summary>
        ///     Replaces characters in a string with their respective replacement at the same index.
        /// </summary>
        /// <param name="source">The string.</param>
        /// <param name="replace">The characters to replace.</param>
        /// <param name="with">The characters to replace with.</param>
        /// <returns>The source string with specific characters replaced with their respective replacement.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><c>replace.Length != with.Length</c>.</exception>
        public static string ReplaceMany(this string source, ReadOnlySpan<char> replace, ReadOnlySpan<char> with)
        {
            if (replace.Length != with.Length)
                throw new ArgumentOutOfRangeException(nameof(with), "Length has to be equal to replace.Length.");
            if (replace.Length == 0)
                return source;
            Span<char> buffer = stackalloc char[source.Length];
            source.AsSpan().CopyTo(buffer);
            
            for (var i = 0; i < buffer.Length; i++)
            {
                int replaceIndex = replace.IndexOf(buffer[i]);
                if (replaceIndex >= 0)
                {
                    buffer[i] = with[replaceIndex];
                }
            }

            return buffer.ToString();
        }

        /// <summary>
        /// Escapes the following characters with a back-slash (<c>'\\'</c>): <c>'\0', '\b', '\t', '\n', '\v', '\\', '\r'. '\"'</c> 
        /// </summary>
        /// <param name="unescaped">The string to escape.</param>
        /// <returns>The escaped representation of the string.</returns>
        public static string Escape(this string unescaped)
        {
            return Escape(unescaped, "\\".AsSpan(), ReadOnlySpan<char>.Empty, CEscapeTable, 1);
        }
        
        /// <summary>
        /// Escapes quotes with a back-slash (<c>'\\'</c>).
        /// </summary>
        /// <param name="unescaped">The string to escape.</param>
        /// <returns>The escaped representation of the string.</returns>
        public static string EscapeQuotes(this string unescaped)
        {
            return Escape(unescaped, "\\".AsSpan(), ReadOnlySpan<char>.Empty, CEscapeTable, 2);
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
        public static string Escape(this string unescaped, ReadOnlySpan<char> escapePrefix, ReadOnlySpan<char> escapePostfix, ReadOnlySpan<byte> escapeTable, byte escapeLevel)
        {
            return EscapeEx.Escape(unescaped, escapePrefix, escapePostfix, escapeTable, escapeLevel);
        }

        /// <summary>
        /// Surrounds the string with quotes, trims existing.
        /// </summary>
        /// <param name="str"></param>
        public static string Quote(this string str)
        {
            ValueStringBuilder sb = new(stackalloc char[Math.Min(4096, str.Length + 2)]);
            sb.Append('\"');
            sb.Append(str.AsSpan().Trim('\"'));
            sb.Append('\"');
            return sb.ToString();
        }

        /// <summary>
        /// Surrounds the string with curly brackets, trims existing. 
        /// </summary>
        /// <param name="str"></param>
        public static string Symbolize(this string str)
        {
            ValueStringBuilder sb = new(stackalloc char[Math.Min(4096, str.Length + 2)]);
            sb.Append('{');
            sb.Append(str.AsSpan().TrimStart('{').TrimEnd('}'));
            sb.Append('}');
            return sb.ToString();
        }
    }
}
