using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Windows.Controls
{
    internal class SizesCache
    {
        Dictionary<int, List<CachedSize>> cache;

        public SizesCache()
        {
            cache = new Dictionary<int, List<CachedSize>>();
        }

        public void AddOrChange(int level, double size)
        {
            List<CachedSize> levelList;
            if (cache.ContainsKey(level)) { levelList = cache[level]; }
            else
            {
                levelList = new List<CachedSize>(5);
                cache.Add(level, levelList);
            }

            CachedSize cachedSize = null;
            foreach (var s in levelList)
            {
                if (s.IsEqual(size))
                {
                    cachedSize = s;
                    break;
                }
            }

            if (cachedSize == null)
            {
                // if list is full, replace item with lowest count, to give other items a chance
                if (levelList.Count > 4)
                {
                    cachedSize = new CachedSize { OccuranceCounter = int.MaxValue, Size = size };
                    int indexToReplace = 0;
                    int smallestCounter = int.MaxValue;
                    for (int i = 0; i < 5; i++)
                    {
                        if (levelList[i].OccuranceCounter < smallestCounter) indexToReplace = i;
                    }
                    levelList[indexToReplace].OccuranceCounter = 1;
                    levelList[indexToReplace].Size = size;
                    cachedSize = levelList[indexToReplace];
                }
                else
                {
                    // add new size to list
                    cachedSize = new CachedSize { OccuranceCounter = 1, Size = size };
                    levelList.Add(cachedSize);
                }
            }
            else
            {
                // prevent overflow
                if(cachedSize.OccuranceCounter == int.MaxValue)
                {
                    foreach (var s in levelList)
                    {
                        s.OccuranceCounter = s.OccuranceCounter / 2;
                    }
                }

                // count occurance up
                cachedSize.OccuranceCounter++;
            }
        }

        public bool ContainsItems(int level)
        {
            if (cache.ContainsKey(level))
            {
                return cache[level].Count > 0;
            }

            return false;
        }

        public void CleanUp(int level)
        {
            cache.Remove(level);
        }

        public double GetEstimate(int level)
        {
            if (cache.ContainsKey(level))
            {
                CachedSize maxUsedSize = new CachedSize { OccuranceCounter = 0 };
                foreach (var s in cache[level])
                {
                    if (maxUsedSize.OccuranceCounter < s.OccuranceCounter) maxUsedSize = s;
                }

                return maxUsedSize.Size;
            }

            return 0;
        }

        class CachedSize
        {
            public double Size { get; set; }
            public int OccuranceCounter { get; set; }

            public bool IsEqual(double size)
            {
                return Math.Abs(Size - size) < 1;
            }
        }
    }
}
