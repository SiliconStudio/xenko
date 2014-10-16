// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Core.Serialization.Assets
{
    public interface IAssetIndexMap : IDisposable
    {
        bool TryGetValue(string url, out ObjectId objectId);

        IEnumerable<KeyValuePair<string, ObjectId>> SearchValues(Func<KeyValuePair<string, ObjectId>, bool> predicate);

        bool Contains(string url);

        ObjectId this[string url] { get; set; }

        IEnumerable<KeyValuePair<string, ObjectId>> GetMergedIdMap();
    }
}