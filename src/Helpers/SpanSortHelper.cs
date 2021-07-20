using System;
using System.Collections.Generic;

namespace StringFormatEx.Helpers
{
    internal static class SpanSortHelper
    {
        public static void InsertionSort<TItem, TComparer>(in this Span<TItem> span, in TComparer comparer)
            where TComparer : IComparer<TItem> // strong generic allows for comparer struct without a boxing allocation or defensive copy
        {
            for (int i = 1; i < span.Length; i++)
            {
                TItem p = span[i];
                int j = i - 1;
                while (j >= 0 && comparer.Compare(span[j], p) > 0)
                {
                    span[j + 1] = span[j--];
                }
                span[j + 1] = p;
            }
        }
    }
}
