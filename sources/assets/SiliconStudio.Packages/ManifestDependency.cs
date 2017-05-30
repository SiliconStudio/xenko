// Copyright (c) 2016-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

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
