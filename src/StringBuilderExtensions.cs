using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using StringFormatEx.Helpers;

namespace StringFormatEx
{
    public static partial class StringBuilderExtensions
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
    }
}
