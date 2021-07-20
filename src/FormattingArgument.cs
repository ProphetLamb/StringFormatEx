using System;
using System.Collections.Generic;

namespace StringFormatEx
{
    public readonly struct FormattingArgument
    {
        public FormattingArgument(string symbol, object? value)
        {
            Symbol = symbol;
            Value = value;
        }

        public readonly string Symbol;

        public readonly object? Value;

        public void Deconstruct(out string symbol, out object? value)
        {
            symbol = Symbol;
            value = Value;
        }

        public static implicit operator FormattingArgument(ValueTuple<string, object?> tuple) => new(tuple.Item1, tuple.Item2);

        public static implicit operator FormattingArgument(Tuple<string, object?> tuple) => new(tuple.Item1, tuple.Item2);
    }

    public readonly struct FormattingArgumentSymbolComparer : IComparer<FormattingArgument>
    {
        public static readonly FormattingArgumentSymbolComparer Default = new();

        public int Compare(FormattingArgument x, FormattingArgument y)
        {
            return String.Compare(x.Symbol, y.Symbol, StringComparison.Ordinal);
        }
    }
}
