// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// Performs a blend at the location of the containing entity. When enabled, takes the up to the MaxBlendCount-most important cubemaps and blends them.
    /// </summary>
    [DataConverter(AutoGenerate = true)]
    [DataContract("CubemapBlendComponent")]
    public sealed class CubemapBlendComponent : EntityComponent
    {
        public static PropertyKey<CubemapBlendComponent> Key = new PropertyKey<CubemapBlendComponent>("Key", typeof(CubemapBlendComponent));

        [DataMemberIgnore]
        private TextureCube textureCube;

        /// <summary>
        /// Initializes a new instance of the <see cref="CubemapBlendComponent"/> class.
        /// </summary>
        public CubemapBlendComponent()
        {
            Size = 256;
            MaxBlendCount = 0;
            MaxLod = 0;
            textureCube = null;
            RenderTargets = null;
            TextureKey = null;
        }

        /// <summary>
        /// Enables the computation of the cubemap.
        /// </summary>
        [DataMemberConvert]
        public bool Enabled { get; set; }

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
        /// The maximum lod of the texture.
        /// </summary>
        [DataMemberIgnore]
        public int MaxLod { get; private set; }

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
                if (textureCube != null)
                {
                    MaxLod = textureCube.Description.MipLevels - 1;
                    RenderTargets = new RenderTarget[6]
                    {
                        textureCube.ToRenderTarget(ViewType.Single, 0, 0),
                        textureCube.ToRenderTarget(ViewType.Single, 1, 0),
                        textureCube.ToRenderTarget(ViewType.Single, 2, 0),
                        textureCube.ToRenderTarget(ViewType.Single, 3, 0),
                        textureCube.ToRenderTarget(ViewType.Single, 4, 0),
                        textureCube.ToRenderTarget(ViewType.Single, 5, 0)
                    };
                }
            }
        }

        /// <summary>
        /// The parameter key the texture will be associated to.
        /// </summary>
        [DataMemberConvert]
        [DefaultValue(null)]
        public ParameterKey<Texture> TextureKey { get; set; }

        /// <summary>
        /// The render targets of the cubemap.
        /// </summary>
        [DataMemberIgnore]
        public RenderTarget[] RenderTargets { get; private set; }

        public override PropertyKey DefaultKey
        {
            get { return Key; }
        }
    }
}