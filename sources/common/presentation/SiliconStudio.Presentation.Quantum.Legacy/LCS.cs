// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

namespace SiliconStudio.Presentation.Quantum.Legacy
{
    /// <summary>
    /// Algorithms to solve the Longest Common Subsequence problem.
    /// http://en.wikibooks.org/wiki/Algorithm_Implementation/Strings/Longest_common_subsequence
    /// </summary>
    // TODO: probably not useful anymore here, but could be moved to Core since it's an useful algorithm.
    public static class LCS
    {
        public static int[,] GetLCS<T>(IList<T> str1, IList<T> str2, IEqualityComparer<T> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default;

            int[,] table;
            GetLCSInternal(str1, str2, out table, comparer);
            return table;
        }

        public static List<T> ReadLCSFromBacktrack<T>(int[,] backtrack, IList<T> string1, IList<T> string2, IEqualityComparer<T> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default;

            var result = new List<T>();
            ReadLCSFromBacktrack(backtrack, string1, string2, string1.Count - 1, string2.Count - 1, result, comparer);

            return result;
        }

        public enum DiffAction
        {
            Add,
            Remove,
            Skip,
        }

        public struct Diff
        {
            public DiffAction Action { get; set; }
            public int SourceIndex { get; set; }
        }

        public static void ApplyDiff<T>(IList<T> list1, IList<T> list2, List<Diff> differences, Func<T, T> copyItem = null)
        {
            int index = 0;
            foreach (var difference in differences)
            {
                switch (difference.Action)
                {
                    case DiffAction.Skip:
                        index++;
                        break;
                    case DiffAction.Add:
                        list1.Insert(index, copyItem != null ? copyItem(list2[difference.SourceIndex]) : list2[difference.SourceIndex]);
                        index++;
                        break;
                    case DiffAction.Remove:
                        list1.RemoveAt(index);
                        break;
                }
            }
        }

        public static List<Diff> GenerateDiff<T>(int[,] backtrack, IList<T> list1, IList<T> list2, IEqualityComparer<T> comparer = null)
        {
            if (comparer == null) comparer = EqualityComparer<T>.Default;

            var differences = new List<Diff>();
            GenerateDiff(backtrack, list1, list2, list1.Count - 1, list2.Count - 1, differences, comparer);

            return differences;
        }

        private static void GenerateDiff<T>(int[,] backtrack, IList<T> list1, IList<T> list2, int list1Position, int list2Position, List<Diff> differences, IEqualityComparer<T> comparer)
        {
            if (list1Position >= 0 && list2Position >= 0 && comparer.Equals(list1[list1Position], list2[list2Position]))
            {
                GenerateDiff(backtrack, list1, list2, list1Position - 1, list2Position - 1, differences, comparer);
                // No changes
                differences.Add(new Diff { Action = DiffAction.Skip });
            }
            else
            {
                if (list2Position >= 0 && (list1Position == -1 || backtrack[list1Position + 1, list2Position] >= backtrack[list1Position, list2Position + 1]))
                {
                    GenerateDiff(backtrack, list1, list2, list1Position, list2Position - 1, differences, comparer);
                    // Add list2[list2Position]
                    differences.Add(new Diff { Action = DiffAction.Add, SourceIndex = list2Position });
                }
                else if (list1Position >= 0 && (list2Position == -1 || backtrack[list1Position + 1, list2Position] < backtrack[list1Position, list2Position + 1]))
                {
                    GenerateDiff(backtrack, list1, list2, list1Position - 1, list2Position, differences, comparer);
                    // Remove list1[list1Position]
                    differences.Add(new Diff { Action = DiffAction.Remove });
                }
            }
        }

        private static void GetLCSInternal<T>(IList<T> list1, IList<T> list2, out int[,] matrix, IEqualityComparer<T> comparer)
        {
            matrix = null;

            if (list1.Count == 0 || list2.Count == 0)
            {
                return;
            }

            var table = new int[list1.Count + 1, list2.Count + 1];
            for (int i = 0; i < table.GetLength(0); i++)
            {
                table[i, 0] = 0;
            }
            for (int j = 0; j < table.GetLength(1); j++)
            {
                table[0, j] = 0;
            }

            for (int i = 1; i < table.GetLength(0); i++)
            {
                for (int j = 1; j < table.GetLength(1); j++)
                {
                    if (comparer.Equals(list1[i - 1], list2[j - 1]))
                        table[i, j] = table[i - 1, j - 1] + 1;
                    else
                    {
                        if (table[i, j - 1] > table[i - 1, j])
                            table[i, j] = table[i, j - 1];
                        else
                            table[i, j] = table[i - 1, j];
                    }
                }
            }

            matrix = table;
        }

        private static void ReadLCSFromBacktrack<T>(int[,] backtrack, IList<T> list1, IList<T> list2, int list1Position, int list2Position, ICollection<T> result, IEqualityComparer<T> comparer)
        {
            if ((list1Position < 0) || (list2Position < 0))
            {
            }
            else if (comparer.Equals(list1[list1Position], list2[list2Position]))
            {
                ReadLCSFromBacktrack(backtrack, list1, list2, list1Position - 1, list2Position - 1, result, comparer);
                result.Add(list1[list1Position]);
            }
            else
            {
                if (backtrack[list1Position, list2Position - 1] >= backtrack[list1Position - 1, list2Position])
                {
                    ReadLCSFromBacktrack(backtrack, list1, list2, list1Position, list2Position - 1, result, comparer);
                }
                else
                {
                    ReadLCSFromBacktrack(backtrack, list1, list2, list1Position - 1, list2Position, result, comparer);
                }
            }
        }
    }
}