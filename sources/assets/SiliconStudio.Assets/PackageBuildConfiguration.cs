// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Describes parameters for building a package
    /// </summary>
    [DataContract("PackageBuildConfiguration")]
    public sealed class PackageBuildConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PackageBuildConfiguration"/> class.
        /// </summary>
        public PackageBuildConfiguration()
        {
            Profiles = new Dictionary<string, PackageProfile>();
        }

        /// <summary>
        /// Gets the profiles.
        /// </summary>
        /// <value>The profiles.</value>
        public Dictionary<string, PackageProfile> Profiles { get; private set; }
    }
}
