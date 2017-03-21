// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details

namespace SiliconStudio.Packages
{
    /// <summary>
    /// Representation of a dependency in a package manifest.
    /// </summary>
    public class ManifestDependency
    {
        /// <summary>
        /// Name of package dependency.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Version of package dependency.
        /// </summary>
        public string Version { get; set; }
    }
}