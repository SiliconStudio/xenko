// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Effects.Composer
{
    /// <summary>
    /// Defines the common interface for a graphics composer responsible to compose the scene to a final render target.
    /// </summary>
    public interface IGraphicsComposer
    {
        /// <summary>
        /// Gets or sets the output of the composer.
        /// </summary>
        /// <value>The output.</value>
        IGraphicsComposerOutput Output { get; set; }
    }

    /// <summary>
    /// Defines the input of a layer.
    /// </summary>
    public interface IGraphicsLayerInput
    {
    }

    /// <summary>
    /// Defines a none input layer.
    /// </summary>
    /// <userdoc>
    /// The layer doesn't take any specific input.
    /// </userdoc>
    [DataContract("GraphicsLayerInputNone")]
    [Display("None")]
    public sealed class GraphicsLayerInputNone : IGraphicsLayerInput
    {
    }

    /// <summary>
    /// Defines a graphics layer input coming from the output of another layer.
    /// </summary>
    [DataContract("GraphicsLayerInputLayer")]
    [Display("Layer")]
    public sealed class GraphicsLayerInputLayer : IGraphicsLayerInput, IEquatable<GraphicsLayerInputLayer>
    {
        /// <summary>
        /// Gets a previous layer.
        /// </summary>
        public static GraphicsLayerInputLayer PreviousLayer()
        {
            return new GraphicsLayerInputLayer(-1);   
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsLayerInputLayer"/> class.
        /// </summary>
        public GraphicsLayerInputLayer()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsLayerInputLayer"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        public GraphicsLayerInputLayer(int index)
        {
            Index = index;
        }

        /// <summary>
        /// Gets or sets the layer index from the <see cref="GraphicsLayerCollection"/>
        /// </summary>
        /// <value>The layer index.</value>
        [DataMember(0)]
        public int Index { get; set; }

        public bool Equals(GraphicsLayerInputLayer other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Index == other.Index;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((GraphicsLayerInputLayer)obj);
        }

        public override int GetHashCode()
        {
            return Index;
        }
    }


    /// <summary>
    /// Defines the output of a <see cref="IGraphicsComposer"/>.
    /// </summary>
    public interface IGraphicsComposerOutput
    {
    }

    /// <summary>
    /// Output to the Direct (same as the output of the master layer).
    /// </summary>
    [DataContract("GraphicsComposerOutputDirect")]
    [Display("Direct")]
    public sealed class GraphicsComposerOutputDirect : IGraphicsComposerOutput
    {
    }

    /// <summary>
    /// Defines the type of rendering (Forward, Deferred...etc.)
    /// </summary>
    public interface IGraphicsRenderingMode
    {
        /// <summary>
        /// Gets or sets the effect mixin that will applied on top of the default Forward effect mixin.
        /// </summary>
        /// <value>The effect overrider.</value>
        /// <userdoc>
        /// The effect overrider allows to override a global effect used when rendering in forward mode. The overrider can
        /// provide an effect that will be 'mixin' after the forward effect, allowing to change the behavior of the default 
        /// forward effect.
        /// </userdoc>
        [Display("Effect Mixin")]
        [DefaultValue(null)]
        IGraphicsEffectMixinProvider EffectMixin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="EffectMixin"/> overrides completely the default effect of the
        /// rendering mode.
        /// </summary>
        /// <value><c>true</c> if [effect mixin overrides]; otherwise, <c>false</c>.</value>
        [Display("Effect Mixin Overrides?")]
        [DefaultValue(false)]
        bool EffectMixinOverrides { get; set; }
    }

    /// <summary>
    /// Defines the interface to provide an effect mixin for a <see cref="IGraphicsRenderingMode"/>.
    /// </summary>
    public interface IGraphicsEffectMixinProvider
    {
        /// <summary>
        /// Generates the shader source used for rendering.
        /// </summary>
        /// <returns>ShaderSource.</returns>
        ShaderSource GenerateShaderSource();
    }

    /// <summary>
    /// A forward rendering mode.
    /// </summary>
    [DataContract("GraphicsRenderingModeForward")]
    [Display("Forward")]
    public class GraphicsRenderingModeForward : IGraphicsRenderingMode
    {
        /// <summary>
        /// Gets or sets the effect mixin that will applied on top of the default Forward effect mixin.
        /// </summary>
        /// <value>The effect overrider.</value>
        /// <userdoc>
        /// The effect overrider allows to override a global effect used when rendering in forward mode. The overrider can
        /// provide an effect that will be 'mixin' after the forward effect, allowing to change the behavior of the default 
        /// forward effect.
        /// </userdoc>
        [DataMember(10)]
        public IGraphicsEffectMixinProvider EffectMixin { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="EffectMixin"/> overrides completely the default effect of the
        /// rendering mode.
        /// </summary>
        /// <value><c>true</c> if [effect mixin overrides]; otherwise, <c>false</c>.</value>
        [DataMember(20)]
        public bool EffectMixinOverrides { get; set; }
    }

    /// <summary>
    /// A graphics renderer.
    /// </summary>
    public interface IGraphicsRenderer
    {
    }

    /// <summary>
    /// A collection of <see cref="IGraphicsRenderer"/>.
    /// </summary>
    [DataContract("GraphicsRendererCollection")]
    public sealed class GraphicsRendererCollection : List<IGraphicsRenderer>
    {
    }

    /// <summary>
    /// A graphics layer.
    /// </summary>
    [DataContract("GraphicsLayer")]
    public class GraphicsLayer
    {
        // TODO: This may be shared with a graph composer

        public GraphicsLayer()
        {
            Enabled = true;
            Mode = new GraphicsRenderingModeForward();
            Input = new GraphicsLayerInputNone();
            Output = new GraphicsComposerOutputDirect();
            Renderers = new GraphicsRendererCollection();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="GraphicsLayer"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        [DataMember(0)]
        [DefaultValue(true)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the name of this layer.
        /// </summary>
        /// <value>The name.</value>
        [DataMember(10)]
        [DefaultValue(null)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the rendering mode.
        /// </summary>
        /// <value>The mode.</value>
        /// <userdoc>Defines the rendering mode (Forward, Deferred...etc.)</userdoc>
        [DataMember(20)]
        [NotNull]
        public IGraphicsRenderingMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the input this layer.
        /// </summary>
        /// <value>The input.</value>
        /// <userdoc>
        /// Defines the input of a layer. This can be the previous layer or a specific layer or a render target...etc.
        /// </userdoc>
        [DataMember(30)]
        [NotNull]
        public IGraphicsLayerInput Input { get; set; }

        /// <summary>
        /// Gets or sets the output of this layer.
        /// </summary>
        /// <value>The output.</value>
        /// <userdoc>
        /// Defines the output of a layer. This can be a local or shared render target.
        /// (This can be the previous layer or a specific layer or a render target...etc.)
        /// </userdoc>
        [DataMember(40)]
        [NotNull]
        public IGraphicsComposerOutput Output { get; set; }

        /// <summary>
        /// Gets the renderers that will be used to render this layer.
        /// </summary>
        /// <value>The renderers.</value>
        /// <userdoc>
        /// The renderers that will be used to render this layer.
        /// </userdoc>
        [DataMember(50)]
        [Category]
        public GraphicsRendererCollection Renderers { get; private set; }
    }

    /// <summary>
    /// A Collection of <see cref="GraphicsLayer"/>.
    /// </summary>
    [DataContract("GraphicsLayerCollection")]
    public sealed class GraphicsLayerCollection : List<GraphicsLayer>
    {
    }

    /// <summary>
    /// A Graphics Composer using layers.
    /// </summary>
    [DataContract("GraphicsComposerLayer")]
    [Display("Layer")]
    public sealed class GraphicsComposerLayer : GraphicsLayer, IGraphicsComposer
    {
        public GraphicsComposerLayer()
        {
            Layers = new GraphicsRendererCollection();
            Input = GraphicsLayerInputLayer.PreviousLayer();
        }

        /// <summary>
        /// Gets the layers used for composing a scene.
        /// </summary>
        /// <value>The layers.</value>
        [DataMember(100)]
        [Category]
        public GraphicsRendererCollection Layers { get; private set; }
    }
}