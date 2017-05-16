// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Storage;

namespace SiliconStudio.Core.Serialization.Contents
{
    public interface IContentIndexMap : IDisposable
    {
        bool TryGetValue(string url, out ObjectId objectId);

        IEnumerable<KeyValuePair<string, ObjectId>> SearchValues(Func<KeyValuePair<string, ObjectId>, bool> predicate);

        bool Contains(string url);

        ObjectId this[string url] { get; set; }

        IEnumerable<KeyValuePair<string, ObjectId>> GetMergedIdMap();
    }
}
