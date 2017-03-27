// Copyright (c) 2016-2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Navigation
{
    /// <summary>
    /// Provides advanced settings to be passed to navigation mesh queries
    /// </summary>
    public struct NavigationQuerySettings
    {
        /// <summary>
        /// The default settings that are used when querying navigation meshes
        /// </summary>
        public static readonly NavigationQuerySettings Default = new NavigationQuerySettings
        {
            FindNearestPolyExtent = new Vector3(2.0f, 4.0f, 2.0f),
            MaxPathPoints = 1024
        };

        /// <summary>
        /// Used as the extend for the find nearest poly bounding box used when scanning for a polygon corresponding to the given starting/ending position. 
        /// Making this bigger will allow you to find paths that allow the entity to start further away or higher from the navigation mesh bounds for example
        /// </summary>
        public Vector3 FindNearestPolyExtent;

        /// <summary>
        /// The maximum number of path points used internally and also the maximum number of output points
        /// </summary>
        public int MaxPathPoints;
    }
}