// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Engine
{
    [DataConverter(AutoGenerate = true)]
    [DataContract("CubemapBlendComponent")]
    public class CubemapBlendComponent : EntityComponent
    {
        public static PropertyKey<CubemapBlendComponent> Key = new PropertyKey<CubemapBlendComponent>("Key", typeof(CubemapBlendComponent));

        /// <summary>
        /// Initializes a new instance of the <see cref="CubemapBlendComponent"/> class.
        /// </summary>
        public CubemapBlendComponent()
        {
            Size = 256;
            MaxBlendCount = 0;
            GenerateMips = false;
        }

        /// <summary>
        /// The size of the target cubemap.
        /// </summary>
        [DataMemberConvert]
        [DefaultValue(256)]
        public int Size { get; set; }

        /// <summary>
        /// The maximum number of cubemaps that can be blended. 0 means as much as possible.
        /// </summary>
        [DataMemberConvert]
        [DefaultValue(0)]
        public int MaxBlendCount { get; set; }

        /// <summary>
        /// Enables the computation of the cubemap.
        /// </summary>
        [DataMemberConvert]
        public bool Enabled { get; set; }

        /// <summary>
        /// Enables mip maps generation.
        /// </summary>
        [DataMemberConvert]
        [DefaultValue(false)]
        public bool GenerateMips { get; set; }

        /// <summary>
        /// The texture attached to this component.
        /// </summary>
        [DataMemberIgnore]
        public TextureCube Texture { get; set; }

        public override PropertyKey DefaultKey
        {
            get { return Key; }
        }
    }
}