// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

namespace SiliconStudio.Xenko.Assets.Navigation
{
    internal class NavigationMeshBuildCache
    {
        /// <summary>
        /// Maximum number of scenes to keep cached
        /// </summary>
        public const int MaxCachedBuilds = 4;

        /// <summary>
        /// Holds scenes which are already built and it's associated hashes and settings
        /// </summary>
        private readonly Dictionary<string, NavigationMeshBuildCacheBuild> builtScenes = new Dictionary<string, NavigationMeshBuildCacheBuild>();

        /// <summary>
        /// Ordered list of built scenes from least recent to most recent
        /// </summary>
        private readonly List<string> recentBuilds = new List<string>();

        public void AddBuild(string targetUrl, NavigationMeshBuildCacheBuild navigationMeshBuildCacheBuild)
        {
            builtScenes.Remove(targetUrl);
            recentBuilds.Remove(targetUrl);

            // Remove least recent build if there are no more available slots
            if (recentBuilds.Count > MaxCachedBuilds)
            {
                string buildToRemove = recentBuilds[0];
                recentBuilds.RemoveAt(0);
                builtScenes.Remove(buildToRemove);
            }

            builtScenes.Add(targetUrl, navigationMeshBuildCacheBuild);
            recentBuilds.Add(targetUrl);
        }

        public NavigationMeshBuildCacheBuild FindBuild(string targetUrl)
        {
            NavigationMeshBuildCacheBuild navigationMeshBuildCacheBuild;
            if (!builtScenes.TryGetValue(targetUrl, out navigationMeshBuildCacheBuild))
                return null;
            return navigationMeshBuildCacheBuild;
        }
    }
}
