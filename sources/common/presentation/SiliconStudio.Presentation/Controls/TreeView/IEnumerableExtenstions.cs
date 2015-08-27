using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Collections
{
    public static class IEnumerableExtensions
    {
        public static void AddOrInsert(this IList list, int index, object data)
        {
            if (index < list.Count)
            {
                list.Insert(index, data);
            }
            else
            {
                list.Add(data);
            }
        }

        public static int Count(this IEnumerable enumerable)
        {
            ICollection collection = enumerable as ICollection;
            if (collection != null)
            {
                return collection.Count;
            }

            int counter = 0;
            foreach (var item in enumerable)
            {
                counter++;
            }
            return counter;
        }

        public static object ElementAt(this IEnumerable enumerable, int index)
        {
            IList list = enumerable as IList;
            if (list != null)
            {
                return list[index];
            }

            int counter = 0;
            foreach (var item in enumerable)
            {
                if (counter == index) return item;
                counter++;
            }

            throw new ArgumentOutOfRangeException("A item at the specified index was not found.");
        }

    }
}
