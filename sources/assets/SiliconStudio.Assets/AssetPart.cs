// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using SiliconStudio.Core;

namespace SiliconStudio.Assets
{
    /// <summary>
    /// A part asset contained by an asset that is <see cref="IAssetPartContainer"/>.
    /// </summary>
    [DataContract("AssetPart")]
    public struct AssetPart
    {
        /// <summary>
        /// Initializes a new instance of <see cref="AssetPart"/> without a base.
        /// </summary>
        /// <param name="id">The asset identifier</param>
        public AssetPart(Guid id) : this()
        {
            Id = id;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="AssetPart"/> with a base.
        /// </summary>
        /// <param name="id">The asset identifier</param>
        /// <param name="baseId">The base asset identifier</param>
        public AssetPart(Guid id, Guid? baseId)
        {
            Id = id;
            BaseId = baseId;
        }

        /// <summary>
        /// Asset identifier.
        /// </summary>
        public Guid Id { get; internal set; }

        /// <summary>
        /// Base asset identifier.
        /// </summary>
        public Guid? BaseId { get; internal set; }
    }
}