using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLight.Extensions
{
    public static class EnumerableExtensions
    {
        public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TKey, TValue, TSource>(this 
            IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector)
        {
            var dict = new ConcurrentDictionary<TKey, TValue>();
            var anon = source.Select(k => new {key = keySelector.Invoke(k), value = valueSelector.Invoke(k)});
            anon.AsParallel().ForAll(k =>
            {
                dict.TryAdd(k.key, k.value);
            });
            return dict;
        }  
    }
}
