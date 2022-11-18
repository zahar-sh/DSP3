using System;
using System.Collections.Generic;
using System.Windows;

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

        public static IEnumerable<Vector> AsPoints(this IEnumerable<double> source)
        {
            return source.SelectWithIndex((i, v) => new Vector(i, v));
        }
    }
}
