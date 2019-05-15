using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpDiddyLib.Helpers
{
    public static class ExtensionMethods
    {
        public static TimeSpan DateDiff(this DateTime dt, DateTime compare)
        {
            return compare - dt;
        }

        public static bool EqualsInsensitive(this string str, string value) => string.Equals(str, value, StringComparison.CurrentCultureIgnoreCase);

        public static bool ContainsKeyValue<TKey, TVal>(this Dictionary<TKey, TVal> dssdd,
                                   TKey expectedKey,
                                   TVal expectedValue) where TVal : IComparable
        {
            TVal actualValue;

            if (!dssdd.TryGetValue(expectedKey, out actualValue))
            {
                return false;
            }
            return actualValue.CompareTo(expectedValue) == 0;
        }

        public static IEnumerable<T> Except<T, TKey>(this IEnumerable<T> items, IEnumerable<T> other, Func<T, TKey> getKey)
        {
            return from item in items
                   join otherItem in other on getKey(item)
                   equals getKey(otherItem) into tempItems
                   from temp in tempItems.DefaultIfEmpty()
                   where ReferenceEquals(null, temp) || temp.Equals(default(T))
                   select item;
        }
    }
}