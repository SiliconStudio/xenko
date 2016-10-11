// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Engine
{
    /// <summary>
    /// Layer containing built tiles for a single <see cref="NavigationAgentSettings"/>
    /// </summary>
    public class NavigationMeshLayer
    {
        /// <summary>
        /// Tiles generated for this layer
        /// </summary>
        public Dictionary<Point, NavigationMeshTile> Tiles = new Dictionary<Point, NavigationMeshTile>();

        /// <summary>
        /// Agent settings for generating this layer, only used at build time
        /// </summary>
        internal NavigationAgentSettings AgentSettings;
    }
}