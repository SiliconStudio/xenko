// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Serialization.Converters;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Cubemap
{
    /// <summary>
    /// Puts a cubemap at the containing entity location. This could be dynamic (runtime rendered) or static (from a file).
    /// </summary>
    [DataConverter(AutoGenerate = true)]
    [DataContract("CubemapSourceComponent")]
    public class CubemapSourceComponent : EntityComponent
    {
        public static PropertyKey<CubemapSourceComponent> Key = new PropertyKey<CubemapSourceComponent>("Key", typeof(CubemapSourceComponent));

        [DataMemberIgnore]
        private Texture textureCube;

        /// <summary>
        /// Initializes a new instance of the <see cref="CubemapSourceComponent"/> class.
        /// </summary>
        public CubemapSourceComponent()
        {
            Size = 256;
            InfluenceRadius = 1.0f;
            InfinityCubemap = false;
            textureCube = null;
            NearPlane = 0.1f;
            FarPlane = 100.0f;
            MaxLod = 0;
            RenderTarget = null;
            RenderTargets = null;
        }

        public CubemapSourceComponent(Texture texture) : this()
        {
            textureCube = texture;
            if (texture != null)
                Size = texture.ViewWidth;
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
        /// The near plane of the cubemap.
        /// </summary>
        [DataMemberConvert]
        [DefaultValue(0.1f)]
        public float NearPlane { get; set; }

        /// <summary>
        /// The far plane of the cubemap.
        /// </summary>
        [DataMemberConvert]
        [DefaultValue(100.0f)]
        public float FarPlane { get; set; }

        /// <summary>
        /// The texture attached to this component.
        /// </summary>
        [DataMemberConvert]
        [DataMemberCustomSerializer]
        public Texture Texture
        {
            get
            {
                return textureCube;
            }
            set
            {
                textureCube = value;
                if (textureCube != null)
                    MaxLod = textureCube.MipLevels - 1;
            }
        }

        /// <summary>
        /// The maximum lod of the texture.
        /// </summary>
        [DataMemberIgnore]
        public int MaxLod { get; private set; }

        /// <summary>
        /// The render target of the cubemap.
        /// </summary>
        [DataMemberIgnore]
        public Texture RenderTarget { get; private set; }

        /// <summary>
        /// The render targets of the cubemap.
        /// </summary>
        [DataMemberIgnore]
        public Texture[] RenderTargets { get; private set; }

        /// <summary>
        /// The texture attached to this component.
        /// </summary>
        [DataMemberIgnore]
        public Texture DepthStencil { get; set; }

        /// <summary>
        /// Creates full view render targets on demand.
        /// </summary>
        public void CreateFullViewRenderTarget()
        {
            // TODO: check previous status to dispose the rendertarget?
            if (textureCube != null)
            {
                RenderTarget = textureCube.ToTextureView(ViewType.Full, 0, 0);
                DepthStencil = Texture.New2D(textureCube.GraphicsDevice, Size, Size, PixelFormat.D24_UNorm_S8_UInt, TextureFlags.DepthStencil, 6);
            }
        }

        /// <summary>
        /// Creates single view render targets on demand.
        /// </summary>
        public void CreateSingleViewRenderTargets()
        {
            // TODO: check previous status to dispose the rendertarget?
            if (textureCube != null)
            {
                RenderTargets = new[]
                {
                    textureCube.ToTextureView(ViewType.Single, 0, 0),
                    textureCube.ToTextureView(ViewType.Single, 1, 0),
                    textureCube.ToTextureView(ViewType.Single, 2, 0),
                    textureCube.ToTextureView(ViewType.Single, 3, 0),
                    textureCube.ToTextureView(ViewType.Single, 4, 0),
                    textureCube.ToTextureView(ViewType.Single, 5, 0)
                };
                DepthStencil = Texture.New2D(textureCube.GraphicsDevice, Size, Size, PixelFormat.D24_UNorm_S8_UInt, TextureFlags.DepthStencil);
            }
        }

        public override PropertyKey DefaultKey
        {
            get { return Key; }
        }
    }
}