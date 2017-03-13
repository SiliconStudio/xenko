// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Navigation
{
    /// <summary>
    /// Layer containing built tiles for a single <see cref="NavigationAgentSettings"/>
    /// </summary>
    public class NavigationMeshLayer
    {
        // Backing field of Tiles
        internal Dictionary<Point, NavigationMeshTile> TilesInternal = new Dictionary<Point, NavigationMeshTile>();

        /// <summary>
        /// Contains all the built tiles mapped to their tile coordinates
        /// </summary>
        public IReadOnlyDictionary<Point, NavigationMeshTile> Tiles => TilesInternal;

        /// <summary>
        /// Tries to find a built tile inside this layer
        /// </summary>
        /// <param name="tileCoordinate">The coordinate of the tile</param>
        /// <returns>The found tile or null</returns>
        public NavigationMeshTile FindTile(Point tileCoordinate)
        {
            NavigationMeshTile foundTile;
            if (TilesInternal.TryGetValue(tileCoordinate, out foundTile))
                return foundTile;
            return null;
        }
    }
}