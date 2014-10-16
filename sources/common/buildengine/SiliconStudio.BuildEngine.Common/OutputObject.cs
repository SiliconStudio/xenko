// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Core.Storage;
using SiliconStudio.Core.Serialization.Assets;

namespace SiliconStudio.BuildEngine
{
    /// <summary>
    /// Build Output Object, as exposed by <see cref="EnumerableBuildStep"/>.
    /// </summary>
    public class OutputObject
    {
        public readonly ObjectUrl Url;
        public ObjectId ObjectId;
        public readonly HashSet<string> Tags;
        protected internal int Counter;
        protected internal Command Command;

        public OutputObject(ObjectUrl url, ObjectId objectId)
        {
            Url = url;
            ObjectId = objectId;
            Tags = new HashSet<string>();
        }
    }
}