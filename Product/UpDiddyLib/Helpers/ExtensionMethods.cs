using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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

        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source != null && toCheck != null && source.IndexOf(toCheck, comp) >= 0;
        }

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

        /// <summary>
        /// https://stackoverflow.com/questions/1415140/can-my-enums-have-friendly-names/1415187#1415187
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    DescriptionAttribute attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }
            return null;
        }
    }
}