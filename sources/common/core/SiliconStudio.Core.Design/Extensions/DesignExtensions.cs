// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SiliconStudio.Core.Extensions
{
    public static class DesignExtensions
    {
        /// <summary>
        /// Checks whether the IEnumerable represents a readonly data source.
        /// </summary>
        /// <param name="source">The IEnumerable to check.</param>
        /// <returns>Returns true if the data source is readonly, false otherwise.</returns>
        public static bool IsReadOnly(this IEnumerable source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var collection = source as ICollection<object>;
            if (collection != null)
                return collection.IsReadOnly;

            var list = source as IList;
            if (list != null)
                return list.IsReadOnly;

            return true;
        }

        /// <summary>
        /// Allow to directly iterate over an enumerator type.
        /// </summary>
        /// <typeparam name="T">Type of items provided by the enumerator.</typeparam>
        /// <param name="enumerator">Enumerator instance to iterate on.</param>
        /// <returns>Returns an enumerable that can be consume in a foreach statement.</returns>
        public static IEnumerable<T> Enumerate<T>(this IEnumerator<T> enumerator)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }

        /// <summary>
        /// Allow to directly iterate over an enumerator type.
        /// </summary>
        /// <typeparam name="T">Type of items provided by the enumerator.</typeparam>
        /// <param name="enumerator">Enumerator instance to iterate on. (subtype is casted to T)</param>
        /// <returns>Returns a typed enumerable that can be consume in a foreach statement.</returns>
        public static IEnumerable<T> Enumerate<T>(this IEnumerator enumerator)
        {
            while (enumerator.MoveNext())
                yield return (T)enumerator.Current;
        }

        public static IEnumerable<Tuple<T1, T2>> Zip<T1, T2>(this IEnumerable<T1> enumerable1, IEnumerable<T2> enumerable2)
        {
            if (enumerable1 == null) throw new ArgumentNullException("enumerable1");
            if (enumerable2 == null) throw new ArgumentNullException("enumerable2");

            using (IEnumerator<T1> enumerator1 = enumerable1.GetEnumerator())
            {
                using (IEnumerator<T2> enumerator2 = enumerable2.GetEnumerator())
                {
                    bool enumMoved = true;
                    while (enumMoved)
                    {
                        enumMoved = enumerator1.MoveNext();
                        bool enum2Moved = enumerator2.MoveNext();
                        if (enumMoved != enum2Moved)
                            throw new InvalidOperationException("Enumerables do not have the same number of items.");

                        if (enumMoved)
                        {
                            yield return Tuple.Create(enumerator1.Current, enumerator2.Current);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Iterates over all elements of source and their children recursively.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="childrenSelector">The children selector.</param>
        /// <returns></returns>
        public static IEnumerable<T> SelectDeep<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> childrenSelector)
        {
            var stack = new Stack<IEnumerable<T>>();
            stack.Push(source);
            while (stack.Count != 0)
            {
                var current = stack.Pop();
                if (current == null)
                    continue;

                foreach (T item in current)
                {
                    yield return item;
                    stack.Push(childrenSelector(item));
                }
            } 
        }

        public static IEnumerable<T> Distinct<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector)
        {
            return source.Distinct(new SelectorEqualityComparer<T, TKey>(selector));
        }

        public static bool Equals<T>(IEnumerable<T> a1, IEnumerable<T> a2, IEqualityComparer<T> comparer = null)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (comparer == null)
                comparer = EqualityComparer<T>.Default;

            var e1 = a1.GetEnumerator();
            var e2 = a2.GetEnumerator();

            while (true)
            {
                bool move1 = e1.MoveNext();
                bool move2 = e2.MoveNext();

                // End of enumeration, success!
                if (!move1 && !move2)
                    break;

                // One of the IEnumerable is shorter than the other?
                if (move1 ^ move2)
                    return false;

                if (!comparer.Equals(e1.Current, e2.Current))
                    return false;
            }

            return true;
        }

        public static bool SequenceEqual(this IEnumerable a1, IEnumerable a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            var e1 = a1.GetEnumerator();
            var e2 = a2.GetEnumerator();

            while (true)
            {
                bool move1 = e1.MoveNext();
                bool move2 = e2.MoveNext();

                // End of enumeration, success!
                if (!move1 && !move2)
                    return true;

                // One of the IEnumerable is shorter than the other?
                if (move1 ^ move2)
                    return false;

                // item from the first enum is non null and does not equal item from the second enum
                if (e1.Current != null && !e1.Current.Equals(e2.Current))
                    return false;

                // item from the second enum is non null and does not equal item from the first enum
                if (e2.Current != null && !e2.Current.Equals(e1.Current))
                    return false;
            }
        }

        public static bool AllEqual(this IEnumerable<object> values, out object value)
        {
            value = null;
            object firstNotNull = values.FirstOrDefault(x => x != null);
            // Either empty, or everything is null
            if (firstNotNull == null)
                return true;

            value = firstNotNull;

            return values.SkipWhile(x => x != firstNotNull).All(firstNotNull.Equals);
        }

        /// <summary>
        /// Returns the value corresponding to the given key. If the key is absent from the dictionary, it is added with the default value of the TValue type.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key of the value we are looking for.</param>
        /// <returns></returns>
        public static TValue GetOrCreateValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
        {
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            {
                value = new TValue();
                dictionary.Add(key, value);
            }
            return value;
        }

        public static int RemoveWhere<T>(this IList<T> list, Predicate<T> predicate)
        {
            int count = 0;
            var array = list.ToArray();
            for (var i = array.Length - 1; i >= 0; --i)
            {
                if (predicate(array[i]))
                    list.RemoveAt(i);

                ++count;
            }
            return count;
        }

        public static int RemoveWhere<T>(this ICollection<T> collection, Predicate<T> predicate)
        {
            int count = 0;
            foreach (var item in collection.ToArray().Where(x => predicate(x)))
            {
                collection.Remove(item);
                ++count;
            }
            return count;
        }

        class SelectorEqualityComparer<T, TKey> : IEqualityComparer<T>
        {
            Func<T, TKey> selector;

            public SelectorEqualityComparer(Func<T, TKey> selector)
            {
                this.selector = selector;
            }

            public bool Equals(T x, T y)
            {
                var keyX = selector(x);
                var keyY = selector(y);
                if (!typeof(T).GetTypeInfo().IsValueType)
                {
                    if (object.ReferenceEquals(keyX, null))
                        return object.ReferenceEquals(keyY, null);
                }

                return selector(x).Equals(selector(y));
            }

            public int GetHashCode(T obj)
            {
                var key = selector(obj);
                if (!typeof(T).GetTypeInfo().IsValueType && object.ReferenceEquals(key, null))
                    return 0;
                return key.GetHashCode();
            }
        }
    }
}
