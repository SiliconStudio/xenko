// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;

namespace SiliconStudio.Paradox.Diffs
{
    internal static class ThreeWayMergeOrdered
    {
        public static void Merge<T, TKey>(IList<T> result, IList<T> listBase, IList<T> list1, IList<T> list2, Func<T, TKey> keyExtractor, Func<T, T, bool> contentComparer, Action<ThreeWayConflictType, IList<T>[], int[], IList<T>> conflictResolver)
        {
            var keyComparer = Comparer<TKey>.Default;

            if (contentComparer == null)
                contentComparer = EqualityComparer<T>.Default.Equals;
            // For now, to simplify some code, just create an empty base list if there is none.
            if (listBase == null)
                listBase = new T[0];

            var lists = new[] { listBase, list1, list2 };
            var indices = new[] { 0, 0, 0 };    // current indices
            var reachedEnd = new bool[3];       // is enumeration finished?
            var keys = new TKey[3];             // extracted key
            var isMinKey = new bool[3];         // is it equal to minimum key this iteration? (=> needs to be processed)

            while (indices[0] < lists[0].Count || indices[1] < lists[1].Count || indices[2] < lists[2].Count)
            {
                bool minKeyDefined = false;
                TKey minKey = default(TKey);

                // Find minimum key this iteration
                for (int i = 0; i < 3; ++i)
                {
                    reachedEnd[i] = indices[i] >= lists[i].Count;
                    if (!reachedEnd[i])
                    {
                        keys[i] = keyExtractor(lists[i][indices[i]]);
                        if (!minKeyDefined || keyComparer.Compare(minKey, keys[i]) > 0)
                        {
                            minKeyDefined = true;
                            minKey = keys[i];
                        }
                    }
                }

                // Find which list has this minimum key in this iteration
                for (int i = 0; i < 3; ++i)
                {
                    isMinKey[i] = (!reachedEnd[i]) && (keyComparer.Compare(minKey, keys[i]) == 0);
                }

                if (!minKeyDefined || (!isMinKey[0] && !isMinKey[1] && !isMinKey[2]))
                    throw new InvalidOperationException();

                if (!isMinKey[0])
                {
                    // New content from either (or both) list1 and list2
                    if (isMinKey[1] && !isMinKey[2])
                    {
                        // Insertion from list1
                        result.Add(lists[1][indices[1]]);
                    }
                    else if (!isMinKey[1] && isMinKey[2])
                    {
                        // Insertion from list2
                        result.Add(lists[2][indices[2]]);
                    }
                    else if (isMinKey[1] && isMinKey[2])
                    {
                        // Insertion from both list1 and list 2, compare content
                        if (!contentComparer(lists[1][indices[1]], lists[2][indices[2]]))
                        {
                            // Different adds, CONFLICT!
                            conflictResolver(ThreeWayConflictType.Insertion1And2, lists.ToArray(), indices.ToArray(), result);
                            //throw new NotImplementedException();
                        }
                        else
                        {
                            result.Add(lists[1][indices[1]]);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
                else
                {
                    if (!isMinKey[1] && !isMinKey[2])
                    {
                        // Deleted from both sides
                        conflictResolver(ThreeWayConflictType.Deleted1And2, lists.ToArray(), indices.ToArray(), result);
                    }
                    else if (isMinKey[1] && isMinKey[2])
                    {
                        // Present everywhere, compare content
                        bool compare01 = contentComparer(lists[0][indices[0]], lists[1][indices[1]]);
                        bool compare02 = contentComparer(lists[0][indices[0]], lists[2][indices[2]]);
                        bool compare12 = contentComparer(lists[1][indices[1]], lists[2][indices[2]]);

                        if (compare01 && compare02)
                        {
                            // No changes
                            result.Add(lists[0][indices[0]]);
                        }
                        else if (compare12)
                        {
                            // Same changes
                            result.Add(lists[1][indices[1]]);
                        }
                        else if (!compare01 && !compare02)
                        {
                            // Both side diverged, CONFLICT!
                            conflictResolver(ThreeWayConflictType.Modified1And2, lists.ToArray(), indices.ToArray(), result);
                            //throw new NotImplementedException();
                        }
                        else if (!compare01 || !compare02)
                        {
                            // Only one side changed, take it
                            if (!compare01)
                                result.Add(lists[1][indices[1]]);
                            else if (!compare02)
                                result.Add(lists[2][indices[2]]);
                        }
                    }
                    else if (isMinKey[1] && !isMinKey[2])
                    {
                        // Present in list1 but not list2 anymore, compare content
                        if (!contentComparer(lists[0][indices[0]], lists[1][indices[1]]))
                        {
                            // Modifed on one side, deleted on the other, CONFLICT!
                            conflictResolver(ThreeWayConflictType.Modified1Deleted2, lists.ToArray(), indices.ToArray(), result);
                            //throw new NotImplementedException();
                        }
                    }
                    else if (!isMinKey[1] && isMinKey[2])
                    {
                        // Present in list2 but not list1 anymore, compare content
                        if (!contentComparer(lists[0][indices[0]], lists[2][indices[2]]))
                        {
                            // Modifed on one side, deleted on the other, CONFLICT!
                            conflictResolver(ThreeWayConflictType.Modified2Deleted1, lists.ToArray(), indices.ToArray(), result);
                            //throw new NotImplementedException();
                        }
                    }
                }

                // Advance iterator
                for (int i = 0; i < 3; ++i)
                {
                    if (isMinKey[i])
                        indices[i]++;
                }
            }
        }
    }
}