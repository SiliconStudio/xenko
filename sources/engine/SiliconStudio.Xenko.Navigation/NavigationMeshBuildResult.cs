// Copyright (c) 2017 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

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
    }
}