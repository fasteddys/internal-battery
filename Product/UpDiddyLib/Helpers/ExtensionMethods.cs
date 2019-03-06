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
    }
}