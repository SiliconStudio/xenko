// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// Common class used by both <see cref="PackageReference"/> and <see cref="PackageDependency"/>.
    /// </summary>
    [DataContract("PackageReferenceBase")]
    public abstract class PackageReferenceBase
    {
        /// <summary>
        /// Asset references that needs to be compiled even if not directly or indirectly referenced (useful for explicit code references).
        /// </summary>
        [DataMember(100)]
        public RootAssetCollection RootAssets { get; private set; } = new RootAssetCollection();
    }
}