// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.ComponentModel;
using SiliconStudio.Assets;
using SiliconStudio.Core;
using SiliconStudio.Core.IO;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Paradox.Assets.Texture;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Materials.Nodes
{
    [ContentSerializer(typeof(DataContentSerializer<MaterialTextureNode>))]
    [DataContract("MaterialTextureNode")]
    public class MaterialTextureNode : MaterialNodeBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MaterialTextureNode()
            : this("", TextureCoordinate.Texcoord0, Vector2.One, Vector2.Zero)
        {
        }

        public MaterialTextureNode(string texturePath, int texcoordIndex, Vector2 scale, Vector2 offset)
            : this(texturePath, (TextureCoordinate)texcoordIndex, scale, offset)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialTextureNode"/> class.
        /// </summary>
        /// <param name="texturePath">Name of the texture.</param>
        /// <param name="texcoordIndex">Index of the texcoord.</param>
        /// <param name="scale">The scale.</param>
        /// <param name="offset">The offset.</param>
        public MaterialTextureNode(string texturePath, TextureCoordinate texcoordIndex, Vector2 scale, Vector2 offset)
        {
            if (texturePath == null)
                throw new ArgumentNullException("texturePath");

            TextureReference = new AssetReference<TextureAsset>(Guid.Empty, new UFile(texturePath));
            TexcoordIndex = texcoordIndex;
            Sampler = new NodeParameterSampler();
            Scale = scale;
            Offset = offset;
            AutoAssignKey = true;
            Key = null;
            UsedParameterKey = null;
        }

        /// <summary>
        /// The texture Reference.
        /// </summary>
        [DataMember(10)] 
        [DefaultValue(null)]
        public AssetReference<TextureAsset> TextureReference { get; set; }

        /// <summary>
        /// The texture coordinate used to sample the texture.
        /// </summary>
        [DataMember(30)]
        [DefaultValue(TextureCoordinate.Texcoord0)]
        public TextureCoordinate TexcoordIndex { get; set; }

        /// <summary>
        /// The sampler of the texture.
        /// </summary>
        [DataMemberIgnore]
        public NodeParameterSampler Sampler { get; set; }

        /// <summary>
        /// The texture filtering mode.
        /// </summary>
        [DataMember(41)]
        [DefaultValue(TextureFilter.Linear)]
        public TextureFilter Filtering
        {
            get
            {
                return Sampler.Filtering;
            }
            set
            {
                Sampler.Filtering = value;
            }
        }

        /// <summary>
        /// The texture address mode.
        /// </summary>
        [DataMember(42)]
        [DefaultValue(TextureAddressMode.Wrap)]
        public TextureAddressMode AddressModeU
        {
            get
            {
                return Sampler.AddressModeU;
            }
            set
            {
                Sampler.AddressModeU = value;
            }
        }

        /// <summary>
        /// The texture address mode.
        /// </summary>
        [DataMember(43)]
        [DefaultValue(TextureAddressMode.Wrap)]
        public TextureAddressMode AddressModeV
        {
            get
            {
                return Sampler.AddressModeV;
            }
            set
            {
                Sampler.AddressModeV = value;
            }
        }

        /// <summary>
        /// The scale of the texture (or the texture coordinates-  not defined yet).
        /// </summary>
        [DataMember(50)]
        public Vector2 Scale { get; set; }

        /// <summary>
        /// The offset in the texture coordinates.
        /// </summary>
        [DataMember(60)]
        public Vector2 Offset { get; set; }

        /// <summary>
        /// A flag stating if the paramater key is automatically assigned.
        /// </summary>
        [DataMember(70)]
        [DefaultValue(true)]
        public bool AutoAssignKey { get; set; }

        /// <summary>
        /// The desired parameter key.
        /// </summary>
        [DataMember(80)]
        [DefaultValue(null)]
        public ParameterKey<Graphics.Texture> Key { get; set; }

        /// <summary>
        /// The parameter key used in the shader.
        /// </summary>
        [DataMemberIgnore]
        public ParameterKey<Graphics.Texture> UsedParameterKey { get; set; }

        /// <summary>
        /// The name of the texture;
        /// </summary>
        public string TextureName
        {
            get
            {
                return TextureReference != null && TextureReference.Location != null ? TextureReference.Location : null;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Texture";
        }
    }
}
