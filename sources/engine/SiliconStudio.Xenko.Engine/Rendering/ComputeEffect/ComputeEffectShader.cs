// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics.Internals;

namespace SiliconStudio.Xenko.Rendering.ComputeEffect
{
    /// <summary>
    /// A compute effect based directly on a single compute shader.
    /// </summary>
    public class ComputeEffectShader : DrawEffect
    {
        public ComputeEffectShader(RenderContext context)
            : this(context, null)
        {
        }

        public ComputeEffectShader(RenderContext context, params ParameterCollection[] sharedParameterCollections)
            : base(context, null)
        {
            // Setup the effect compiler
            EffectInstance = new DynamicEffectInstance("ComputeEffectShader", Parameters);
            EffectInstance.Initialize(context.Services);

            ThreadNumbers = new Int3(1);
            ThreadGroupCounts = new Int3(1);

            SetDefaultParameters();
        }

        /// <summary>
        /// The current effect instance.
        /// </summary>
        public DynamicEffectInstance EffectInstance { get; private set; }

        /// <summary>
        /// Gets or sets the number of group counts the shader should be dispatched to.
        /// </summary>
        public Int3 ThreadGroupCounts { get; set; }

        /// <summary>
        /// Gets or sets the number of threads desired by thread group.
        /// </summary>
        public Int3 ThreadNumbers { get; set; }

        /// <summary>
        /// Gets or sets the name of the input compute shader file (.xksl)
        /// </summary>
        public string ShaderSourceName { get; set; }

        /// <summary>
        /// Sets the default parameters (called at constructor time and if <see cref="DrawEffect.Reset"/> is called)
        /// </summary>
        protected override void SetDefaultParameters()
        {
        }

        protected override void PreDrawCore(RenderContext context)
        {
            base.PreDrawCore(context);

            // Default handler for parameters
            UpdateParameters();
        }

        /// <summary>
        /// Updates the effect <see cref="ComputeEffectShader.Parameters" /> from properties defined in this instance. See remarks.
        /// </summary>
        protected virtual void UpdateParameters()
        {
        }

        protected override void DrawCore(RenderContext context)
        {
            if (string.IsNullOrEmpty(ShaderSourceName))
                return;

            EffectInstance.SetPermutationValue(ComputeEffectShaderKeys.ThreadNumbers, ThreadNumbers);
            EffectInstance.SetPermutationValue(ComputeEffectShaderKeys.ComputeShaderName, ShaderSourceName);
            Parameters.SetValueSlow(ComputeShaderBaseKeys.ThreadGroupCountGlobal, ThreadGroupCounts);

            // Apply the effect
            EffectInstance.Apply(GraphicsDevice);

            // Draw a full screen quad
            GraphicsDevice.Dispatch(ThreadGroupCounts.X, ThreadGroupCounts.Y, ThreadGroupCounts.Z);

            // Un-apply
            //throw new InvalidOperationException();
            //EffectInstance.Effect.UnbindResources(GraphicsDevice);
        }
    }
}