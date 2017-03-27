// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Navigation
{
    /// <summary>
    /// The result of building a navigation mesh
    /// </summary>
    public class NavigationMeshBuildResult
    {
        /// <summary>
        /// <c>true</c> if the build was successful
        /// </summary>
        public bool Success = false;

        /// <summary>
        /// The generated navigation mesh
        /// </summary>
        public NavigationMesh NavigationMesh;

        /// <summary>
        /// List of updated layers + tiles
        /// </summary>
        public List<NavigationMeshLayerUpdateInfo> UpdatedLayers = new List<NavigationMeshLayerUpdateInfo>();
    }

    /// <summary>
    /// Information about what tiles changes after building a navigation mesh
    /// </summary>
    public class NavigationMeshLayerUpdateInfo
    {
        /// <summary>
        /// The id of the group
        /// </summary>
        public Guid GroupId;

        /// <summary>
        /// Coordinates of the tiles that were updated
        /// </summary>
        public List<Point> UpdatedTiles = new List<Point>();
    }
}