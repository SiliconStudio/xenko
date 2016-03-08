// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Graphics;

namespace SiliconStudio.Xenko.Rendering.ComputeEffect
{
    /// <summary>
    /// A compute effect based directly on a single compute shader.
    /// </summary>
    public class ComputeEffectShader : DrawEffect
    {
        private MutablePipelineState pipelineState = new MutablePipelineState();
        private bool pipelineStateDirty = true;

        public ComputeEffectShader(RenderContext context)
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

        protected override void PreDrawCore(RenderDrawContext context)
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

        protected override void DrawCore(RenderDrawContext context)
        {
            if (string.IsNullOrEmpty(ShaderSourceName))
                return;

            Parameters.Set(ComputeEffectShaderKeys.ThreadNumbers, ThreadNumbers);
            Parameters.Set(ComputeEffectShaderKeys.ComputeShaderName, ShaderSourceName);
            Parameters.Set(ComputeShaderBaseKeys.ThreadGroupCountGlobal, ThreadGroupCounts);

            if (pipelineStateDirty)
            {
                EffectInstance.UpdateEffect(GraphicsDevice);

                pipelineState.State.SetDefaults();
                pipelineState.State.RootSignature = EffectInstance.RootSignature;
                pipelineState.State.EffectBytecode = EffectInstance.Effect.Bytecode;
                pipelineState.Update(GraphicsDevice);
                pipelineStateDirty = false;
            }

            // Apply pipeline state
            context.CommandList.SetPipelineState(pipelineState.CurrentState);

            // Apply the effect
            EffectInstance.Apply(context.GraphicsContext);

            // Draw a full screen quad
            context.CommandList.Dispatch(ThreadGroupCounts.X, ThreadGroupCounts.Y, ThreadGroupCounts.Z);

            // Un-apply
            //throw new InvalidOperationException();
            //EffectInstance.Effect.UnbindResources(GraphicsDevice);
        }
    }
}