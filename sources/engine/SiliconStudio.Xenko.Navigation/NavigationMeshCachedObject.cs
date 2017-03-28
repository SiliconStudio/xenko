// Copyright (c) 2016-2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Navigation
{
    /// <summary>
    /// Represents cached data for a static collider component on an entity
    /// </summary>
    [DataContract]
    internal class NavigationMeshCachedObject
    {
        /// <summary>
        /// Guid of the collider
        /// </summary>
        public Guid Guid;

        /// <summary>
        /// Hash obtained with <see cref="NavigationMeshBuildUtils.HashEntityCollider"/>
        /// </summary>
        public int ParameterHash;

        /// <summary>
        /// Cached vertex data
        /// </summary>
        public NavigationMeshInputBuilder InputBuilder;

        /// <summary>
        /// List of infinite planes contained on this object
        /// </summary>
        public List<Plane> Planes = new List<Plane>();
    }
}