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

        private readonly DefaultEffectInstance effectInstance;

        private readonly DynamicEffectCompiler effectCompiler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageEffect"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="effectName">Name of the shader.</param>
        public ImageEffect(ImageEffectContext context, string effectName)
            : base(context)
        {
            if (effectName == null) throw new ArgumentNullException("effectName");

            // Setup this instance parameters
            parameters = new ParameterCollection();

            // Setup the effect compiler
            effectInstance = new DefaultEffectInstance(parameters);
            effectCompiler = new DynamicEffectCompiler(context.Services, effectName);

            // Setup default parameters
            SetDefaultParameters();
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

        public override void Reset()
        {
            base.Reset();
            SetDefaultParameters();
        }

        /// <summary>
        /// Sets the default parameters (called at constructor time and if <see cref="Reset"/> is called)
        /// </summary>
        protected virtual void SetDefaultParameters()
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
    }
}