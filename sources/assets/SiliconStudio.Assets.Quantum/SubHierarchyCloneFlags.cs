// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using SiliconStudio.Core;

namespace SiliconStudio.Assets.Quantum
{
    [Flags]
    public enum SubHierarchyCloneFlags
    {
        /// <summary>
        /// No specific flag.
        /// </summary>
        None = 0,
        /// <summary>
        /// Clean any reference to an <see cref="IIdentifiable"/> object that is external to the sub-hierarchy.
        /// </summary>
        CleanExternalReferences = 1,
        /// <summary>
        /// Generates new identifiers for any <see cref="IIdentifiable"/> object that is internal to the sub-hierarchy.
        /// </summary>
        GenerateNewIdsForIdentifiableObjects = 2,
        /// <summary>
        /// Do not apply overrides on the cloned sub-hierarchy.
        /// </summary>
        RemoveOverrides = 4,
    }
}
