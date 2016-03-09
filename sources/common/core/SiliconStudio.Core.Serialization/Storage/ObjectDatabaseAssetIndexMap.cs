// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Core.Serialization.Assets;

namespace SiliconStudio.Core.Storage
{
    /// <summary>
    /// Content Index Map implementation which regroups all the asset index maps of every loaded file backend and asset bundle backends.
    /// </summary>
    public class ObjectDatabaseAssetIndexMap : IAssetIndexMap
    {
        public Dictionary<string, ObjectId> values = new Dictionary<string, ObjectId>();

        public IAssetIndexMap WriteableAssetIndexMap { get; set; }

        /// <summary>
        /// Merges the values from the given asset index map.
        /// </summary>
        /// <param name="assetIndexMap">The asset index map to merge.</param>
        public void Merge(IAssetIndexMap assetIndexMap)
        {
            Merge(assetIndexMap.GetMergedIdMap());
        }

        /// <summary>
        /// Merges the values from the given assets.
        /// </summary>
        /// <param name="assets">The assets to merge.</param>
        public void Merge(IEnumerable<KeyValuePair<string, ObjectId>> assets)
        {
            lock (values)
            {
                foreach (var item in assets)
                {
                    values[item.Key] = item.Value;
                }
            }
        }

        /// <summary>
        /// Unmerges the values from the given assets.
        /// </summary>
        /// <param name="assets">The assets to merge.</param>
        public void Unmerge(IEnumerable<KeyValuePair<string, ObjectId>> assets)
        {
            lock (values)
            {
                foreach (var item in assets)
                {
                    values.Remove(item.Key);
                }
            }
        }

        public bool TryGetValue(string url, out ObjectId objectId)
        {
            lock (values)
            {
                return values.TryGetValue(url, out objectId);
            }
        }

        public IEnumerable<KeyValuePair<string, ObjectId>> SearchValues(Func<KeyValuePair<string, ObjectId>, bool> predicate)
        {
            lock (values)
            {
                return values.Where(predicate).ToArray();
            }
        }

        public bool Contains(string url)
        {
            lock (values)
            {
                return values.ContainsKey(url);
            }
        }

        public ObjectId this[string url]
        {
            get
            {
                lock (values)
                {
                    return values[url];
                }
            }
            set
            {
                lock (values)
                {
                    if (WriteableAssetIndexMap != null)
                        WriteableAssetIndexMap[url] = value;
                    values[url] = value;
                }
            }
        }

        public IEnumerable<KeyValuePair<string, ObjectId>> GetMergedIdMap()
        {
            lock (values)
            {
                return values.ToArray();
            }
        }

        public void Dispose()
        {
        }
    }
}