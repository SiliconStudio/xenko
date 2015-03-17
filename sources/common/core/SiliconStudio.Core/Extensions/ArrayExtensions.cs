// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Core.Extensions
{
    public static class ArrayExtensions
    {
        // TODO: Merge this file with CollectionExtensions.cs

        // This is not really an extension method, maybe it should go somewhere else.
        public static bool ArraysEqual<T>(IList<T> a1, IList<T> a2, IEqualityComparer<T> comparer = null)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Count != a2.Count)
                return false;

            if (comparer == null)
                comparer = EqualityComparer<T>.Default;
            for (int i = 0; i < a1.Count; i++)
            {
                if (!comparer.Equals(a1[i], a2[i]))
                    return false;
            }

            return true;
        }

        public static bool ArraysReferenceEqual<T>(IList<T> a1, IList<T> a2) where T : class
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Count != a2.Count)
                return false;

            for (int i = 0; i < a1.Count; i++)
            {
                if (a1[i] != a2[i])
                    return false;
            }

            return true;
        }

        public static int ComputeHash<T>(this ICollection<T> data, IEqualityComparer<T> comparer = null)
        {
            unchecked
            {
                if (data == null)
                    return 0;

                if (comparer == null)
                    comparer = EqualityComparer<T>.Default;

                int hash = 17 + data.Count;
                int result = hash;
                foreach (T unknown in data)
                    result = result*31 + comparer.GetHashCode(unknown);
                return result;
            }
        }

        public static int ComputeHash<T>(this T[] data, IEqualityComparer<T> comparer = null)
        {
            unchecked
            {
                if (data == null)
                    return 0;

                if (comparer == null)
                    comparer = EqualityComparer<T>.Default;

                int hash = 17 + data.Length;
                int result = hash;
                foreach (T unknown in data)
                    result = result * 31 + comparer.GetHashCode(unknown);
                return result;
            }
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            var result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static T[] Concat<T>(this T[] array1, T[] array2)
        {
            var result = new T[array1.Length + array2.Length];

            array1.CopyTo(result, 0);
            array2.CopyTo(result, array1.Length);

            return result;
        }
    }
}