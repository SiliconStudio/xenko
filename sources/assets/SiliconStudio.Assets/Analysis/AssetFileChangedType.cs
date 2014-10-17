// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Assets.Analysis
{
    /// <summary>
    /// Type of a change event for an asset.
    /// </summary>
    [Flags]
    public enum AssetFileChangedType
    {
        /// <summary>
        /// An asset was added to the disk
        /// </summary>
        Added = 1,

        /// <summary>
        /// The asset was deleted from the disk
        /// </summary>
        Deleted = 2,

        /// <summary>
        /// The asset is updated on the disk
        /// </summary>
        Updated = 4,

        /// <summary>
        /// The asset event mask (Added | Deleted | Updated).
        /// </summary>
        AssetEventMask = Added | Deleted | Updated,

        /// <summary>
        /// The asset import was modified on the disk
        /// </summary>
        SourceUpdated = 8,

        /// <summary>
        /// The asset import was deleted from the disk
        /// </summary>
        SourceDeleted = 16,

        /// <summary>
        /// The source event mask (SourceUpdated | SourceDeleted).
        /// </summary>
        SourceEventMask = SourceUpdated | SourceDeleted,
    }
}