// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.ComponentModel;

using SiliconStudio.Core;
using SiliconStudio.Core.Annotations;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Materials;
using SiliconStudio.Paradox.Rendering.Materials.ComputeColors;
using SiliconStudio.Paradox.Shaders;

namespace SiliconStudio.Paradox.Rendering.Materials
{
    /// <summary>
    /// A material blend layer
    /// </summary>
    [DataContract("MaterialBlendLayer")]
    [Display("Material Layer")]
    public class MaterialBlendLayer : IMaterialShaderGenerator
    {
        internal const string BlendStream = "matBlend";

        /// <summary>
        /// Initializes a new instance of the <see cref="MaterialBlendLayer"/> class.
        /// </summary>
        public MaterialBlendLayer()
        {
            Enabled = true;
            BlendMap = new ComputeTextureScalar();
            Overrides = new MaterialOverrides();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="MaterialBlendLayer"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        [DefaultValue(true)]
        [DataMember(10)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the name of this blend layer.
        /// </summary>
        /// <value>The name.</value>
        [DefaultValue(null)]
        [DataMember(20)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the material.
        /// </summary>
        /// <value>The material.</value>
        [DefaultValue(null)]
        [DataMember(30)]
        public Material Material { get; set; }

        /// <summary>
        /// Gets or sets the blend map.
        /// </summary>
        /// <value>The blend map.</value>
        [Display("Blend Map")]
        [DefaultValue(null)]
        [DataMember(40)]
        [DataMemberRange(0.0, 1.0, 0.01, 0.1, 2)]
        public IComputeScalar BlendMap { get; set; }

        /// <summary>
        /// Gets or sets the material overrides.
        /// </summary>
        /// <value>The overrides.</value>
        [DataMember(50)]
        [Category]
        [Display("Layer Overrides")]
        public MaterialOverrides Overrides { get; private set; }

        public virtual void Visit(MaterialGeneratorContext context)
        {
            // If not enabled, or Material or BlendMap are null, skip this layer
            if (!Enabled || Material == null || BlendMap == null || context.FindAsset == null)
            {
                return;
            }

            // Find the material from the reference
            var material = context.FindAsset(Material);
            if (material == null)
            {
                context.Log.Error("Unable to find material [{0}]", Material);
                return;
            }

            // TODO: Because we are not fully supporting Streams declaration in shaders, we have to workaround this limitation by using a dynamic shader (inline)
            // TODO: Handle MaterialOverrides

            // Push a layer for the sub-material
            context.PushOverrides(Overrides);
            context.PushLayer();

            // Generate the material shaders into the current context
            material.Visit(context);

            // Generate Vertex and Pixel surface shaders
            foreach (MaterialShaderStage stage in Enum.GetValues(typeof(MaterialShaderStage)))
                Generate(stage, context);

            // Pop the stack
            context.PopLayer();
            context.PopOverrides();
        }

        private void Generate(MaterialShaderStage stage, MaterialGeneratorContext context)
        {
            if (!context.HasSurfaceShaders(stage))
            {
                return;
            }

            // Blend setup for this layer
            context.SetStream(stage, BlendStream, BlendMap, MaterialKeys.BlendMap, MaterialKeys.BlendValue);

            // Generate a dynamic shader name
            // Create a mixin
            var shaderMixinSource = new ShaderMixinSource();
            shaderMixinSource.Mixins.Add(new ShaderClassSource("MaterialSurfaceStreamsBlend"));

            // Add all streams
            foreach (var stream in context.Streams[stage])
            {
                shaderMixinSource.AddCompositionToArray("blends", context.GetStreamBlendShaderSource(stream));
            }

            var materialBlendLayerMixin = context.GenerateSurfaceShader(stage);

            // Add the shader to the mixin
            shaderMixinSource.AddComposition("layer", materialBlendLayerMixin);

            context.ResetSurfaceShaders(stage);
            context.AddSurfaceShader(stage, shaderMixinSource);
        }
    }
}