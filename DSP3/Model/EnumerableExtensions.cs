using System;
using System.Collections.Generic;

namespace DSP3.Model
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TResult> SelectWithIndex<T, TResult>(this IEnumerable<T> source, Func<int, T, TResult> selector)
        {
            int i = 0;
            foreach (var item in source)
            {
                yield return selector(i, item);
                i++;
            }
        }
    }
}
