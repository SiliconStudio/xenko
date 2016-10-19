// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using SiliconStudio.Core;

namespace SiliconStudio.Xenko.Assets.Navigation
{
    /// <summary>
    /// Represents cached data for a single entity and it's static colliders
    /// </summary>
    [DataContract]
    internal class NavigationMeshCachedBuildObject
    {
        /// <summary>
        /// Guid of the entity
        /// </summary>
        public Guid Guid;

        /// <summary>
        /// Hash obtained with <see cref="NavigationMeshBuildUtils.HashEntityCollider"/>
        /// </summary>
        public int ParameterHash;

        /// <summary>
        /// Cached vertex data
        /// </summary>
        public NavigationMeshInputBuilder Data;
    }
}