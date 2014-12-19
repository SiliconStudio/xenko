// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Assets.Materials.ComputeColors;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Assets.Materials
{
    public interface INodeParameter
    {
    }

    [DataContract("NodeParameter")]
    public abstract class NodeParameter : INodeParameter
    {
    }

    [DataContract("NodeParameterTexture")]
    public class NodeParameterTexture : NodeParameter
    {
        public NodeParameterTexture()
        {
            Texture = new MaterialTextureComputeColor();
        }

        public MaterialTextureComputeColor Texture { get; private set; }
    }

    [DataContract()]
    public abstract class NodeParameterValue<T> : INodeParameter
    {
        [DataMember(10)]
        public T Value { get; set; }
    }

    [DataContract("NodeStringParameter")]
    public class NodeStringParameter : NodeParameterValue<string>
    {
        public NodeStringParameter()
            : base()
        {
            Value = string.Empty;
        }
    }

    [DataContract("NodeParameterFloat")]
    public class NodeParameterFloat : NodeParameterValue<float>
    {
        public NodeParameterFloat()
            : base()
        {
            Value = 0.0f;
        }
    }

    [DataContract("NodeParameterInt")]
    public class NodeParameterInt : NodeParameterValue<int>
    {
        public NodeParameterInt()
            : base()
        {
            Value = 0;
        }
    }

    [DataContract("NodeParameterFloat2")]
    public class NodeParameterFloat2 : NodeParameterValue<Vector2>
    {
        public NodeParameterFloat2()
            : base()
        {
            Value = Vector2.Zero;
        }
    }

    [DataContract("NodeParameterFloat3")]
    public class NodeParameterFloat3 : NodeParameterValue<Vector3>
    {
        public NodeParameterFloat3()
            : base()
        {
            Value = Vector3.Zero;
        }
    }

    [DataContract("NodeParameterFloat4")]
    public class NodeParameterFloat4 : NodeParameterValue<Vector4>
    {
        public NodeParameterFloat4()
            : base()
        {
            Value = Vector4.Zero;
        }
    }

    [DataContract("NodeParameterSampler")]
    public class NodeParameterSampler : INodeParameter
    {
        /// <summary>
        /// The texture filtering mode.
        /// </summary>
        [DataMember(10)]
        [DefaultValue(TextureFilter.Linear)]
        public TextureFilter Filtering { get; set; }

        /// <summary>
        /// The texture address mode.
        /// </summary>
        [DataMember(20)]
        [DefaultValue(TextureAddressMode.Wrap)]
        public TextureAddressMode AddressModeU { get; set; }

        /// <summary>
        /// The texture address mode.
        /// </summary>
        [DataMember(30)]
        [DefaultValue(TextureAddressMode.Wrap)]
        public TextureAddressMode AddressModeV { get; set; }

        public NodeParameterSampler()
        {
            Filtering = TextureFilter.Linear;
            AddressModeU = TextureAddressMode.Wrap;
            AddressModeV = TextureAddressMode.Wrap;
        }
    }
}
