// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Collections.Generic;

using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Images
{
    /// <summary>
    /// Post effect using an <see cref="Effect"/> (either pdxfx or pdxsl).
    /// </summary>
    public class ImageEffect : ImageEffectBase
    {
        private readonly ParameterCollection parameters;

        private readonly InternalEffectInstance effectInstance;

        private readonly DynamicEffectCompiler effectCompiler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffect"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="effectName">Name of the shader.</param>
        public ImageEffect(ImageEffectContext context, string effectName)
            : base(context, effectName)
        {
            if (effectName == null) throw new ArgumentNullException("effectName");

            // Setup this instance parameters
            parameters = new ParameterCollection();

            // Setup the effect compiler
            effectInstance = new InternalEffectInstance(parameters);
            effectCompiler = new DynamicEffectCompiler(context.Services, effectName);

            // As this is used by PostEffectBase, we just setup it here by default
            parameters.Set(TexturingKeys.Sampler, GraphicsDevice.SamplerStates.LinearClamp);
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

        /// <summary>
        /// Reset all parameters to their default values.
        /// </summary>
        public virtual void Reset()
        {
            Parameters.Set(TexturingKeys.Sampler, GraphicsDevice.SamplerStates.LinearClamp);
        }

        protected override void PreDrawCore(string name)
        {
            base.PreDrawCore(name);

            // Default handler for parameters
            UpdateParameters();
        }

        /// <summary>
        /// Updates the effect <see cref="Parameters"/> from properties defined in this instance. See remarks.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Expecting less than 10 textures in input</exception>
        /// <remarks>
        /// By default, all the input textures will be remapped to <see cref="TexturingKeys.Texture0"/>...etc.
        /// </remarks>
        protected virtual void UpdateParameters()
        {
            // By default, we are copying all input textures to TexturingKeys.Texture#
            var count = InputCount;
            for (int i = 0; i < count; i++)
            {
                var texture = GetInput(i);
                if (i < TexturingKeys.DefaultTextures.Count)
                {
                    Parameters.Set(TexturingKeys.DefaultTextures[i], texture);
                }
                else
                {
                    throw new InvalidOperationException("Expecting less than 10 textures in input");
                }
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

            public override void FillParameterCollections(IList<ParameterCollection> parameterCollections)
            {
                parameterCollections.Add(parameters);
            }
        }
    }
}