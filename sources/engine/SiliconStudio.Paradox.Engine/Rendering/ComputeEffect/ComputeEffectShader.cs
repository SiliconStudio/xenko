// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System.Collections.Generic;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics.Internals;

namespace SiliconStudio.Paradox.Rendering.ComputeEffect
{
    /// <summary>
    /// A compute effect based directly on a single compute shader.
    /// </summary>
    public class ComputeEffectShader : DrawEffect
    {
        /// <summary>
        /// The current effect instance.
        /// </summary>
        protected readonly DefaultEffectInstance EffectInstance;

        private readonly DynamicEffectCompiler effectCompiler;

        private readonly List<ParameterCollection> parameterCollections;

        private readonly List<ParameterCollection> appliedParameterCollections;
        private EffectParameterCollectionGroup effectParameterCollections;

        public ComputeEffectShader(RenderContext context)
            : this(context, null)
        {
        }

        public ComputeEffectShader(RenderContext context, params ParameterCollection[] sharedParameterCollections)
            : base(context, null)
        {
            parameterCollections = new List<ParameterCollection> { context.Parameters };
            if (sharedParameterCollections != null)
            {
                parameterCollections.AddRange(sharedParameterCollections);
            }
            parameterCollections.Add(Parameters);

            appliedParameterCollections = new List<ParameterCollection>();

            // Setup the effect compiler
            EffectInstance = new DefaultEffectInstance(parameterCollections);
            effectCompiler = new DynamicEffectCompiler(context.Services, "ComputeEffectShader");

            ThreadNumbers = new Int3(1);
            ThreadGroupCounts = new Int3(1);

            SetDefaultParameters();
        }

        /// <summary>
        /// Gets or sets the number of group counts the shader should be dispatched to.
        /// </summary>
        public Int3 ThreadGroupCounts { get; set; }

        /// <summary>
        /// Gets or sets the number of threads desired by thread group.
        /// </summary>
        public Int3 ThreadNumbers { get; set; }

        /// <summary>
        /// Gets or sets the name of the input compute shader file (.pdxsl)
        /// </summary>
        public string ShaderSourceName { get; set; }

        /// <summary>
        /// Gets the parameter collections used by this effect.
        /// </summary>
        /// <value>The parameter collections.</value>
        public List<ParameterCollection> ParameterCollections
        {
            get
            {
                return parameterCollections;
            }
        }

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

        protected void UpdateEffect()
        {
            // Dynamically update/compile the effect based on the current parameters.
            effectCompiler.Update(EffectInstance, null);
        }

        protected override void DrawCore(RenderContext context)
        {
            if (string.IsNullOrEmpty(ShaderSourceName))
                return;

            Parameters.Set(ComputeEffectShaderKeys.ThreadNumbers, ThreadNumbers);
            Parameters.Set(ComputeEffectShaderKeys.ComputeShaderName, ShaderSourceName);
            Parameters.Set(ComputeShaderBaseKeys.ThreadGroupCountGlobal, ThreadGroupCounts);

            UpdateEffect();

            if (effectParameterCollections == null || effectParameterCollections.Effect != EffectInstance.Effect)
            {
                appliedParameterCollections.Clear();
                if (context != null)
                {
                    appliedParameterCollections.Add(context.Parameters);
                }
                appliedParameterCollections.AddRange(parameterCollections);

                effectParameterCollections = new EffectParameterCollectionGroup(GraphicsDevice, EffectInstance.Effect, appliedParameterCollections);
            }

            // Apply the effect
            EffectInstance.Effect.Apply(GraphicsDevice, effectParameterCollections, false);

            // Draw a full screen quad
            GraphicsDevice.Dispatch(ThreadGroupCounts.X, ThreadGroupCounts.Y, ThreadGroupCounts.Z);

            // Un-apply
            EffectInstance.Effect.UnbindResources(GraphicsDevice);
        }
    }
}