// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Flags used by <see cref="AssetCloner.Clone"/>
    /// </summary>
    [Flags]
    public enum AssetClonerFlags
    {
        /// <summary>
        /// No special flags while cloning.
        /// </summary>
        None,

        /// <summary>
        /// Attached references will be cloned as <c>null</c>
        /// </summary>
        ReferenceAsNull = 1,

        /// <summary>
        /// Remove ids attached to item of collections when cloning
        /// </summary>
        RemoveItemIds = 2,

        /// <summary>
        /// Removes invalid objects
        /// </summary>
        RemoveUnloadableObjects = 4,
    }
}