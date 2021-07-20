using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace StringFormatEx
{
    internal readonly ref struct FormattingArgumentCollection<T>
    {
        private readonly IReadOnlyDictionary<string, T>? _dictionary;
        private readonly ReadOnlySpan<FormattingArgument> _orderedSpan;
        private readonly IFormatProvider? _provider;
        private readonly ICustomFormatter? _formatter;

        public FormattingArgumentCollection(IReadOnlyDictionary<string, T>? dictionary, IFormatProvider? provider)
        {
            _dictionary = dictionary;
            _orderedSpan = default;
            _provider = provider;
            _formatter = provider?.GetFormat(typeof(ICustomFormatter)) as ICustomFormatter;
        }

        public FormattingArgumentCollection(in ReadOnlySpan<FormattingArgument> orderedSpan, IFormatProvider? provider)
        {
            _dictionary = null;
            _orderedSpan = orderedSpan;
            _provider = provider;
            _formatter = provider?.GetFormat(typeof(ICustomFormatter)) as ICustomFormatter;
        }

        public bool TryGetValue(string key, out string? replacement)
        {
            if (_dictionary == null)
            {
                FormattingArgument dummy = new(key, null);
                
                int argumentIndex = _orderedSpan.BinarySearch(dummy, FormattingArgumentSymbolComparer.Default);
                if (argumentIndex >= 0)
                {
                    replacement = TryFormat(_orderedSpan[argumentIndex].Value);
                    return true;
                }
            }
            else
            {
                if (_dictionary.TryGetValue(key, out var value))
                {
                    replacement = TryFormat(in value);
                    return true;
                }
            }

            replacement = default;
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string? TryFormat(object? value)
        {
            return _formatter == null ? value?.ToString() : _formatter.Format("{0}", value, _provider);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string? TryFormat(in T? value)
        {
            return _formatter == null ? value?.ToString() : _formatter.Format("{0}", value, _provider);
        }
    }
}
