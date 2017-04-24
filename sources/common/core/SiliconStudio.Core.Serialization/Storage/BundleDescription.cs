// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;

namespace SiliconStudio.Core.Storage
{
    /// <summary>
    /// Description of a bundle: header, dependencies, objects and assets.
    /// </summary>
    public class BundleDescription
    {
        public BundleOdbBackend.Header Header { get; set; }

        public List<string> Dependencies { get; private set; }
        public List<ObjectId> IncrementalBundles { get; private set; }
        public List<KeyValuePair<ObjectId, BundleOdbBackend.ObjectInfo>> Objects { get; private set; }
        public List<KeyValuePair<string, ObjectId>> Assets { get; private set; }

        public BundleDescription()
        {
            Dependencies = new List<string>();
            IncrementalBundles = new List<ObjectId>();
            Objects = new List<KeyValuePair<ObjectId, BundleOdbBackend.ObjectInfo>>();
            Assets = new List<KeyValuePair<string, ObjectId>>();
        }
    }
}
