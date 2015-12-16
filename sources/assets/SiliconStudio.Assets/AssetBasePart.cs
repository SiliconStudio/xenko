// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A base asset used as a part/composition.
    /// </summary>
    [DataContract("AssetBasePart")]
    public sealed class AssetBasePart
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AssetBasePart"/>
        /// </summary>
        public AssetBasePart() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="AssetBasePart"/>
        /// </summary>
        /// <param name="base"></param>
        public AssetBasePart(AssetBase @base)
        {
            Base = @base;
            InstanceIds = new List<Guid>();
        }

        /// <summary>
        /// The base asset (location and asset copy).
        /// </summary>
        public AssetBase Base { get; set; }

        /// <summary>
        /// The instance ids (ids of instance of <see cref="Base"/> asset.
        /// </summary>
        public List<Guid> InstanceIds { get; set; }
    }
}