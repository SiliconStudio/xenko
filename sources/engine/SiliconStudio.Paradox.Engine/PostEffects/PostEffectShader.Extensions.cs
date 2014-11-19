// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects.Modules;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.PostEffects
{
    /// <summary>
    /// Post effect using a an effect or shader file (either pdxfx or pdxsl).
    /// </summary>
    public class PostEffectShader : PostEffectBase
    {
        private readonly ParameterCollection parameters;

        private readonly InternalEffectInstance effectInstance;

        private readonly DynamicEffectCompiler effectCompiler;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostEffectShader"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="effectName">Name of the shader.</param>
        public PostEffectShader(PostEffectContext context, string effectName)
            : base(context, effectName)
        {
            if (effectName == null) throw new ArgumentNullException("effectName");

            // Setup this instance parameters
            parameters = new ParameterCollection();
            // As this is used by PostEffectBase, we just setup it here by default
            parameters.Set(TexturingKeys.Sampler, GraphicsDevice.SamplerStates.LinearClamp);

            // Setup the effect compiler
            effectInstance = new InternalEffectInstance(parameters);
            effectCompiler = new DynamicEffectCompiler(context.Services, effectName);
        }

        /// <summary>
        /// Gets the name of the effect.
        /// </summary>
        public string EffectName
        {
            get
            {
                return effectCompiler.EffectName;
            }
        }

        /// <summary>
        /// Gets the effect parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public ParameterCollection Parameters
        {
            get
            {
                return parameters;
            }
        }

        protected override void DrawCore()
        {
            // Dynamically update/compile the effect based on the current parameters.
            effectCompiler.Update(effectInstance);

            // Draw a full screen quad
            GraphicsDevice.DrawQuad(effectInstance.Effect, Parameters);
        }

        /// <summary>
        /// Internal class used for dynamic effect compilation.
        /// </summary>
        private class InternalEffectInstance : DynamicEffectInstance
        {
            private readonly ParameterCollection parameters;

            public InternalEffectInstance(ParameterCollection parameters)
            {
                this.parameters = parameters;
            }

            protected internal override void FillParameterCollections(IList<ParameterCollection> parameterCollections)
            {
                parameterCollections.Add(parameters);
            }
        }
    }
}