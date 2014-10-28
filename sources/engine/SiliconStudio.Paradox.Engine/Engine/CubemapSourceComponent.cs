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
    [DataContract("CubemapSourceComponent")]
    public class CubemapSourceComponent : EntityComponent
    {
        public static PropertyKey<CubemapSourceComponent> Key = new PropertyKey<CubemapSourceComponent>("Key", typeof(CubemapSourceComponent));

        [DataMemberIgnore]
        private TextureCube textureCube;

        /// <summary>
        /// Initializes a new instance of the <see cref="CubemapSourceComponent"/> class.
        /// </summary>
        public CubemapSourceComponent()
        {
            Size = 256;
            InfluenceRadius = 1.0f;
            InfinityCubemap = false;
            GenerateMips = false;
            textureCube = null;
        }

        public CubemapSourceComponent(TextureCube texture)
        {
            InfluenceRadius = 1.0f;
            InfinityCubemap = false;
            GenerateMips = false;
            textureCube = texture;
            IsDynamic = false;
        }

        /// <summary>
        /// Enables the computation of the cubemap if this one is dynamic.
        /// </summary>
        [DataMemberConvert]
        public bool Enabled { get; set; }

        /// <summary>
        /// Enables runtime cubemap creation.
        /// </summary>
        [DataMemberConvert]
        public bool IsDynamic { get; set; }

        /// <summary>
        /// The size of the target cubemap if this one is dynamic.
        /// </summary>
        [DataMemberConvert]
        [DefaultValue(256)]
        public int Size { get; set; }

        /// <summary>
        /// Enables mip maps generation.
        /// </summary>
        [DataMemberConvert]
        [DefaultValue(false)]
        public bool GenerateMips { get; set; }

        /// <summary>
        /// The cubemap has no location.
        /// </summary>
        [DataMemberConvert]
        public bool InfinityCubemap { get; set; }

        /// <summary>
        /// The influence radius of the cubemap. 0 is infinity?
        /// </summary>
        [DataMemberConvert]
        [DefaultValue(1.0f)]
        public float InfluenceRadius { get; set; }

        /// <summary>
        /// The texture attached to this component.
        /// </summary>
        [DataMemberIgnore]
        public TextureCube Texture
        {
            get
            {
                return textureCube;
            }
            set
            {
                textureCube = value;
                // TODO: check previous status to dispose the rendertarget?
                if (IsDynamic && textureCube != null)
                {
                    RenderTarget = textureCube.ToRenderTarget(ViewType.Full, 0, 0);
                }
            }
        }

        /// <summary>
        /// The render target of the cubemap.
        /// </summary>
        [DataMemberIgnore]
        public RenderTarget RenderTarget { get; private set; }

        /// <summary>
        /// The texture attached to this component.
        /// </summary>
        [DataMemberIgnore]
        public DepthStencilBuffer DepthStencil { get; set; }

        public override PropertyKey DefaultKey
        {
            get { return Key; }
        }
    }
}